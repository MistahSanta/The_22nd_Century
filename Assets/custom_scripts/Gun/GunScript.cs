using UnityEngine;

public class GunScript : MonoBehaviour
{
    private bool gun_equip = false;
    private float equip_speed = 8f;
    private Transform main_camera;
    public GameObject bullet;
    public Transform gun_barrel;
    public ParticleSystem muzzle_flash;
    private Animator gun_animator;
    Light gunLight;

    void Start()
    {
        gun_animator = GetComponentInChildren<Animator>();

        // Add blue emission glow to gun model
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            foreach (Material mat in r.materials)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(0.15f, 0.4f, 0.9f) * 1.5f);
            }
        }

        // Add point light at gun center
        Bounds bounds = new Bounds(transform.position, Vector3.zero);
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            bounds.Encapsulate(r.bounds);

        GameObject lightObj = new GameObject("GunGlow");
        lightObj.transform.position = bounds.center;
        lightObj.transform.SetParent(transform, true);
        gunLight = lightObj.AddComponent<Light>();
        gunLight.type = LightType.Point;
        gunLight.color = new Color(0.2f, 0.5f, 1f);
        gunLight.range = 12f;
        gunLight.intensity = 4f;

        // Big BoxCollider for easy detection from all angles
        BoxCollider col = GetComponent<BoxCollider>();
        if (col == null) col = gameObject.AddComponent<BoxCollider>();
        col.center = transform.InverseTransformPoint(bounds.center);
        col.size = Vector3.one * 5f;
        col.isTrigger = true;


        // Also add self-detection (backup if raycast fails)
        if (GetComponent<GunProximityDetector>() == null)
            gameObject.AddComponent<GunProximityDetector>();
    }

    public bool IsEquipped() { return gun_equip; }

    public void SetGunEquip()
    {
        gun_equip = true;
        Debug.Log("Gun equipped!");

    // Directly grab the camera assigned to the local user
        if (LocalPlayerHolder.LocalCamera != null)
        {
            main_camera = LocalPlayerHolder.LocalCamera;
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

        GameObject b = Instantiate(bullet, gun_barrel != null ? gun_barrel.position : transform.position,
            Quaternion.Euler(90,0,0));
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
