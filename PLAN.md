# DePhobia — Sequenced Engineering & Modernization Plan

Based on the findings in `AUDIT_REPORT.md`, this plan outlines the step-by-step engineering execution to modernize DePhobia to a shippable Unity 2022.3 LTS OpenXR project.

---

## Items to be Removed vs. Replaced

| Item / Directory | Action | Technical Rationale |
| :--- | :--- | :--- |
| `Assets/GoogleVR` | **Remove** | Obsolete Google Cardboard/Daydream SDK. Incompatible with modern Unity 2022.3 LTS, causes compile errors, and blocks OpenXR initialization. |
| `Assets/Menu/orientation.cs` | **Remove** | Forcing `Portrait` phone orientation breaks VR stereo rendering. |
| `Assets/Menu/VrStop.cs` | **Remove** | Uses deprecated `XRSettings.LoadDeviceByName("None")`. In VR headsets, the menu is a 3D world-space scene. |
| `Assets/Menu/Valuefix.cs` | **Remove / Replace** | Hack running in `Update()` to prevent empty string crashes; replaced with proper event-driven input validation in `TherapySettingsManager`. |
| `Assets/Menu/Getval.cs` | **Replace** | Replaced with `TherapySettingsManager`. |
| `Assets/Menu/Getval1.cs` | **Replace** | Replaced with `TherapySettingsManager`. |
| `Assets/Menu/Getval2.cs` | **Replace** | Replaced with `TherapySettingsManager`. |
| `Assets/Room/Scripts/StartVr.cs` | **Replace** | Replaced with unified `VRPlayerRig` and `TherapySessionController`. |
| `Assets/StartVr2.cs` | **Replace** | Replaced with unified `VRPlayerRig` and `TherapySessionController`. |
| `Assets/Menu/StartVr3.cs` | **Replace** | Replaced with unified `VRPlayerRig` and `TherapySessionController`. |

---

## Sequenced Execution Phases

### Phase 2.1 — Version Control & Package Foundation (Effort: Low | Risk: Low)
1. Initialize local Git repository with baseline commit on branch `main`.
2. Create and switch to working branch `fix/ai-repair`.
3. Update `ProjectSettings/ProjectVersion.txt` to `2022.3.40f1` (Standard 2022.3 LTS).
4. Update `Packages/manifest.json`:
   - Add `com.unity.xr.openxr`: `1.8.2`
   - Add `com.unity.xr.management`: `4.4.1`
   - Add `com.unity.xr.interaction.toolkit`: `2.5.4`
   - Add `com.unity.inputsystem`: `1.7.0`
   - Remove obsolete `com.unity.collab-proxy`.
5. Update `ProjectSettings/EditorBuildSettings.asset` to register all 4 game scenes:
   - Index 0: `Assets/Menu/Menu.unity`
   - Index 1: `Assets/Room/scenes/the last revelation.unity`
   - Index 2: `Assets/Snowconesolid Assets/Super Beach Pack/Scenes/Island Example 3.unity`
   - Index 3: `Assets/Geopipe/Pack01Demo/Pack01Demo01.unity`
6. Update `ProjectSettings/ProjectSettings.asset`:
   - Set Product Name: `DePhobia`
   - Set Company Name: `MG Games`
   - Set Application Identifier: `com.mggames.dephobia`
   - Set Color Space to Linear (`m_ActiveColorSpace: 1`).
   - Set Android Architectures to ARM64 (`AndroidTargetArchitectures: 2`).

### Phase 2.2 — Core Architecture & Data Persistence (Effort: Medium | Risk: Low)
1. Implement `TherapySettingsManager.cs`:
   - Central ScriptableObject / persistent singleton holding settings for all 3 simulations with safe defaults.
   - Event-driven UI bindings (`onEndEdit`, `onValueChanged`) with validation clamps (no `Update()` polling).
   - Persists patient configurations cleanly via `PlayerPrefs` / JSON.
2. Implement `PatientSessionManager.cs`:
   - Structured session history data (`SessionRecord`) saving timestamp, duration, phobia simulation type, settings used, and peak metrics.
   - Handles saving session outcomes gracefully to persistent JSON data with clean backwards compatibility.
3. Implement `VRFadeManager.cs`:
   - Reusable VR screen fader for smooth, comfortable transitions between scenes without visual jarring.

### Phase 2.3 — Unified OpenXR Player Rig & Input (Effort: Medium | Risk: Medium)
1. Implement `VRPlayerRig.prefab` / `VRPlayerRig.cs`:
   - Universal OpenXR camera setup with Tracked Pose Driver.
   - Left and right controller tracking with ray interactors for UI and direct interactors.
   - Dual-input locomotion:
     - VR controller thumbstick smooth movement and snap/smooth turning.
     - Desktop keyboard/mouse fallback (WASD movement + Mouse look) for testing in Editor without wearing a headset.
   - Integrated VR Comfort Vignette (darkens FOV during movement to eliminate motion sickness).
   - Recenter and seated/standing height calibration helper.
2. Implement `VRInGamePauseMenu.cs`:
   - Floating world-space pause canvas toggled by VR controller Menu/Y button or keyboard `Esc`.
   - Displays session elapsed time, Resume, and Save & Exit to Main Menu options.
   - Spawns a physical in-world emergency exit beacon for patients in distress.

### Phase 2.4 — Scene Wiring & Simulation Repair (Effort: High | Risk: Medium)
1. **Claustrophobia (`the last revelation.unity`):**
   - Replace legacy `StartVr` and GoogleVR raycaster with `VRPlayerRig`.
   - Update `xposmove.cs` and `xnegmove.cs` to source parameters from `TherapySettingsManager`.
   - Add player collision boundary to prevent clipping when walls close in.
2. **Aquaphobia (`Island Example 3.unity`):**
   - Replace legacy `StartVr2` and GoogleVR raycaster with `VRPlayerRig`.
   - Fix `MoreHeight1.cs`: uncomment water speed and interval variables, wiring them dynamically to `TherapySettingsManager`.
   - Add water level detection and underwater visual tint / safety clamp.
3. **Acrophobia (`Pack01Demo01.unity`):**
   - Replace legacy `StartVr3` and GoogleVR raycaster with `VRPlayerRig`.
   - Refine `vrlook.cs`: maintain the authentic **Look-Down Exposure Therapy Mechanic** while adapting angle checks to 6DoF OpenXR head rotation.
   - Fix altitude and timer UI in `ValChange.cs` (cache references, eliminate per-frame string allocations).
   - Add graceful ceiling/landing handling so the player does not drop into void geometry upon reaching max height.

### Phase 2.5 — Main Menu VR Transformation (Effort: Medium | Risk: Low)
1. Convert `Assets/Menu/Menu.unity` Canvas from Screen Space Overlay to a 3D World Space panel positioned comfortably 2.5m in front of the player.
2. Update `mainmenu.cs` to integrate with `TherapySettingsManager` and `PatientSessionManager`.
3. Add a scrollable session history viewer displaying past therapy records with dates and durations.
4. Replace deprecated `WWW.EscapeURL` in `FOLLOW.cs` with `UnityWebRequest.EscapeURL`.

### Phase 2.6 — Polish, Documentation & Verification (Effort: Medium | Risk: Low)
1. Create `CHANGELOG_AI.md` detailing every architectural and script change.
2. Create `THIRD_PARTY_ASSETS.md` cataloging third-party assets and licenses.
3. Update `README.md` with complete setup instructions, VR controls (Quest/PCVR), desktop test controls, and build guidelines.
4. Run static verification: confirm zero compiler warnings/errors, clean PlayMode execution, and step-by-step scene walkthrough checklist.
