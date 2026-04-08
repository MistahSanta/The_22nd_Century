#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using UnityEditor.Events;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// One-click setup for all interactions, HUD, and controls menu.
/// Run from menu: Tools > Setup Main Scene
/// </summary>
public class SceneSetup : MonoBehaviour
{
    [MenuItem("Tools/Setup Main Scene")]
    static void Setup()
    {
        SetupTimeMachine();
        SetupTrashGrabber();
        SetupPresentTrash();
        CreateHUD();
        CreateControlsMenu();
        FixSafetyFloor();
        SetupPlayerSafety();

        SetupCrosshair();
        SetupControllerMapping();
        SetupPromptTexts();
        SetupGameManager();
        SetupGunFull();
        SetupZombies();
        PlaceVisibleTrashOnRoad();
        MakeTimeMachineVisible();

        // Save the scene automatically
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("Main scene setup complete! Scene saved.");
    }

    static void SetupGameManager()
    {
        // Create GameManager if it doesn't exist
        GameObject gm = GameObject.Find("GameManager");
        if (gm == null)
        {
            gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
            gm.AddComponent<ControllerMapping>();
        }

        var gmScript = gm.GetComponent<GameManager>();
        if (gmScript == null) gmScript = gm.AddComponent<GameManager>();
        if (gm.GetComponent<ControllerMapping>() == null) gm.AddComponent<ControllerMapping>();

        // Bind world references - search all root objects including inactive
        var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var root in roots)
        {
            switch (root.name)
            {
                case "FutureWorld_Apocalypse":
                    gmScript.futureApocalypse = root; break;
                case "FutureWorld_Cleaner":
                    gmScript.futureCleaner = root; break;
                case "FutureWorld_GettingCleaner":
                    gmScript.futureGettingCleaner = root; break;
                case "FutureWorld_VeryClean":
                    gmScript.futureVeryClean = root; break;
                case "PresentWorld":
                    gmScript.presentWorld = root; break;
            }
        }

        // Set skybox materials
        var so = new SerializedObject(gmScript);
        SetMaterialProperty(so, "skyboxApocalypse", "Assets/SkySeries Freebie/DarkStorm.mat");
        SetMaterialProperty(so, "skyboxCleaner", "Assets/AllSkyFree/Overcast Low/AllSky_Overcast4_Low.mat");
        SetMaterialProperty(so, "skyboxGettingCleaner", "Assets/SkySeries Freebie/Cloudymorning.mat");
        SetMaterialProperty(so, "skyboxVeryClean", "Assets/SkySeries Freebie/CasualDay.mat");
        SetMaterialProperty(so, "skyboxPresent", "Assets/SkySeries Freebie/FluffballDay.mat");

        // Set gun and TM references
        GameObject gun = GameObject.Find("Gun");
        if (gun != null) so.FindProperty("gunObject").objectReferenceValue = gun;
        GameObject tm = GameObject.Find("TimeMachine");
        if (tm != null) so.FindProperty("timeMachineObject").objectReferenceValue = tm;

        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(gmScript);
        Debug.Log("GameManager created with all world references, skyboxes, gun & TM.");
    }

    static void SetMaterialProperty(SerializedObject so, string propName, string assetPath)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
        if (mat != null)
            so.FindProperty(propName).objectReferenceValue = mat;
        else
            Debug.LogWarning($"Skybox material not found: {assetPath}");
    }

    static void SetupGunFull()
    {
        GameObject gun = GameObject.Find("Gun");
        if (gun == null) { Debug.LogError("Gun not found!"); return; }

        // Add components if missing
        var gunScript = gun.GetComponent<GunScript>();
        if (gunScript == null) gunScript = gun.AddComponent<GunScript>();

        var interactable = gun.GetComponent<InteractableObjectScript>();
        if (interactable == null) interactable = gun.AddComponent<InteractableObjectScript>();

        var col = gun.GetComponent<BoxCollider>();
        if (col == null) col = gun.AddComponent<BoxCollider>();
        col.size = new Vector3(2, 2, 2);
        col.isTrigger = true;

        interactable.promptText = "Pick up Gun";

        // Bind InteractableObjectScript -> SetGunEquip
        interactable.runButtonClickFunction = new UnityEvent();
        UnityEventTools.AddVoidPersistentListener(interactable.runButtonClickFunction, gunScript.SetGunEquip);

        // Create gun_barrel if missing
        Transform barrel = gun.transform.Find("gun_barrel");
        if (barrel == null)
        {
            GameObject barrelObj = new GameObject("gun_barrel");
            barrelObj.transform.SetParent(gun.transform, false);
            barrelObj.transform.localPosition = new Vector3(0, 0.05f, 0.3f);
            barrel = barrelObj.transform;
        }

        // Create Bullet if missing
        GameObject bullet = GameObject.Find("Bullet");
        if (bullet == null)
        {
            bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bullet.name = "Bullet";
            bullet.transform.position = new Vector3(0, -100, 0);
            bullet.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            bullet.AddComponent<Rigidbody>().useGravity = false;
            bullet.AddComponent<BulletScript>();
            bullet.GetComponent<SphereCollider>().isTrigger = true;
            bullet.SetActive(false);
        }

        // Set GunScript references
        Camera mainCam = Camera.main;
        var so = new SerializedObject(gunScript);
        if (mainCam != null)
            so.FindProperty("main_camera").objectReferenceValue = mainCam.transform;
        so.FindProperty("bullet").objectReferenceValue = bullet;
        so.FindProperty("gun_barrel").objectReferenceValue = barrel;
        so.ApplyModifiedProperties();

        // FIX: Reset M1911 child model to local origin
        Transform m1911 = gun.transform.Find("M1911");
        if (m1911 != null)
        {
            m1911.localPosition = Vector3.zero;
            m1911.localRotation = Quaternion.identity;
            Debug.Log("M1911 position reset to local origin.");
        }

        Debug.Log("Gun fully set up with bullet, barrel, and interaction.");
    }

    static void SetupZombies()
    {
        // Find all zombies in FutureWorld_Apocalypse/Zombies
        GameObject zombiesParent = GameObject.Find("Zombies");
        if (zombiesParent == null) { Debug.Log("Zombies parent not found"); return; }

        int count = 0;
        foreach (Transform child in zombiesParent.transform)
        {
            if (child.GetComponent<ZombieScript>() == null)
                child.gameObject.AddComponent<ZombieScript>();
            if (child.GetComponent<Collider>() == null)
                child.gameObject.AddComponent<BoxCollider>();
            count++;
        }
        Debug.Log($"Zombies setup done: {count} zombies configured.");
    }

    static void SetupCrosshair()
    {
        if (GameObject.Find("CrosshairManager") != null) return;

        GameObject obj = new GameObject("CrosshairManager");
        obj.AddComponent<CrosshairUI>();
        Debug.Log("CrosshairUI created.");
    }

    static void SetupControllerMapping()
    {
        GameObject gm = GameObject.Find("GameManager");
        if (gm == null) return;
        if (gm.GetComponent<ControllerMapping>() == null)
            gm.AddComponent<ControllerMapping>();
        Debug.Log("ControllerMapping added to GameManager.");
    }

    static void SetupPromptTexts()
    {
        // Gun prompt - close range
        GameObject gun = GameObject.Find("Gun");
        if (gun != null)
        {
            var interactable = gun.GetComponent<InteractableObjectScript>();
            if (interactable == null) interactable = gun.GetComponentInChildren<InteractableObjectScript>();
            if (interactable != null)
            {
                interactable.promptText = "Pick up Gun";
                interactable.maxInteractDistance = 3f;
            }
        }

        // TimeMachine prompt - medium range
        GameObject tm = GameObject.Find("TimeMachine");
        if (tm != null)
        {
            var interactable = tm.GetComponent<InteractableObjectScript>();
            if (interactable != null)
            {
                interactable.promptText = "Travel through Time";
                interactable.maxInteractDistance = 5f;
            }
        }

        // TrashGrabber prompt
        GameObject grabber = GameObject.Find("TrashGrabber");
        if (grabber != null)
        {
            var interactable = grabber.GetComponent<InteractableObjectScript>();
            if (interactable != null)
            {
                interactable.promptText = "Pick up Grabber";
                interactable.maxInteractDistance = 5f;
            }
        }

        Debug.Log("Prompt texts set.");
    }

    static void MakeTimeMachineVisible()
    {
        // Delete old TimeMachine
        GameObject oldTm = GameObject.Find("TimeMachine");
        if (oldTm != null)
            Object.DestroyImmediate(oldTm);

        // Load the GLB model as prefab
        string modelPath = "Assets/TimeMachineModel/time_machine.glb";
        GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);

        // Place inside the Post_Apocalyptic_Building area
        Vector3 portalPos = new Vector3(3f, 0.5f, -3f); // On the road, not overlapping buildings

        GameObject tm;
        if (modelPrefab != null)
        {
            tm = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab);
            tm.name = "TimeMachine";
            tm.transform.position = portalPos;
            tm.transform.eulerAngles = new Vector3(-90f, 0f, -154.292f);
            tm.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f); // 1/10 scale
            Debug.Log("TimeMachine: loaded GLB model.");
        }
        else
        {
            // Fallback: create simple portal if model not found
            tm = new GameObject("TimeMachine");
            tm.transform.position = portalPos;

            GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            platform.name = "Platform";
            platform.transform.SetParent(tm.transform, false);
            platform.transform.localPosition = Vector3.zero;
            platform.transform.localScale = new Vector3(3f, 0.1f, 3f);

            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "EnergyCore";
            core.transform.SetParent(tm.transform, false);
            core.transform.localPosition = new Vector3(0, 1.5f, 0);
            core.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0f, 0.8f, 1f);
            mat.SetColor("_EmissionColor", new Color(0f, 0.8f, 1f) * 3f);
            mat.EnableKeyword("_EMISSION");
            core.GetComponent<MeshRenderer>().material = mat;
            platform.GetComponent<MeshRenderer>().material = mat;

            Debug.LogWarning("TimeMachine: GLB model not found at " + modelPath + ", using fallback.");
        }

        // Add interaction collider (covers the whole machine)
        BoxCollider interactCol = tm.AddComponent<BoxCollider>();
        interactCol.size = new Vector3(4f, 4f, 4f);
        interactCol.center = new Vector3(0, 2f, 0);
        interactCol.isTrigger = true;

        // Portal Light
        GameObject lightObj = new GameObject("PortalLight");
        lightObj.transform.SetParent(tm.transform, false);
        lightObj.transform.localPosition = new Vector3(0, 2f, 0);
        Light portalLight = lightObj.AddComponent<Light>();
        portalLight.type = LightType.Point;
        portalLight.range = 15f;
        portalLight.intensity = 4f;
        portalLight.color = new Color(0.2f, 0.6f, 1f);

        // Add scripts
        var tmScript = tm.AddComponent<TimeMachineScript>();
        var interactable = tm.AddComponent<InteractableObjectScript>();
        interactable.promptText = "Travel through Time";
        interactable.runButtonClickFunction = new UnityEvent();
        UnityEventTools.AddVoidPersistentListener(interactable.runButtonClickFunction, tmScript.ActivateTravel);

        var so = new SerializedObject(tmScript);
        so.FindProperty("portalLight").objectReferenceValue = portalLight;
        so.ApplyModifiedProperties();

        // Move TrashGrabber to PresentWorld (only in present)
        GameObject grabber = GameObject.Find("TrashGrabber");
        // Find PresentWorld even if inactive
        GameObject pw = null;
        foreach (GameObject root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (root.name == "PresentWorld") { pw = root; break; }
        }
        if (grabber != null && pw != null)
        {
            grabber.transform.SetParent(pw.transform, true);
            Debug.Log("TrashGrabber moved to PresentWorld.");
        }

        Debug.Log("TimeMachine setup complete, placed in building.");
    }

    static void FixSafetyFloor()
    {
        // Delete old SafetyFloor if exists
        GameObject old = GameObject.Find("SafetyFloor");
        if (old != null)
            Object.DestroyImmediate(old);

        // Create thick box instead of plane - impossible to fall through
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "SafetyFloor";
        floor.transform.position = new Vector3(0, -0.5f, 0);
        floor.transform.localScale = new Vector3(300, 1, 300);

        // Make invisible
        var renderer = floor.GetComponent<MeshRenderer>();
        if (renderer != null)
            renderer.enabled = false;

        Debug.Log("SafetyFloor replaced with thick box collider.");
    }

    static void SetupPlayerSafety()
    {
        GameObject player = GameObject.Find("Player");
        if (player == null) return;

        // Fix player scale - 0.7 makes camera too low
        player.transform.localScale = Vector3.one;

        // Set player position
        player.transform.position = new Vector3(-14.64f, 1.5f, -10.98f);

        // Set gun position - right in front of player, higher up so visible
        GameObject gun = GameObject.Find("Gun");
        if (gun != null)
        {
            gun.transform.position = new Vector3(-14f, 1.2f, -10f);
            gun.transform.localScale = new Vector3(3f, 3f, 3f);
        }

        // Fix CharacterController
        var cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.center = new Vector3(0, 1f, 0);
            cc.height = 2f;
            cc.radius = 0.5f;
            cc.slopeLimit = 60f;
            cc.stepOffset = 0.5f;
        }

        // THE KEY FIX: Raise the HeightOffset so camera is at eye level (1.6m)
        Transform rig = player.transform.Find("XRCardboardRig");
        if (rig != null)
        {
            // Reset rig local position
            rig.localPosition = new Vector3(0, 0, 0);

            Transform heightOffset = rig.Find("HeightOffset");
            if (heightOffset != null)
            {
                heightOffset.localPosition = new Vector3(0, 1.8f, 0);
                Debug.Log("HeightOffset set to 1.8m.");
            }
        }

        if (player.GetComponent<PlayerSafetyNet>() == null)
            player.AddComponent<PlayerSafetyNet>();

        if (player.GetComponent<PlayerInteraction>() == null)
            player.AddComponent<PlayerInteraction>();

        if (player.GetComponent<MouseLook>() == null)
            player.AddComponent<MouseLook>();

        if (player.GetComponent<ControllerDebug>() == null)
            player.AddComponent<ControllerDebug>();

        Debug.Log("PlayerSafetyNet added to Player.");
    }

    static void SetupTimeMachine()
    {
        // TimeMachine is now fully set up in MakeTimeMachineVisible()
        // Just verify it exists
        GameObject tm = GameObject.Find("TimeMachine");
        if (tm == null)
            Debug.LogWarning("TimeMachine not found - will be created by MakeTimeMachineVisible.");
        else
            Debug.Log("TimeMachine setup done.");
    }

    static void SetupTrashGrabber()
    {
        // Delete ALL old TrashGrabbers (including inactive ones)
        var allGrabbers = new System.Collections.Generic.List<GameObject>();
        foreach (GameObject root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                if (child != null && child.name == "TrashGrabber")
                    allGrabbers.Add(child.gameObject);
        foreach (var g in allGrabbers)
            if (g != null) Object.DestroyImmediate(g);

        // Create new one from OBJ model
        string modelPath = "Assets/GarbagePickerModel/Garbage_Picker.obj";
        GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);

        GameObject grabber;
        if (modelPrefab != null)
        {
            grabber = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab);
            grabber.name = "TrashGrabber";
            grabber.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            Debug.Log("TrashGrabber: loaded OBJ model.");
        }
        else
        {
            // Fallback: create simple cylinder
            grabber = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            grabber.name = "TrashGrabber";
            grabber.transform.localScale = new Vector3(0.1f, 0.5f, 0.1f);
            Debug.LogWarning("TrashGrabber: OBJ not found, using fallback.");
        }

        // Right next to TimeMachine so player sees it immediately after travel
        // TM is at (3, 0.5, -3), put picker at (5, 0.5, -2)
        grabber.transform.position = new Vector3(5f, 0.5f, -2f);
        grabber.transform.eulerAngles = new Vector3(90f, 0f, 0f);
        grabber.transform.localScale = new Vector3(0.006f, 0.006f, 0.006f); // Much smaller

        // Add components
        if (grabber.GetComponent<TrashGrabberScript>() == null)
            grabber.AddComponent<TrashGrabberScript>();
        if (grabber.GetComponent<InteractableObjectScript>() == null)
            grabber.AddComponent<InteractableObjectScript>();
        if (grabber.GetComponent<GrabberProximityDetector>() == null)
            grabber.AddComponent<GrabberProximityDetector>();
        if (grabber.GetComponent<BoxCollider>() == null)
        {
            var box = grabber.AddComponent<BoxCollider>();
            box.size = new Vector3(100f, 100f, 100f);
            box.isTrigger = true;
        }

        // Add bright blue glow light - very visible
        GameObject lightObj = new GameObject("GrabberGlow");
        lightObj.transform.SetParent(grabber.transform, false);
        lightObj.transform.localPosition = Vector3.zero;
        Light glow = lightObj.AddComponent<Light>();
        glow.type = LightType.Point;
        glow.color = new Color(0.2f, 0.5f, 1f);
        glow.range = 15f;
        glow.intensity = 5f;

        // Move to PresentWorld
        GameObject pw = null;
        foreach (GameObject root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            if (root.name == "PresentWorld") { pw = root; break; }
        if (pw != null)
            grabber.transform.SetParent(pw.transform, true);

        if (grabber == null) { Debug.LogError("TrashGrabber not found!"); return; }

        // Set up InteractableObjectScript
        var interactable = grabber.GetComponent<InteractableObjectScript>();
        var grabScript = grabber.GetComponent<TrashGrabberScript>();
        if (interactable != null)
        {
            interactable.promptText = "Pick up Garbage Picker";
            interactable.maxInteractDistance = 4f;
        }
        if (interactable != null && grabScript != null)
        {
            interactable.runButtonClickFunction = new UnityEvent();
            UnityEventTools.AddVoidPersistentListener(interactable.runButtonClickFunction, grabScript.SetEquipped);
        }

        // Set camera reference
        Camera mainCam = Camera.main;
        if (mainCam != null && grabScript != null)
        {
            var so = new SerializedObject(grabScript);
            so.FindProperty("mainCamera").objectReferenceValue = mainCam.transform;
            so.ApplyModifiedProperties();
        }

        // Add emission glow
        foreach (Renderer r in grabber.GetComponentsInChildren<Renderer>())
            foreach (Material mat in r.materials)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(0.15f, 0.4f, 0.9f) * 1.5f);
            }

        Debug.Log("TrashGrabber setup done with OBJ model.");
    }

    static void PlaceVisibleTrashOnRoad()
    {
        // Find PresentWorld
        GameObject pw = null;
        foreach (GameObject root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            if (root.name == "PresentWorld") { pw = root; break; }
        if (pw == null) return;

        bool wasActive = pw.activeSelf;
        pw.SetActive(true);

        // Place 10 visible glowing trash items near the road/TM area
        Vector3[] positions = new Vector3[] {
            new Vector3(1, 0.3f, -1),
            new Vector3(3, 0.3f, 1),
            new Vector3(-1, 0.3f, -4),
            new Vector3(5, 0.3f, -6),
            new Vector3(7, 0.3f, -2),
            new Vector3(-3, 0.3f, 2),
            new Vector3(2, 0.3f, -8),
            new Vector3(8, 0.3f, -5),
            new Vector3(0, 0.3f, 3),
            new Vector3(6, 0.3f, 0)
        };

        string[] trashNames = { "Trash Bag", "Bottle", "Can", "Debris", "Wrapper",
                                "Trash Bag", "Bottle", "Can", "Debris", "Wrapper" };

        int created = 0;
        for (int i = 0; i < positions.Length; i++)
        {
            string name = $"VisibleTrash_{i}";
            // Skip if exists
            bool exists = false;
            foreach (Transform child in pw.GetComponentsInChildren<Transform>(true))
                if (child.name == name) { exists = true; break; }
            if (exists) continue;

            // Create trash object - use cylinder/sphere for variety
            GameObject trash;
            if (i % 3 == 0)
            {
                trash = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                trash.transform.localScale = new Vector3(0.15f, 0.3f, 0.15f);
            }
            else if (i % 3 == 1)
            {
                trash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                trash.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
            }
            else
            {
                trash = GameObject.CreatePrimitive(PrimitiveType.Cube);
                trash.transform.localScale = new Vector3(0.3f, 0.2f, 0.3f);
            }

            trash.name = name;
            trash.transform.SetParent(pw.transform, false);
            trash.transform.position = positions[i];
            trash.transform.eulerAngles = new Vector3(Random.Range(0,20), Random.Range(0,360), Random.Range(0,20));

            // Brown/dirty color with red glow
            var renderer = trash.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(0.4f, 0.25f, 0.1f);
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(0.9f, 0.3f, 0.1f) * 0.8f);
                renderer.material = mat;
            }

            // Add light so it's easy to find
            GameObject lightObj = new GameObject("TrashLight");
            lightObj.transform.SetParent(trash.transform, false);
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(0.9f, 0.3f, 0.1f);
            l.range = 5f;
            l.intensity = 2f;

            // Collider
            var col = trash.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            // Add interaction
            var pickup = trash.AddComponent<TrashPickup>();
            var interactable = trash.AddComponent<InteractableObjectScript>();
            interactable.promptText = trashNames[i];
            interactable.maxInteractDistance = 4f;
            interactable.runButtonClickFunction = new UnityEvent();
            UnityEventTools.AddVoidPersistentListener(interactable.runButtonClickFunction, pickup.Collect);

            created++;
        }

        pw.SetActive(wasActive);
        Debug.Log($"Placed {created} visible trash items on road.");
    }

    static void SetupPresentTrash()
    {
        // GameObject.Find can't find inactive objects, search all root objects
        GameObject presentWorld = null;
        foreach (GameObject root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (root.name == "PresentWorld") { presentWorld = root; break; }
        }
        if (presentWorld == null) { Debug.LogError("PresentWorld not found!"); return; }

        // Temporarily activate PresentWorld to find children
        bool wasActive = presentWorld.activeSelf;
        presentWorld.SetActive(true);

        int trashCount = 0;

        // Find all mesh renderers in PresentWorld that could be trash
        // Look for objects from Mess Maker Free (bottles, cans, food, etc.)
        Transform[] allChildren = presentWorld.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            if (child == presentWorld.transform) continue;
            if (child.parent == presentWorld.transform) continue; // skip direct children (Map, etc.)

            string name = child.gameObject.name.ToLower();
            bool isTrash = name.Contains("bottle") || name.Contains("can") || name.Contains("crushed") ||
                           name.Contains("bone") || name.Contains("burger") || name.Contains("cheeseburger") ||
                           name.Contains("apple") || name.Contains("plate") || name.Contains("pottery") ||
                           name.Contains("mug") || name.Contains("glass") || name.Contains("fish") ||
                           name.Contains("chicken") || name.Contains("vodka") || name.Contains("wine") ||
                           name.Contains("beer") || name.Contains("soda") || name.Contains("pint");

            if (isTrash)
            {
                // Remove any Outline components (causes mesh read errors)
                Outline outline = child.GetComponent<Outline>();
                if (outline != null) Object.DestroyImmediate(outline);

                // Add TrashPickup if not already there
                if (child.GetComponent<TrashPickup>() == null)
                {
                    var pickup = child.gameObject.AddComponent<TrashPickup>();

                    // Add InteractableObjectScript
                    var interactable = child.GetComponent<InteractableObjectScript>();
                    if (interactable == null)
                        interactable = child.gameObject.AddComponent<InteractableObjectScript>();

                    interactable.runButtonClickFunction = new UnityEvent();
                    UnityEventTools.AddVoidPersistentListener(interactable.runButtonClickFunction, pickup.Collect);

                    // Add collider if missing
                    if (child.GetComponent<Collider>() == null)
                    {
                        var box = child.gameObject.AddComponent<BoxCollider>();
                        box.isTrigger = true;
                    }
                    else
                    {
                        child.GetComponent<Collider>().isTrigger = true;
                    }

                    trashCount++;
                }
            }
        }

        // Update GameManager total trash count
        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            var so = new SerializedObject(gm);
            so.FindProperty("totalTrashInPresent").intValue = 5; // Always need 5 to reach 100%
            so.ApplyModifiedProperties();
        }

        presentWorld.SetActive(wasActive);
        Debug.Log($"Present trash setup done. Found {trashCount} trash items.");
    }

    static void CreateHUD()
    {
        // Check if HUD already exists
        if (GameObject.Find("CleanlinessHUD") != null)
        {
            Debug.Log("HUD already exists, skipping.");
            return;
        }

        // Create World Space Canvas
        GameObject hudObj = new GameObject("CleanlinessHUD");
        Canvas canvas = hudObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        hudObj.AddComponent<CanvasScaler>();
        hudObj.AddComponent<GraphicRaycaster>();

        RectTransform canvasRect = hudObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(400, 100);
        canvasRect.localScale = new Vector3(0.005f, 0.005f, 0.005f);

        // Background panel
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(hudObj.transform, false);
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.6f);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(400, 100);

        // Progress bar background
        GameObject barBg = new GameObject("ProgressBarBG");
        barBg.transform.SetParent(panel.transform, false);
        Image barBgImg = barBg.AddComponent<Image>();
        barBgImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        RectTransform barBgRect = barBg.GetComponent<RectTransform>();
        barBgRect.anchoredPosition = new Vector2(0, 15);
        barBgRect.sizeDelta = new Vector2(350, 25);

        // Progress bar fill
        GameObject barFill = new GameObject("ProgressBarFill");
        barFill.transform.SetParent(barBg.transform, false);
        Image fillImg = barFill.AddComponent<Image>();
        fillImg.color = new Color(0.9f, 0.2f, 0.2f, 1f);
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillAmount = 0f;
        RectTransform fillRect = barFill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;

        // Percent text
        GameObject pctObj = new GameObject("PercentText");
        pctObj.transform.SetParent(panel.transform, false);
        TextMeshProUGUI pctText = pctObj.AddComponent<TextMeshProUGUI>();
        pctText.text = "0%";
        pctText.fontSize = 20;
        pctText.alignment = TextAlignmentOptions.Center;
        RectTransform pctRect = pctObj.GetComponent<RectTransform>();
        pctRect.anchoredPosition = new Vector2(0, 15);
        pctRect.sizeDelta = new Vector2(350, 25);

        // Status text
        GameObject statusObj = new GameObject("StatusText");
        statusObj.transform.SetParent(panel.transform, false);
        TextMeshProUGUI statusText = statusObj.AddComponent<TextMeshProUGUI>();
        statusText.text = "Future: Apocalypse";
        statusText.fontSize = 16;
        statusText.alignment = TextAlignmentOptions.Center;
        RectTransform statusRect = statusObj.GetComponent<RectTransform>();
        statusRect.anchoredPosition = new Vector2(0, -20);
        statusRect.sizeDelta = new Vector2(350, 40);

        // Add CleanlinessHUD script
        CleanlinessHUD hud = hudObj.AddComponent<CleanlinessHUD>();

        // Set references via SerializedObject
        var so = new SerializedObject(hud);
        so.FindProperty("progressBarFill").objectReferenceValue = fillImg;
        so.FindProperty("statusText").objectReferenceValue = statusText;
        so.FindProperty("percentText").objectReferenceValue = pctText;

        Camera mainCam = Camera.main;
        if (mainCam != null)
            so.FindProperty("playerCamera").objectReferenceValue = mainCam.transform;

        so.ApplyModifiedProperties();

        Debug.Log("HUD created.");
    }

    static void CreateControlsMenu()
    {
        if (GameObject.Find("ControlsMenu") != null)
        {
            Debug.Log("ControlsMenu already exists, skipping.");
            return;
        }

        // Create World Space Canvas
        GameObject menuObj = new GameObject("ControlsMenu");
        Canvas canvas = menuObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        menuObj.AddComponent<CanvasScaler>();
        menuObj.AddComponent<GraphicRaycaster>();

        RectTransform canvasRect = menuObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(500, 400);
        canvasRect.localScale = new Vector3(0.005f, 0.005f, 0.005f);

        // Menu panel (starts hidden)
        GameObject panel = new GameObject("MenuPanel");
        panel.transform.SetParent(menuObj.transform, false);
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.85f);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(500, 400);

        // Controls text
        GameObject textObj = new GameObject("ControlsText");
        textObj.transform.SetParent(panel.transform, false);
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.fontSize = 18;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.margin = new Vector4(20, 20, 20, 20);
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        // Add ControlsMenu script
        ControlsMenu menu = menuObj.AddComponent<ControlsMenu>();

        var so = new SerializedObject(menu);
        so.FindProperty("menuPanel").objectReferenceValue = panel;
        so.FindProperty("controlsText").objectReferenceValue = text;

        Camera mainCam = Camera.main;
        if (mainCam != null)
            so.FindProperty("playerCamera").objectReferenceValue = mainCam.transform;

        so.ApplyModifiedProperties();

        panel.SetActive(false); // Start hidden

        Debug.Log("ControlsMenu created.");
    }
}
#endif
