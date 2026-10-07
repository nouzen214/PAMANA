# PAMANA - Philippine Historical & Cultural Heritage Adventure

**PAMANA** is a 2D top-down educational adventure game developed as a Capstone Project for the **College of Computer Studies at the University of Perpetual Help System Laguna (UPHSL)**. Players navigate a museum of ancient halls and chambers, discover pre-colonial and historic Philippine artifacts, inspect historical plaques, and engage with cultural lore through interactive gameplay.

---

## Project Overview

- **Title**: PAMANA
- **Engine**: Unity 6 (6000.0.54f1)
- **Render Pipeline**: Universal Render Pipeline (2D Core / URP)
- **Target Platform**: PC (Windows / macOS / Linux) & Mobile
- **Language**: C# (.NET / Mono)
- **Institution**: University of Perpetual Help System Laguna – College of Computer Studies

---

## Key Features

1. **Dual Character Selection**: Choose between **Darrel** and **Darla** with custom 4-directional animations and dialogue avatars.
2. **Interactive Museum Hub**: Central lobby connecting to Left and Right museum wings with 12 distinct artifact exhibition rooms.
3. **Artifact Inspection & Lore**: Proximity-based interaction system featuring ornate dialogue banners, silhouette inspection, and educational historical plaques.
4. **Adaptive Controls**: Dual-support for keyboard (WASD/Arrows) and touch controls via dynamic on-screen Virtual Joystick.
5. **Interactive HUD**: Clean in-game interface with Heart health container, Settings gear button, Backpack inventory button, Book journal button, and In-Game Pause modal.

---

## Project Structure

```text
Assets/
├── Prefabs/              # Reusable game objects (InGameCanvas, Player, Joystick)
├── Scenes/               # All game levels and menu scenes
│   ├── MainMenu.unity
│   ├── CharacterSelect.unity
│   ├── Lobby.unity
│   ├── LeftHallway.unity
│   ├── RightHallway.unity
│   └── Room_1.unity ... Room_12.unity
├── Scripts/              # C# game systems and architecture
│   ├── Editor/           # Scene generators, build & UI automation tools
│   ├── Movement & Input  # PlayerMovement, PlayerInputHandler, VirtualJoystick
│   ├── Animation & Select# CharacterAnimator2D, CharacterSelectManager
│   ├── Exploration & Room# RoomController, RoomDoor, PortalController
│   ├── Health & Hazards  # Health, HealthUI, HeartHealthUI, Hazard
│   └── UI & Menus        # MainMenuController, InGameMenuController, ArtifactInspectionUI
├── Settings/             # Universal Render Pipeline (URP) assets and quality profiles
├── Sprites/              # 2D visual assets (Characters, Artifacts, Environment)
├── TextMesh Pro/         # Dynamic typography assets and shaders
└── UI/                   # User interface graphics (Frames, Dialogue boxes, Icons)
```

---

## Installation & Setup

1. **Prerequisites**:
   - Install **Unity Hub** and **Unity 6.0 (6000.0.54f1)**.
   - Modules: Universal Windows Platform / Standalone Support.
2. **Open Project**:
   - In Unity Hub, click **Add** > select this project folder.
   - Open with Unity 6.0.54f1.
3. **Play**:
   - Open `Assets/Scenes/MainMenu.unity` and click **Play** (or press Play from any scene; `MainMenu` will automatically run as configured in `GameStartupConfig.cs`).
