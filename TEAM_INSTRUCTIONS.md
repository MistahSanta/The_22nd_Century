# The 22nd Century - Update Notes

**Updated by: Yili | April 8, 2026**

---

## 1. What Yili Achieved

| Feature                                  | Script File                                                                        | Description                                                            |
| ---------------------------------------- | ---------------------------------------------------------------------------------- | ---------------------------------------------------------------------- |
| Accelerometer Jump/Crouch (Advanced Req) | `AccelerometerMovement.cs`                                                         | Detects physical jump/crouch via phone accelerometer                   |
| Haptic Vibration                         | `HapticFeedback.cs`                                                                | Phone vibrates on shoot (50ms), zombie hit (200ms), interact (30ms)    |
| Time Machine Travel                      | `TimeMachineScript.cs`                                                             | 3D model time machine, gaze to interact, travel Future ↔ Present       |
| Trash Pickup System                      | `TrashPickup.cs`, `TrashGrabberScript.cs`                                          | Pick up garbage picker first, then collect 5 trash to clean the future |
| World Switching + Cleanliness            | `GameManager.cs`                                                                   | Manages 5 worlds in 1 scene, tracks trash score, switches skybox       |
| Zombie AI (modified)                     | `ZombieScript.cs`                                                                  | Zombies chase player slowly, turn red on hit, game over on touch       |
| Gun Glow + Gaze Detection                | `GunScript.cs`, `GunProximityDetector.cs`                                          | Blue glow on gun, proximity gaze detection, first-person hold          |
| Crosshair + Gaze Prompts                 | `CrosshairUI.cs`, `InteractableObjectScript.cs`                                    | Center screen crosshair, world-space prompt on gaze                    |
| HUD Progress Bar                         | `CleanlinessHUD.cs`                                                                | Shows 0/5 trash collected, only visible in Present world               |
| Controls Menu                            | `ControlsMenu.cs`                                                                  | Press Menu button to see all button mappings                           |
| Controller Mapping                       | `ControllerMapping.cs`                                                             | Configurable button mapping in Inspector                               |
| Scene Merge Tool                         | `SceneMerger.cs` (Editor)                                                          | Merged 5 separate scenes into 1 Main scene                             |
| Auto Scene Setup                         | `SceneSetup.cs`, `GunSetup.cs` (Editor)                                            | One-click setup for all interactions and references                    |
| Proximity Gaze Detection                 | `PlayerInteraction.cs`, `GrabberProximityDetector.cs`, `TrashProximityDetector.cs` | Detects when player looks at interactable objects                      |
| Mouse Look (Editor)                      | `MouseLook.cs`                                                                     | Mouse camera control for editor testing, auto-disabled on phone        |
| Anti-Fall Safety                         | `PlayerSafetyNet.cs`                                                               | Teleports player back if they fall through the map                     |
| Controller Debug                         | `ControllerDebug.cs`                                                               | Debug tool to identify controller button names (disabled by default)   |

All scripts are located in `Assets/custom_scripts/`. Editor tools are in `Assets/Editor/`.

---

## 2. How to Download & Run

### Pull Latest Code

```bash
cd [your project folder]
git pull origin main
```

### Open in Unity

- Required version: **Unity 6000.3.4f1**
- Open the project folder via Unity Hub

### Open the Main Scene

- Navigate to **Assets/Scenes/Main.unity** and open it
- This is the merged scene containing all 5 world variants

### Run Setup (REQUIRED after every pull)

1. Click menu **Tools > Setup Main Scene** — wait until console shows "Main scene setup complete!"
2. Click menu **Tools > Setup Gun Interaction** — wait until console shows "Gun setup complete!"
3. Press **Ctrl+S** (or Cmd+S on Mac) to save the scene
4. Press **Play** to test

### Build to Android Phone

1. **File > Build Settings**
2. Platform: **Android**, Scenes in Build: **Main**
3. Click **Build and Run** with phone connected

---

## 3. Controller Mapping

| Button                | Action                                        |
| --------------------- | --------------------------------------------- |
| Joystick              | Move player                                   |
| Head movement         | Look around (VR) / Mouse (Editor)             |
| **A** (js11)          | Interact — pick up gun, travel, collect trash |
| **Top / H** (js7)     | Shoot gun                                     |
| **Y** (js5)           | Jump                                          |
| **Menu** (js9 / js13) | Toggle controls help menu                     |

To change mappings: Select **GameManager** object in Hierarchy > Inspector > **ControllerMapping** component.

---

## 4. Game Flow

1. **Start in Future (Apocalypse)** — dark, destroyed world with zombies
2. **Pick up Gun** — look at it (blue glow), press A
3. **Shoot Zombies** — press Top button, zombies turn red and die
4. **Walk to Time Machine** — look at it, press A to travel to Present
5. **Pick up Garbage Picker** — blue glow, near Time Machine, press A
6. **Collect 5 Trash Items** — red glow on the road, look + press A
7. **Progress bar reaches 5/5** — Time Machine lights up
8. **Travel back to Future** — walk to Time Machine, press A
9. **Future is now clean** — blue sky, green trees, restored buildings
