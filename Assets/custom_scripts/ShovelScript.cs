using UnityEngine;
using Fusion;

public class ShovelScript : NetworkBehaviour
{
    public Transform mainCamera;

    [Networked, OnChangedRender(nameof(OnEquippedChanged))]
    public PlayerRef EquippedBy { get; set; } = PlayerRef.None;

    bool isLocallyEquipped = false;
    NetworkTransform _nt;

    public bool IsAvailable => EquippedBy == PlayerRef.None;

    public override void Spawned()
    {
        _nt = GetComponent<NetworkTransform>();

        if (mainCamera == null)
        {
            var cam = Camera.main;
            if (cam != null) mainCamera = cam.transform;
            else Debug.LogWarning("[ShovelScript] mainCamera not assigned and Camera.main not found!");
        }
    }

    public void SetEquipped()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayShovelPickup();

        if (Runner == null || !IsAvailable)
        {
            Debug.Log("Shovel already taken!");
            return;
        }

        if (!HasStateAuthority)
        {
            RPC_RequestEquip(Runner.LocalPlayer);
            return;
        }

        DoEquip(Runner.LocalPlayer);
    }


    void DoEquip(PlayerRef player)
    {
        EquippedBy = player;
    }

    [Rpc(RpcSources.Proxies, RpcTargets.StateAuthority)]
    void RPC_RequestEquip(PlayerRef requestingPlayer)
    {
        if (!IsAvailable) return;
        DoEquip(requestingPlayer);
    }

    void OnEquippedChanged()
    {
        isLocallyEquipped = Runner != null && EquippedBy == Runner.LocalPlayer;

        // Disable NetworkTransform so Fusion stops overriding our LateUpdate position
        if (_nt != null) _nt.enabled = !isLocallyEquipped;

        if (isLocallyEquipped)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PickUpShovel();
                GameManager.Instance.EquipShovel();
            }

            foreach (Light l in GetComponentsInChildren<Light>())
                l.enabled = false;

            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                r.enabled = true; // Make sure it's visible immediately
                foreach (Material mat in r.materials)
                {
                    mat.DisableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", Color.black);
                }
            }

            Debug.Log("Shovel equipped by local player!");
        }
        else if (EquippedBy != PlayerRef.None)
        {
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
                r.enabled = false;

            Debug.Log($"Shovel taken by player {EquippedBy}");
        }
    }

    void LateUpdate()
    {
        if (!isLocallyEquipped || mainCamera == null) return;

        bool isActive = GameManager.Instance != null &&
                        GameManager.Instance.IsReady &&
                        GameManager.Instance.CurrentTool == GameManager.EquippedTool.Shovel;

        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = isActive;

        // Always update position regardless of isActive
        Vector3 target = mainCamera.position
            + mainCamera.forward * 0.5f
            + mainCamera.right * 0.25f
            - mainCamera.up * 0.3f;

        transform.position = Vector3.Lerp(transform.position, target, 8f * Time.deltaTime);
        transform.rotation = mainCamera.rotation;
    }
}