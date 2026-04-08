# The 22nd Century - Project Spec
**Team 13 (JOYIKA):** Jonathan Le, Yili Wen, Katie Park
**Course:** CS 6334 Virtual Reality
**Progress Check:** April 1 | **Submission:** April 11 | **Demo:** April 13 (Mon)

---

## Current Project State (as of March 31)

### Scene Visual Review (April 1)
1. **Apocalypse** — Dark, destroyed buildings, dead trees, trash everywhere, rubble, wrecked cars. Full post-apocalyptic feel.
2. **Cleaner_Apocalypse** — Slightly brighter, small green plants appearing among ruins.
3. **Getting_Cleaner_Apocalypse** — Intact buildings, sky with clouds visible, city recovering.
4. **Very_Clean_Apocalypse** — Blue sky, lots of green trees lining streets, city restored.
5. **Present** — Clean modern city, blue sky with white clouds, healthy trees. This is the "past" where player cleans up.

Map progression is visually clear and well done (Katie).

### Assets in Project
- **Scenes (5):** Apocalypse, Cleaner_Apocalypse, Getting_Cleaner_Apocalypse, Very_Clean_Apocalypse, Present
- **Gun model:** Low Poly Weapons VOL.1 (M1911 pistol with fire animation + muzzle flash)
- **Zombie models:** Mummy, Apocalypse Zombie, Zombie Gorilla (GAMWILL)
- **Map assets:** CartoonLowPolyCityLite, post-apocalyptic buildings, abandoned buildings, dead trees/shrubs, rubble, trash bags, solar panels, wind turbines, polygon trees, field poppies, grass
- **Materials:** Ground materials for each scene variant (Apocalypse, Cleaner, Getting_Cleaner, Very_Clean, Present)
- **VR Platform:** Google Cardboard XR Plugin (configured in XR settings)
- **Utilities:** QuickOutline (object highlight), MiLabCardboardExtension, TextMesh Pro, Mess Maker Free

### Scripts Implemented (5 total)
| Script | Owner | What it does |
|--------|-------|-------------|
| `GunScript.cs` | Jonathan | Gun equip, follow camera, fire bullets, muzzle flash, animation triggers |
| `BulletScript.cs` | Jonathan | Bullet collision detection, deals 50 damage to zombies, auto-destroy |
| `ZombieScript.cs` | Jonathan | Zombie health (100hp), hit/die animations, death destruction |
| `InteractableObjectScript.cs` | Jonathan | Reticle pointer enter/exit, button press to interact, UnityEvent callback |
| `ButtonInterface.cs` | Jonathan | IButton interface for UI buttons (Execute, setHover) |

---

## Feature Checklist: What's Done vs What's Needed

### BASIC REQUIREMENTS (must have for preliminary prototype)

| # | Feature | Owner | Status | Notes |
|---|---------|-------|--------|-------|
| 1 | User movement (Cardboard controller) | All | DONE | Cardboard XR plugin configured, joystick input in scripts |
| 2 | Pick up gun | Jonathan | DONE | GunScript handles equip with smooth lerp to camera |
| 3 | Aim/point the gun | Jonathan | DONE | Gun follows camera (head tracking) |
| 4 | Shoot gun | Jonathan | DONE | Bullet prefab instantiation, muzzle flash, fire animation |
| 5 | Zombies get hit and die | Jonathan | DONE | BulletScript deals damage, ZombieScript handles hit/die |
| 6 | Zombie AI - track and follow user | Jonathan | NOT DONE | ZombieScript has no NavMesh/movement logic yet |
| 7 | Zombies spawn near user and attack | Jonathan | NOT DONE | No spawner script exists |
| 8 | 4+ map variations (future gets cleaner) | Katie | MOSTLY DONE | 5 scenes exist (Apocalypse through Present), need verification of visual progression |
| 9 | Time machine travel (future <-> present) | Unassigned | NOT DONE | No scene-switching script exists |
| 10 | Present world cleanup -> future gets cleaner | Unassigned | NOT DONE | No state tracking between worlds |
| 11 | Inventory system (switch guns/tools) | Katie | NOT DONE | No inventory script |
| 12 | Health system | Jonathan | NOT DONE | No player health script (only zombie health exists) |
| 13 | Haptic vibration on hit/shoot | **Yili** | NOT DONE | No vibration/haptics script |
| 14 | Pick up trash / clean pollutants | Katie | NOT DONE | InteractableObjectScript framework exists but no trash pickup logic |
| 15 | Story & NPC dialog | Katie | NOT DONE | No dialog system or NPC scripts |
| 16 | System control menu (Resume/Quit) | Jonathan | NOT DONE | ButtonInterface exists but no pause menu |

### ADVANCED REQUIREMENTS (need at least 1 for preliminary prototype)

| # | Feature | Owner | Status | Notes |
|---|---------|-------|--------|-------|
| A1 | Mobile sensor jump/crouch (accelerometer) | **Yili** | NOT DONE | No accelerometer script |
| A2 | AI-powered robot/drone companion (Gemini/ChatGPT) | Unassigned | NOT DONE | No AI integration scripts |

### DOCUMENTATION & SUBMISSION REQUIREMENTS

| Item | Status | Notes |
|------|--------|-------|
| Source document (asset URLs) | NOT DONE | Need to compile all asset store links |
| ReadMe document | NOT DONE | Needs scene info, target device, GitHub link, interaction instructions, YouTube link |
| Team document | NOT DONE | Needs member names, task assignments, contributions |
| Demo video (<2 min, 720p+, narrated) | NOT DONE | Upload to YouTube |
| APK build (preliminary.apk) | NOT DONE | Must build and test on device |
| .zip submission (<800MB) | NOT DONE | Clean Library/obj/Temp folders first |
| Add TA collaborators (ayush554, alphayama) | UNKNOWN | Check GitHub repo settings |

---

## Priority Action Plan for April 1 Progress Check

Tomorrow (April 1) is the **5-minute progress check** in class. The team needs to show a basic VR scene running on a headset. Current state is decent for a progress demo (scenes exist, gun works, zombies take damage).

### CRITICAL - Before April 11 Submission

**Jonathan (priority):**
1. Zombie AI movement - add NavMeshAgent to track and chase player
2. Zombie spawner - spawn waves near player
3. Player health system - take damage from zombies, death/game-over state
4. Pause menu - Resume/Quit floating UI using ButtonInterface

**Katie (priority):**
1. Verify 5 scene variations look visually distinct and progressively cleaner
2. Inventory system - script to switch between gun and cleanup tool
3. Trash pickup - extend InteractableObjectScript for trash collection
4. NPC dialog system - basic text-based dialog with NPCs

**Yili (priority):**
1. **Android haptic vibration** - use `Handheld.Vibrate()` or `AndroidJavaClass` for vibration on shoot/damage events
2. **Accelerometer jump/crouch** (Advanced Req A1) - read `Input.acceleration` to detect jump/crouch gestures
3. Help with time machine scene-switching logic
4. Help with connecting present cleanup state to future world appearance

### Must-Have for Submission
- At least **all basic requirements functional** + **1 advanced requirement** (accelerometer is easiest)
- Source, ReadMe, Team documents
- Demo video on YouTube
- Working APK build

---

## Yili's Task Breakdown (Your Work)

### Task 1: Android Haptic Vibration (Basic Requirement)
- Trigger phone vibration when player is hit by zombie
- Trigger vibration when shooting gun (for feedback)
- Use `Handheld.Vibrate()` for simple vibration
- For custom patterns: use `AndroidJavaClass("android.os.Vibrator")`
- Hook into `ZombieScript` (on player damage) and `GunScript` (on fire)

### Task 2: Accelerometer Jump & Crouch (Advanced Requirement - CRITICAL)
- Read `Input.acceleration` each frame
- Detect fast upward spike -> trigger player jump (CharacterController or Rigidbody)
- Detect fast downward spike -> trigger player crouch (scale camera Y down)
- Threshold tuning needed to avoid false triggers
- This is the team's most achievable advanced requirement for the deadline

### Task 3: Support - Time Machine Scene Switching
- Script to load different scenes based on game state
- Use `SceneManager.LoadScene()` to switch between Present and future variants
- Track a "cleanliness score" that determines which future scene to load

### Task 4: Support - Present Cleanup -> Future Connection
- Global state/GameManager to track how much trash picked up, trees planted
- Map cleanliness score to which future scene loads (0-25% = Apocalypse, 25-50% = Cleaner, etc.)

---

## Asset Sources (for Source Document)
| Asset | URL |
|-------|-----|
| Low Poly Weapons VOL.1 (Pistol) | https://assetstore.unity.com/packages/3d/props/guns/low-poly-weapons-vol-1-151980 |
| Cartoon Low Poly City Pack Lite | https://assetstore.unity.com/packages/3d/environments/urban/cartoon-low-poly-city-pack-lite-166617 |
| Free Mummy Monster | https://assetstore.unity.com/packages/3d/characters/free-mummy-monster-134212 |
| Apocalypse Series Zombie Character Free | https://assetstore.unity.com/packages/3d/characters/humanoids/fantasy/apocalypse-series-zombie-character-free-272407 |
| Monsters Series Zombie Gorilla Free | https://assetstore.unity.com/packages/3d/characters/creatures/monsters-series-zombie-gorilla-free-sample-272322 |
| QuickOutline | (included in project - check asset store) |
| Google Cardboard XR Plugin | https://developers.google.com/cardboard |
| Mess Maker Free | (check asset store) |
| AllSkyFree | (check asset store - skybox) |
| SkySeries Freebie | (check asset store - skybox) |
| Realistic Terrain Textures Lite | (check asset store) |
