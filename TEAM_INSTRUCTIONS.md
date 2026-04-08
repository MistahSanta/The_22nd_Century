# The 22nd Century - Team Instructions
**Last Updated: April 8, 2026**
**Updated by: Yili**

---

## Features Implemented (Summary)

### Advanced Requirements
- **Accelerometer Jump** — Physical jump detected via phone accelerometer
- **Accelerometer Crouch** — Physical crouch detected via phone accelerometer

### Core Gameplay
- **Gun System** — Pick up gun (blue glow), first-person aim, shoot bullets
- **Zombie AI** — Zombies slowly chase player, turn red and die when shot
- **Game Over** — Zombie touches player → red screen → press A to restart
- **Time Machine** — 3D model, gaze interaction, travel between Future and Present
- **Trash Pickup** — Pick up garbage picker first, then collect 5 trash items
- **World Switching** — 5 world variants in 1 scene, instant switching (no loading)
- **Skybox Switching** — Each world has its own sky
- **Cleanliness System** — Picking trash in Present improves the Future world
- **Haptic Vibration** — Phone vibrates on shoot, hit, and interact

### UI/UX
- **Crosshair** — Center screen reticle
- **Gaze Interaction** — Look at objects → yellow highlight + prompt text
- **HUD Progress Bar** — Shows trash collected (0/5), only in Present
- **Controls Menu** — Press Menu button to see button mappings
- **Controller Mapping** — Configurable in Inspector (GameManager > ControllerMapping)

---

## How to Open & Run

### Step 1: Pull from GitHub
```
cd [project folder]
git pull origin main
```

### Step 2: Open in Unity
- Unity version: **6000.3.4f1**
- Open the project folder in Unity Hub

### Step 3: Open Main Scene
- Open **Assets/Scenes/Main.unity** (this is the merged scene with all 5 worlds)

### Step 4: Run Setup (IMPORTANT - do this after every pull)
1. **Tools > Setup Main Scene** (wait for it to finish)
2. **Tools > Setup Gun Interaction**
3. **Ctrl+S** (Cmd+S on Mac) to save
4. Press **Play** to test

### Step 5: Build to Android
- **File > Build Settings** > Platform: Android > Scenes: Main
- Click **Build and Run**

---

## Game Flow

```
1. START in Future (Apocalypse)
   ↓
2. Pick up GUN (blue glow, nearby) → Press A
   ↓
3. Shoot ZOMBIES (press Top/H button) → They turn red and die
   ↓
4. Walk to TIME MACHINE → Press A → Travel to Present
   ↓
5. Pick up GARBAGE PICKER (blue glow, near TM) → Press A
   ↓
6. Pick up 5 TRASH items (red glow) → Press A on each
   ↓
7. Progress bar reaches 5/5 → Time Machine lights up
   ↓
8. Walk to TIME MACHINE → Press A → Travel back to Future
   ↓
9. Future is now CLEAN (VeryClean world with blue sky and trees)
```

---

## Controller Mapping

| Button | Action |
|--------|--------|
| **Joystick** | Move player |
| **Head movement** | Look around (VR) |
| **A (js11)** | Interact (pick up, travel, collect) |
| **Top / H (js7)** | Shoot gun |
| **Y (js5)** | Jump |
| **Menu (js9/js13)** | Toggle controls menu |

> To change mappings: Select **GameManager** in Hierarchy > Inspector > **ControllerMapping** component > change button names

---

## Important Notes for Teammates

### DO
- Always run **Tools > Setup Main Scene** after pulling new changes
- Always run **Tools > Setup Gun Interaction** after Setup Main Scene
- Save (**Ctrl+S**) before playing
- Edit in **Main.unity** scene (not the individual old scenes)
- Test on Android phone with Cardboard headset for full experience

### DON'T
- Don't delete any asset folders (they may be referenced by scenes)
- Don't install MCP for Unity package (it causes Unity to freeze during domain reload)
- Don't modify the old individual scenes (Apocalypse.unity, Present.unity, etc.) — they are backups
- Don't change Enter Play Mode Options (already configured to skip domain reload)

### If Unity freezes on "Reloading Domain"
1. Force quit Unity (Activity Monitor or `pkill -9 Unity` in terminal)
2. Delete `Library/ScriptAssemblies` folder
3. Reopen Unity and wait for full compile (3-5 min, this is normal)

---

## Project Structure

```
Assets/
├── Scenes/
│   └── Main.unity              ← USE THIS (merged scene)
├── custom_scripts/             ← All game scripts
│   ├── GameManager.cs          — World switching, score, game over
│   ├── AccelerometerMovement.cs — Phone sensor jump/crouch
│   ├── HapticFeedback.cs       — Phone vibration
│   ├── ControllerMapping.cs    — Button config
│   ├── InteractableObjectScript.cs — Gaze + interact system
│   ├── PlayerInteraction.cs    — Raycast detection
│   ├── CrosshairUI.cs          — Screen crosshair
│   ├── CleanlinessHUD.cs       — Progress bar (Present only)
│   ├── ControlsMenu.cs         — Button help menu
│   ├── TimeMachineScript.cs    — Time travel logic
│   ├── TrashPickup.cs          — Trash collection
│   ├── TrashGrabberScript.cs   — Garbage picker tool
│   ├── PlayerSafetyNet.cs      — Anti-fall protection
│   ├── MouseLook.cs            — Editor mouse control (disabled on phone)
│   ├── Gun/
│   │   ├── GunScript.cs        — Gun pickup, aim, shoot
│   │   ├── BulletScript.cs     — Bullet damage
│   │   └── GunProximityDetector.cs — Gaze detection for gun
│   └── Zombies/
│       └── ZombieScript.cs     — Zombie AI, chase, die
├── Editor/
│   ├── SceneSetup.cs           — One-click scene setup tool
│   ├── GunSetup.cs             — Gun interaction binding
│   └── SceneMerger.cs          — Scene merge tool (already used)
├── TimeMachineModel/           — Time machine 3D model (.glb)
├── GarbagePickerModel/         — Garbage picker 3D model (.obj)
└── [Third-party assets]        — Map, zombies, guns, skybox, etc.
```

---

## What Still Needs Work

- [ ] Skybox materials need to be assigned in GameManager Inspector (if not auto-set)
- [ ] Gun animator (fire animation) — original animator reference was lost during merge
- [ ] Sound effects (shoot, pickup, ambient) — not yet added
- [ ] NPC dialog system — not yet implemented
- [ ] AI drone companion (Advanced Requirement 2) — not yet implemented
- [ ] Documentation: Source doc, ReadMe doc, Team doc for submission
- [ ] Demo video (< 2 min, 720p+, narrated) for YouTube
- [ ] APK build named `preliminary.apk` for submission
