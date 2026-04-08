#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using UnityEditor.Events;

public class GunSetup : MonoBehaviour
{
    [MenuItem("Tools/Setup Gun Interaction")]
    static void Setup()
    {
        GameObject gun = GameObject.Find("Gun");
        if (gun == null) { Debug.LogError("Gun not found!"); return; }

        var gunScript = gun.GetComponent<GunScript>();
        var interactable = gun.GetComponent<InteractableObjectScript>();

        if (gunScript != null && interactable != null)
        {
            interactable.runButtonClickFunction = new UnityEvent();
            UnityEventTools.AddVoidPersistentListener(interactable.runButtonClickFunction, gunScript.SetGunEquip);
            Debug.Log("Gun interaction bound: InteractableObjectScript -> SetGunEquip");
        }

        // Save
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("Gun setup complete and saved!");
    }
}
#endif
