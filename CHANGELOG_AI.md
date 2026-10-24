# CHANGELOG_AI.md

This document tracks all changes made to the DePhobia codebase during modernization, repair, and feature completion.

---

## [Unreleased] - Branch `fix/ai-repair`

### Phase 1: Discovery & Audit
- Generated `AUDIT_REPORT.md` analyzing engine version, packages, scenes, scripts, and runtime bugs.
- Created `PLAN.md` outlining the sequenced engineering execution.
- Initialized local Git repository on branch `main` and branched into `fix/ai-repair`.

### Phase 2: Engine, Package & Build Setup (P0 Blockers Resolved)
- **`ProjectSettings/ProjectVersion.txt`**: Upgraded target editor to `2022.3.40f1` (Unity 2022.3 LTS).
- **`Packages/manifest.json`**:
  - Added modern OpenXR (`com.unity.xr.openxr`: `1.8.2`).
  - Added XR Interaction Toolkit (`com.unity.xr.interaction.toolkit`: `2.5.4`).
  - Added XR Management (`com.unity.xr.management`: `4.4.1`).
  - Added Input System (`com.unity.inputsystem`: `1.7.0`).
  - Updated TextMeshPro (`3.0.9`) and Timeline (`1.7.6`).
  - Removed obsolete `com.unity.collab-proxy` and removed `packages-lock.json`.
- **`ProjectSettings/EditorBuildSettings.asset`**: Registered all 4 game scenes in build settings:
  - Index 0: `Assets/Menu/Menu.unity`
  - Index 1: `Assets/Room/scenes/the last revelation.unity` (Claustrophobia)
  - Index 2: `Assets/Snowconesolid Assets/Super Beach Pack/Scenes/Island Example 3.unity` (Aquaphobia)
  - Index 3: `Assets/Geopipe/Pack01Demo/Pack01Demo01.unity` (Acrophobia)
- **`ProjectSettings/ProjectSettings.asset`**:
  - Set product name to `DePhobia`, company name to `MG Games`, application ID `com.mggames.dephobia`.
  - Set Color Space to Linear (`m_ActiveColorSpace: 1`) for modern VR headsets.
  - Set Android Target Architectures to ARM64 (`AndroidTargetArchitectures: 2`).
  - Set Android minimum SDK to API 26 (Android 8.0).
- **`Assets/Water Float/.../CrossPlatformInputInitialize.cs`**:
  - Removed obsolete `BuildTarget.PSM`, `BuildTarget.Tizen`, `BuildTargetGroup.PSM`, `BuildTargetGroup.Tizen` references that cause compile failure in Unity 2022.3 LTS.

### Phase 3 & 4: Core Systems, Data Persistence & VR Subsystems
- **`Assets/Scripts/Core/TherapySettingsManager.cs`**:
  - Central singleton managing settings for Claustrophobia, Aquaphobia, Acrophobia, and VR comfort.
  - Replaced per-frame string parsing with event-driven `onEndEdit` listeners and range clamps.
  - Persists settings via `PlayerPrefs` / JSON.
- **`Assets/Scripts/Core/PatientSessionManager.cs`**:
  - Tracks session elapsed time and peak metrics.
  - Saves structured history to `session_history.json`.
  - Concurrently updates legacy `.txt` files in `persistentDataPath/PersonStats` for full backwards compatibility.
- **`Assets/Scripts/VR/VRFadeManager.cs`**:
  - Provides smooth screen fade-to-black scene transitions to eliminate VR headset visual popping.
- **`Assets/Scripts/VR/VRComfortVignette.cs`**:
  - Procedural circular FOV reduction tunneling during locomotion and vertical movement to prevent vestibular motion sickness.
- **`Assets/Scripts/VR/VRInGamePauseMenu.cs`**:
  - Floating 3D pause canvas toggled by VR controller Menu button or keyboard `Escape`.
  - Displays session timer, Resume, and Save & Return to Menu options.
- **`Assets/Scripts/VR/VREmergencyExitBeacon.cs`**:
  - In-world physical rescue beacon for patients in acute distress (gaze, ray, or click to exit).
- **`Assets/Scripts/VR/VRPlayerRig.cs`**:
  - Unified OpenXR player rig with CharacterController locomotion.
  - Dual-input: VR controller thumbsticks + desktop keyboard (WASD) and mouse look fallback.
  - Snap turn and smooth turn options.
  - Pitch tracking helper (`GetHeadPitch()`, `IsLookingDown()`).
- **`Assets/Scripts/VR/VRWorldSpaceCanvasHelper.cs`**:
  - Adapts UI canvases from Screen Space to comfortable 3D World Space panels in front of the player.
- **`Assets/Scripts/VR/LegacyCompat/`**:
  - Added clean compatibility stubs matching legacy GoogleVR GUIDs (`GvrPointerPhysicsRaycaster`, `GvrReticlePointer`, `GvrControllerInput`, `GvrPointerInputModule`, `GvrEditorEmulator`) so existing scenes load with zero missing script warnings.

### Phase 5: Simulation Repair & Feature Completion
- **Claustrophobia (`Assets/Room/` & `Assets/`):**
  - Modernized `StartVr.cs` to initialize session via `PatientSessionManager` and handle VR fades.
  - Modernized `xposmove.cs` and `xnegmove.cs` to pull speed and intervals from `TherapySettingsManager`.
  - Added null safety checks to `wallstable.cs` and `poswallstable.cs`.
- **Aquaphobia (`Assets/Snowconesolid Assets/` & `Assets/`):**
  - Modernized `MoreHeight1.cs`: un-commented speed and interval variables, wiring them dynamically to `TherapySettingsManager`.
  - Modernized `StartVr2.cs` for session lifecycle and VR rig setup.
  - Modernized `VRLookWalk.cs` to support dual-input locomotion (thumbstick/WASD + gaze-look walk).
- **Acrophobia (`Assets/Geopipe/` & `Assets/`):**
  - Modernized `vrlook.cs`: preserved and refined the core **Look-Down Exposure Therapy Mechanic** with 6DoF pitch normalization, audio clip null guards, and ground safety clamping at `y = 27.04m`.
  - Modernized `ValChange.cs`: cached UI references and eliminated per-frame string allocations.
  - Modernized `StartVr3.cs` for session lifecycle and VR rig setup.
- **Main Menu (`Assets/Menu/`):**
  - Modernized `mainmenu.cs`: integrated with `TherapySettingsManager`, `VRFadeManager`, and `PatientSessionManager` history viewer.
  - Modernized `Getval.cs`, `Getval1.cs`, `Getval2.cs` with event-driven callbacks.
  - Migrated `FOLLOW.cs` from obsolete `WWW.EscapeURL` to `UnityWebRequest.EscapeURL`.
  - Converted `orientation.cs`, `VrStop.cs`, `Valuefix.cs`, and `splashissue.cs` to safe, harmless components.

### Phase 6: Documentation & Licensing
- Created `THIRD_PARTY_ASSETS.md` cataloging all third-party models, shaders, and audio assets.
- Completely rewrote `README.md` with modern OpenXR setup, VR controls, and architecture overview.
