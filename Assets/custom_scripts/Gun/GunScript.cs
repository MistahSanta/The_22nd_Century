using UnityEngine;

public class GunScript : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip shootSound;
    private bool gun_equip = false;
    private float equip_speed = 8f;
    private Transform main_camera;
    public GameObject bullet;
    public Transform gun_barrel;
    public ParticleSystem muzzle_flash;
    private Animator gun_animator;
    Light gunLight;
    [Range(0f, 1f)] public float shootVolume = 1f;
    private AudioSource _audioSource;
    void Start()
    {
        gun_animator = GetComponentInChildren<Animator>();

        // No glow on gun — light beam guides player instead

        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.spatialBlend = 1f;  // 3D audio so other players hear it positionally
        _audioSource.rolloffMode = AudioRolloffMode.Linear;
        _audioSource.maxDistance = 30f;
        _audioSource.playOnAwake = false;

        // Also add self-detection (backup if raycast fails)
        // if (GetComponent<GunProximityDetector>() == null)
        //     gameObject.AddComponent<GunProximityDetector>();
    }

    public bool IsEquipped() { return gun_equip; }

    public void SetGunEquip()
    {
        gun_equip = true;
        Debug.Log("Gun equipped!");

        // Disable collider, so raycast doesn't accidently show popup menu 
        // while holding the gun 
        BoxCollider myCollider = GetComponent<BoxCollider>();
        myCollider.enabled = false;

    // Directly grab the camera assigned to the local user
        if (LocalPlayerHolder.GetLocalCamera() != null)
        {
            main_camera = LocalPlayerHolder.GetLocalCamera();
            gun_equip = true;
            Debug.Log("Gun attached to local player camera.");
        }
        else
        {
            // Safety fallback
            main_camera = Camera.main?.transform;
            gun_equip = true;
            Debug.LogWarning("LocalPlayerHolder not set, falling back to Camera.main");
        }


        if (gunLight != null) gunLight.enabled = false;
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            foreach (Material mat in r.materials)
            {
                mat.DisableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", Color.black);
            }
    }

    public void Fire()
    {
        if (bullet == null)
        {
            Debug.LogWarning("Gun: bullet not assigned!");
            return;
        }

        if (shootSound != null && _audioSource != null)
            _audioSource.PlayOneShot(shootSound, shootVolume);

        // Spawn bullet facing camera forward direction
        GameObject b = Instantiate(bullet, gun_barrel != null ? gun_barrel.position : transform.position,
            Quaternion.LookRotation(main_camera.forward));
        b.SetActive(true);
        Rigidbody rb = b.GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = main_camera.forward * 20f;
        if (gun_animator != null) gun_animator.SetTrigger("Fire");
        HapticFeedback.VibrateShoot();
        if (muzzle_flash != null) muzzle_flash.Play();
        Destroy(b, 4);
    }

    void LateUpdate()
    {
        if (!gun_equip) return;

        Vector3 target = main_camera.position + main_camera.forward * 0.5f
            + main_camera.right * 0.25f - main_camera.up * 0.25f;
        transform.position = Vector3.Lerp(transform.position, target, equip_speed * Time.deltaTime);
        // Rotate gun to face forward (away from player)
        transform.rotation = main_camera.rotation * Quaternion.Euler(0, 0, 0);

        bool shoot = ControllerMapping.Instance != null
            ? ControllerMapping.Instance.GetShootDown()
            : Input.GetKeyDown(KeyCode.H);
        if (shoot) Fire();
    }
}
