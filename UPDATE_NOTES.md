# Update Notes — May 4, 2026
**Updated by: Yili**

## New Features

### Player Health System
- 3 hearts (using Simple Heart Health System asset)
- Screen flashes red on damage with 2.5s invincibility window
- Full red screen on death with "GAME OVER" prompt
- Teammate health shown as 3D spheres above their head
- "DEAD" tag appears above teammate when they die

### Objective Tracker
- Task bar in top-left showing current objective and direction arrow
- Arrow turns green when facing the correct direction
- Distance to target displayed in meters
- Light beam marks the current objective location
- Mission briefing popup when entering Present world

### Kill Counter & Victory
- Kill counter displayed in Future world
- Floating damage numbers (-50) appear on zombie hits
- Victory screen "YOU SAVED THE FUTURE!" after completing all tasks

### Haptic Feedback (6 patterns)
- Shoot: short pulse (50ms)
- Damage: strong pulse (300ms)
- Pickup: double-tap pattern
- Game Over: long vibration (500ms)
- Zombie proximity: heartbeat pulse (closer = faster)
- Timer urgent: rapid triple pulse

### Bullet Improvement
- Replaced sphere with 3D bullet model (bullet.fbx)
- Correct forward orientation
- Orange glowing trail effect

### Map Boundaries
- Hard boundary clamp prevents walking off map
- Fog effect near edges
- Invisible boundary walls

### Timer & Game Over
- Present world timer set to 120 seconds
- Time runs out = Game Over
- Player death only affects that player (teammate can continue)
- Restart shuts down Photon cleanly before reloading

### Multiplayer Compatibility
- All UI uses WorldSpace Canvas (visible in VR split-screen)
- Start menu visible in VR with connection status
- PlayerFeatureAttacher auto-attaches components to Photon-spawned players
- Collision and tree collider fixes

### Advanced Requirement
- Accelerometer-based jump and crouch using phone sensors
