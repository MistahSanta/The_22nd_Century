#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Editor tool to merge all scene variants into one main scene.
/// Run from menu: Tools > Merge Scenes Into One
/// </summary>
public class SceneMerger : MonoBehaviour
{
    [MenuItem("Tools/Merge Scenes Into One")]
    static void MergeScenes()
    {
        // Save current scene first
        EditorSceneManager.SaveOpenScenes();

        // Create new main scene
        Scene mainScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Scene paths and their parent names
        string[] scenePaths = new string[]
        {
            "Assets/Scenes/Apocalypse.unity",
            "Assets/Scenes/Cleaner_Apocalypse.unity",
            "Assets/Scenes/Getting_Cleaner_Apocalypse.unity",
            "Assets/Scenes/Very_Clean_Apocalypse.unity",
            "Assets/Scenes/Present.unity"
        };

        string[] parentNames = new string[]
        {
            "FutureWorld_Apocalypse",
            "FutureWorld_Cleaner",
            "FutureWorld_GettingCleaner",
            "FutureWorld_VeryClean",
            "PresentWorld"
        };

        // Objects to keep at root level (shared across worlds)
        HashSet<string> sharedObjects = new HashSet<string>
        {
            "Player", "Gun", "Directional Light", "SafetyFloor"
        };

        bool playerAdded = false;
        bool gunAdded = false;
        bool lightAdded = false;
        bool safetyFloorAdded = false;

        for (int i = 0; i < scenePaths.Length; i++)
        {
            // Load scene additively
            Scene loadedScene = EditorSceneManager.OpenScene(scenePaths[i], OpenSceneMode.Additive);

            // Create world parent in main scene
            GameObject worldParent = new GameObject(parentNames[i]);
            SceneManager.MoveGameObjectToScene(worldParent, mainScene);

            // Move all root objects
            GameObject[] rootObjects = loadedScene.GetRootGameObjects();
            foreach (GameObject obj in rootObjects)
            {
                SceneManager.MoveGameObjectToScene(obj, mainScene);

                if (obj.name == "Player" && !playerAdded)
                {
                    playerAdded = true;
                    continue; // Keep Player at root
                }
                else if (obj.name == "Player")
                {
                    Object.DestroyImmediate(obj); // Remove duplicate players
                    continue;
                }

                if (obj.name == "Gun" && !gunAdded)
                {
                    gunAdded = true;
                    continue; // Keep Gun at root
                }
                else if (obj.name == "Gun")
                {
                    Object.DestroyImmediate(obj);
                    continue;
                }

                if (obj.name == "Directional Light" && !lightAdded)
                {
                    lightAdded = true;
                    continue; // Keep first light at root
                }
                else if (obj.name == "Directional Light")
                {
                    Object.DestroyImmediate(obj);
                    continue;
                }

                if (obj.name == "SafetyFloor" && !safetyFloorAdded)
                {
                    safetyFloorAdded = true;
                    continue; // Keep first SafetyFloor at root
                }
                else if (obj.name == "SafetyFloor")
                {
                    Object.DestroyImmediate(obj);
                    continue;
                }

                // Parent everything else under the world parent
                obj.transform.SetParent(worldParent.transform, true);
            }

            // Deactivate all worlds except the first (Apocalypse)
            if (i > 0)
                worldParent.SetActive(false);

            // Close the loaded scene
            EditorSceneManager.CloseScene(loadedScene, true);
        }

        // Save as new main scene
        string mainScenePath = "Assets/Scenes/Main.unity";
        EditorSceneManager.SaveScene(mainScene, mainScenePath);

        // Update build settings
        EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(mainScenePath, true)
        };
        EditorBuildSettings.scenes = buildScenes;

        Debug.Log("Scene merge complete! Main scene saved to: " + mainScenePath);
        Debug.Log("Build settings updated with Main scene only.");
    }
}
#endif
