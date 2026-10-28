# CHANGELOG_AI.md

This document tracks all changes made to the Kanso VR (formerly DePhobia) codebase during modernization, repair, and feature completion.

---

## [2.0.0] - 2026-10-28: Forest Mineral & Sage UI Redesign

### Complete Visual & Architectural Redesign
- **Forest Mineral & Sage Visual Language**: Purged all generic blue and violet shades across the entire project. Standardized on Earth Charcoal (#141614), Dark Spruce Moss (#1E221E), Lichen Border (#343A34), Warm Off-White (#F3F5F2), Ash Olive (#9AA299), Matcha Sage (#76936A), and Umber Clay (#AD6E60).
- **Plus Jakarta Sans Typography**: Introduced clean geometric sans typography (PlusJakartaSans.ttf) with balanced weights, sentence case, and zero shouting ALL-CAPS.
- **Centralized Design Tokens (ThemeTokens.cs)**: Created source-of-truth palette and UI styling helper methods used uniformly across all UI scripts.

### Main Portal (Sanctuary) Overhaul (mainmenu.cs)
- **Single Center Panel with 3 Tabs**: Replaced 6 nested legacy submenus and raw float input fields with a unified 3-tab portal (Enclosed Space, Rising Water, High Elevation).
- **Inline Intensity Presets**: One-click calibrated presets (Gentle, Standard, Deep) with real-time parameter feedback.
- **Loud Primary CTA**: Prominent Matcha Sage [ Begin Session ] button centered on the screen.
- **In-Place View Swapping**: Selecting Session History or Comfort & Audio swaps the panel content in-place with a clean [ ← Back to Sessions ] header, completely eliminating popups and modal disorientation.
- **Decommissioned Clutter**: Safely deactivated legacy FOLLOWOPT, social media solicitation links, and raw float input boxes.

### Active Exposure & Somatic Feedback
- **Zero Floating HUD in Line of Sight (ValChange.cs, rlook.cs)**: Removed all 5 floating HUD timers and telemetry numbers from the active field of view, keeping underlying mathematical telemetry intact for session logging.
- **Horizon Respiration & Escape Ring (VRHorizonRespirationRing.cs)**: Added an ethereal 4.5s breathing guide anchored at the lower peripheral boundary. Looking down at the ring for 1.5s or tapping smoothly returns to the menu without anxiety.
- **Somatic Cues**: Replaced intrusive text toast banners with subtle haptic impulses and spatial acoustic chimes.

### In-Game Pause Slate & Milestone Closure
- **Minimalist Two-Button Slate (VRInGamePauseMenu.cs)**: Streamlined pause interface to two clear choices: [ Resume Session ] (Sage) and [ Return to Menu ] (Clay), with elapsed time and a quiet hint for recentering view via Right Stick or 'R' key.
- **Milestone Closure Flow (TherapySessionRitual.cs)**: Smooth fade to a clean summary card upon reaching milestones (summit 360m in Acrophobia, wall clamp 2.85m in Claustrophobia, water clamp 1.72m in Aquaphobia) showing duration and peak metric, with a single button to return to the Sanctuary.

---
## [1.0.0] - 2026-10-24: Kanso VR Identity, Legibility & GitHub Release Polish

### Brand & Presentation Modernization
- **High-Legibility ANSI Shadow ASCII Art**: Replaced clustered, uninterpretable ASCII art with crystal-clear ANSI Shadow block typography banner in README.md and BRAND_IDENTITY.md featuring subtitle [ 簡素 · Simplicity, Composure, and Psychological Safety ].
- **Global Brand Alignment**: Replaced legacy prototype naming across README.md, BRAND_IDENTITY.md, ACCESSIBILITY.md, DESIGN_SYSTEM.md, TONE_OF_VOICE.md, PLAYTEST_SCRIPT.md, and THIRD_PARTY_ASSETS.md with the finalized brand name **Kanso VR**.
- **Bulletproof In-Page Navigation**: Implemented explicit HTML anchor tags (#vision, #simulations, #architecture, #accessibility, #hardware, #privacy, #setup, #docs, #seo, #citation, #license) in README.md preventing broken links from GitHub emoji-slug filtering.
- **Academic Citation & Schema**: Embedded BibTeX citation block directly in README.md alongside CITATION.cff and Schema.org SoftwareApplication JSON-LD.
- **GitHub Repository Synchronization**: Updated repository description and homepage to reflect **Kanso VR** on https://github.com/husain-saifee/kanso-vr.

---

## [Unreleased] - Branch `polish/experience` (Quality, Feel & Accessibility Pass)

### Phase 1: Whole-Experience Audit
- Authored and delivered `EXPERIENCE_AUDIT.md`: severity-tagged audit across First-Run, Moment-to-Moment Feel, Failure Modes, Coherence, Audio, Accessibility, Comfort, Flow Structure, and Tone of Voice.
- Discovered and cataloged critical P0 bugs: Acrophobia camera rig decoupling from hoverboard, startling car horn audio spikes at volume 1.4, flashing red countdown alerts, missing in-world emergency beacons, and Main Menu canvas disconnected by 700m from the camera.

### Phase 2: Design System & Calibration (Direction C: Soft & Abstract Horizon)
- Created `DESIGN_SYSTEM.md`: codified Direction C aesthetic (Dusk Lavender, Dawn Amber, Deep Twilight Surfaces), WCAG 2.2 AAA contrast standards (14.8:1), typography scale, motion language (`breathe`, `settle`, `acknowledge`), and acoustic hierarchy.
- Authored `TONE_OF_VOICE.md`: established non-shaming, non-clinical, supportive second-person copy standards and eliminated all medical/efficacy claims.

### Phase 3 & 4: Feel, Responsiveness & VR Rig Anchoring
- **`Assets/Scripts/VR/VRPlayerRig.cs`**:
  - Resolved Acrophobia decoupling bug by properly anchoring `VRPlayerRig` as a child of the hoverboard platform, and introduced `enableLocomotion` and `enableGravity` flags to prevent falling off the board.
  - Added universal OpenXR controller input shortcuts (`CommonUsages.menuButton`, `CommonUsages.secondaryButton` Y/B, and thumbstick click for recenter).
  - Added gentle informational haptic impulses via `SendHapticImpulse()`.
  - Added dynamic seated mode eye-level compensation (`+0.35m`) and horizontal yaw recentering (`RecenterView()`).
- **`Assets/Scripts/VR/VRWorldSpaceCanvasHelper.cs`**:
  - Fixed world-space canvas positioning bug by dynamically computing canvas arrival position directly in front of `Camera.main` at 1.8m eye level, eliminating the 700m offset bug.

### Phase 5: Audio Design & Subtitle Architecture
- **`Assets/Scripts/Core/TherapyAudioManager.cs`**:
  - Implemented procedural 432 Hz harmonic ambient drone synthesis in C# (warm, continuous cosine envelope, zero loop seams, 100% royalty-free CC0).
  - Implemented procedural acoustic chime clicks (<40ms) for UI feedback.
  - Created global subtitle/caption event bus.
- **`Assets/Scripts/VR/TherapyCaptionUI.cs`**:
  - World-space subtitle banner anchored in lower third of user field of view for deaf/hard-of-hearing accessibility.
- **Audio Cleanup Across Simulations**:
  - In `vrlook.cs`: Permanently disabled startling car and truck horn honks (`mixkit-car-horn-718`, `720`, `1565`, `CarHornDopplerBy PE865405`).
  - In `StartVr.cs`: Muted mechanical hard-drive grinding audio.
  - In `mainmenu.cs`: Softened upbeat acoustic ukulele track.

### Phase 6: Accessibility, Comfort & Emergency Systems
- **`Assets/Scripts/VR/VREmergencyExitBeacon.cs`**:
  - Overhauled with Direction C styling (soft lavender/rose card, non-shaming copy: *"Return to Menu"* / *"Hold gaze or tap to exit safely"*).
  - Added `EnsureBeaconInScene()` auto-spawner ensuring an in-world rescue anchor is present in all therapy scenes.
- **`Assets/Scripts/VR/VRInGamePauseMenu.cs`**:
  - Overhauled with Direction C surface palette and supportive copy (*"You are in control. Take all the time you need"*).
  - Added Recenter View and Seated/Standing Mode quick toggles; replaced harsh red exit button with gentle rose tone (`#D48D8D`).
- Authored `ACCESSIBILITY.md`: documented full WCAG 2.2 AAA contrast compliance, multi-modal presentation, seated mode calibration, and vestibular comfort measures.

### Phase 7 & 8: Therapeutic Rituals, UI Polish & Privacy Hardening
- **`Assets/Scripts/VR/TherapySessionRitual.cs`**:
  - Created structured Start Ritual featuring a guided 3-breath visualizer ring before exposure mechanics begin.
  - Created calm End Ritual closure card allowing peaceful reflection and voluntary exit without forced countdown locks.
- **Simulation Controllers (`StartVr.cs`, `StartVr2.cs`, `StartVr3.cs`)**:
  - Connected simulations to `TherapySessionRitual`, holding stimuli until user feels settled.
  - In `vrlook.cs` & `ValChange.cs`: Removed flashing red alert text and yellow/red countdown shifts; fixed typo `Time Elasped` -> `Elapsed Time`.
- **Main Menu (`mainmenu.cs`) & Privacy Hardening**:
  - Dynamically cleans up misspellings (`CLAUSTROPHOBHIA` -> `Claustrophobia`), all-caps shouting headers, and clinical terms.
  - Completely neutralized external social links and data solicitation in `FOLLOW.cs`.
  - Removed `com.unity.modules.unityanalytics` from `Packages/manifest.json` and disabled `submitAnalytics` in `ProjectSettings.asset`.

### Phase 9 & 10: Verification & Playtesting
- Authored `PLAYTEST_SCRIPT.md`: comprehensive 20-minute manual test script for human testers covering first-run, all 3 simulations, safety exits, accessibility, and data persistence.
- Updated `README.md`, `DESIGN_SYSTEM.md`, and `THIRD_PARTY_ASSETS.md`.

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

### Phase 7 & 8: VR Stereo Rendering Hardening & Deprecation Cleanup
- **`Assets/Scripts/VR/VRFadeManager.cs` & `VRComfortVignette.cs`**:
  - Replaced unsupported `RenderMode.ScreenSpaceOverlay` with `RenderMode.ScreenSpaceCamera`.
  - Added dynamic stereo camera binding to `Camera.main` with calibrated plane distance (0.25m–0.35m) to ensure correct single-pass and multi-pass OpenXR stereo projection in both eyes without headset dropouts.
- **`Assets/Water Float/Demo/Standard Assets/Editor/CrossPlatformInput/CrossPlatformInputInitialize.cs`**:
  - Resolved `CS0618` deprecation warnings on `SetScriptingDefineSymbolsForGroup` / `GetScriptingDefineSymbolsForGroup` by adopting `NamedBuildTarget` on Unity 2021.2+.
