# DePhobia — Comprehensive Unity VR Project Audit Report

**Date:** October 2026  
**Auditor:** Senior Unity XR Technical Architect  
**Project:** DePhobia (VR Exposure Therapy Application)  
**Repository Working Directory:** `c:\hsz_projects\DePhobia-main\DePhobia-main`  
**Target Platform Selected:** Universal OpenXR Standalone VR (Meta Quest 2/3/Pro, Pico, HTC Vive, SteamVR/PCVR)  
**Target Unity LTS Version Selected:** Unity 2022.3 LTS  
**Target Render Pipeline:** Built-in Render Pipeline (preserves legacy shaders/materials without conversion breakage)  

---

## Executive Summary

DePhobia is an exposure therapy VR application originally developed for Android smartphones using the legacy Google Cardboard (GoogleVR) SDK on **Unity 2019.4.34f1**. It features three therapeutic exposure simulations:
1. **Claustrophobia:** Closed room where walls progressively contract inward over time.
2. **Aquaphobia:** Tropical island environment where surrounding sea levels progressively rise.
3. **Acrophobia:** Hoverboard elevating above a 3D city landscape, where confronting fear of heights (gazing downward) ascends the player and looking away descends the player.

While the core therapeutic concepts are functional in prototype form, the project as downloaded contains several critical architectural and operational blockers:
- **Zero scenes are configured in `EditorBuildSettings.asset`**, meaning standalone builds fail immediately to load any simulation.
- **The XR subsystem relies completely on the discontinued GoogleVR SDK** and legacy `UnityEngine.XR.XRSettings`, which cannot run on modern 6DoF VR headsets (Meta Quest, SteamVR, Vive, Pico) and is rejected on modern Android OS versions.
- **Settings and UI logic in `Update()`** cause frequent CPU stalls, per-frame garbage generation, and unhandled `FormatException` crashes when text fields are empty.
- **Aquaphobia parameters are hardcoded and commented out**, ignoring user settings entirely.
- **In-game session exit relies on the physical Android phone "Back" button** (`KeyCode.Escape`), leaving VR headset players trapped inside simulations with no way to pause or exit safely.
- **Project settings are unconfigured defaults** (Gamma color space instead of Linear, ARMv7 32-bit instead of ARM64, and Product Name set to `My project (3)`).

---

## 1. Technical Baseline & Environment Audit

| Parameter | Current State in Repository | Required Modern Target State |
| :--- | :--- | :--- |
| **Unity Version** | `2019.4.34f1` (`ProjectVersion.txt`) | **`2022.3 LTS`** (Industry standard for OpenXR) |
| **Render Pipeline** | Built-in Render Pipeline (`GraphicsSettings.asset`) | **Built-in Render Pipeline** (Linear Color Space) |
| **XR Subsystem** | Legacy GoogleVR + Built-in `XRSettings` | **Unity OpenXR Plugin** + **XR Interaction Toolkit 2.5+** |
| **Target Architectures**| Android ARMv7 (32-bit) | **ARM64 (v8a)** + Standalone Windows x64 |
| **Color Space** | Gamma (`m_ActiveColorSpace: 0`) | **Linear** (`m_ActiveColorSpace: 1`) (Required for VR) |
| **Input System** | Legacy `UnityEngine.Input` + GoogleVR Gaze | **Unity Input System** + **XRI Controllers & Fallback Keys** |
| **Git Version Control**| Not initialized (`.git` absent in folder) | **Git initialized (`main` baseline + `fix/ai-repair`)** |

---

## 2. Project Map & Inventory

### 2.1 Scenes
The project contains 5 scenes, none of which were registered in `EditorBuildSettings.asset`:
1. `Assets/Menu/Menu.unity` — Main Menu (Simulation selection, settings, patient stats, social links).
2. `Assets/Room/scenes/the last revelation.unity` — Claustrophobia room simulation.
3. `Assets/Snowconesolid Assets/Super Beach Pack/Scenes/Island Example 3.unity` — Aquaphobia island simulation.
4. `Assets/Geopipe/Pack01Demo/Pack01Demo01.unity` — Acrophobia city hoverboard simulation.
5. `Assets/Scenes/SampleScene.unity` — Default empty sample scene (to be excluded from builds).

### 2.2 Custom Project Scripts (20 Total)
- **Menu & Management:**
  - `Assets/Menu/mainmenu.cs`: UI button routing, scene loading by build index, and stats file reading.
  - `Assets/Menu/Getval.cs`: Reads Claustrophobia settings from UI input fields in `Update()`.
  - `Assets/Menu/Getval1.cs`: Reads Aquaphobia settings from UI input fields in `Update()`.
  - `Assets/Menu/Getval2.cs`: Reads Acrophobia settings from UI input fields in `Update()`.
  - `Assets/Menu/Valuefix.cs`: Hack to set default text if input field is blank in `Update()`.
  - `Assets/Menu/VrStop.cs`: Disables VR in Menu via `XRSettings.LoadDeviceByName("None")`.
  - `Assets/Menu/orientation.cs`: Forces `Screen.orientation = Portrait` in `Update()`.
  - `Assets/Menu/splashissue.cs`: Monitors `SplashScreen.isFinished` to toggle canvas visibility.
  - `Assets/Menu/FOLLOW.cs`: Social web links (`Application.OpenURL`), uses deprecated `WWW.EscapeURL`.
- **Simulation Mechanics:**
  - `Assets/Room/Scripts/StartVr.cs`: Enables Cardboard VR on Start, saves Claustro stats on `KeyCode.Escape`.
  - `Assets/Room/Scripts/VRLookWalk.cs`: Moves player forward when looking down between 30° and 90°.
  - `Assets/Room/Scripts/xposmove.cs`: Moves right wall inward along +X.
  - `Assets/Room/Scripts/xnegmove.cs`: Moves left wall inward along -X.
  - `Assets/wallstable.cs`: Offsets wall object relative to moving plane.
  - `Assets/poswallstable.cs`: Offsets opposite wall object relative to moving plane.
  - `Assets/StartVr2.cs`: Enables Cardboard VR on Start, saves Aqua stats on `KeyCode.Escape`.
  - `Assets/MoreHeight1.cs`: Water elevation rising logic in Aquaphobia.
  - `Assets/Menu/StartVr3.cs`: Enables Cardboard VR on Start, saves Acro stats on `KeyCode.Escape`.
  - `Assets/vrlook.cs`: Core Acrophobia gaze therapy mechanic (look down to ascend, look away to fall), sound triggers.
  - `Assets/ValChange.cs`: Acrophobia altitude calculation and HUD timer texts.

### 2.3 Third-Party Plugins & Assets
- `Assets/GoogleVR`: Legacy Google Cardboard/Daydream SDK. Contains 100+ deprecated scripts, obsolete Android JNI/AAR binaries, and legacy editor tools. **Must be completely removed and replaced with modern OpenXR.**
- `Assets/Geopipe`: 3D New York block city environment for Acrophobia. Clean, functional assets.
- `Assets/Snowconesolid Assets/Super Beach Pack`: Tropical island environment for Aquaphobia. Clean, functional models.
- `Assets/MobileDepthWater`: Mobile water shader (`DiffuseWaterOpaque.shader`).
- `Assets/Water Float`: Legacy standard assets water scripts and buoyancy.
- `Assets/Room`: 3D enclosed room environment for Claustrophobia. Clean models and textures.
- `Assets/TextMesh Pro`: Standard TMP assets.

---

## 3. Prioritized Issue Categorization

### Priority 0 (P0) — Blocks Build, Loading, or Launch
1. **Empty Build Settings (`EditorBuildSettings.asset`):**
   `m_Scenes: []` is empty. Calling `SceneManager.LoadScene(index)` in standalone builds fails catastrophically with an unhandled exception.
2. **Obsolete & Incompatible XR Stack (`GoogleVR` + `XRSettings`):**
   `XRSettings.LoadDeviceByName("cardboard")` and `Assets/GoogleVR` fail to compile on Unity 2022.3 LTS and cannot interface with Meta Quest, Pico, or SteamVR OpenXR runtimes.
3. **Locked Portrait Screen Orientation:**
   `Assets/Menu/orientation.cs` sets `Screen.orientation = Portrait` every frame in `Update()`, corrupting VR headset stereo projection.
4. **Unhandled Exceptions in Settings Input Parsing:**
   `float.Parse(spee.GetComponent<TMP_InputField>().text)` in `Getval.cs`, `Getval1.cs`, and `Getval2.cs` runs every frame in `Update()` without `try/catch` or `float.TryParse`. If an input field is empty or contains non-numeric text, Unity throws a continuous `FormatException` spam, freezing UI execution.
5. **No VR In-Game Exit or Pause Mechanism:**
   `StartVr.cs`, `StartVr2.cs`, and `StartVr3.cs` only check `Input.GetKeyUp(KeyCode.Escape)` on Android phone hardware. In a modern VR headset, patients have no way to exit a distressing simulation.

### Priority 1 (P1) — Broken or Severely Degraded Features
1. **Ignored Settings in Aquaphobia (`MoreHeight1.cs`):**
   Water speed and interval parameters from the menu are commented out:
   `speed = 0.06f; // Getval1.watspeed/100;`
   `timerate = 1; // Getval1.watermovetime;`
   `vtime = 1; // Getval1.watermoveduration;`
   The user's configuration is completely ignored.
2. **Infinite Coroutine Spam in Menu (`Getval1.cs`, `Getval2.cs`):**
   `if (mainmenu.flag == 1) { wa5(3.5f); }` runs inside `Update()`, firing 60 to 90 coroutines per second when Quit is triggered, causing rapid memory leaks.
3. **2D Screen Space Canvas in VR Headset:**
   `Assets/Menu/Menu.unity` renders canvas in Screen Space Overlay. In a VR headset, overlay canvases are stuck to the player's retinas, causing immediate double vision and nausea.
4. **Fragile Plaintext Stats Storage:**
   `persistentDataPath/PersonStats` writes unformatted tab-delimited text files (`ClausStats.txt`, `AquaStats.txt`, `AcroStats.txt`). A single missing value corrupts column alignments, and `mainmenu.cs` dumps the raw text into a single non-scrollable box that overflows off-screen after a few sessions.
5. **Gamma Color Space in VR:**
   `ProjectSettings.asset` is set to Gamma (`m_ActiveColorSpace: 0`). OpenXR and modern VR displays require Linear color space for correct lighting and anti-aliasing.

### Priority 2 (P2) — Half-Finished Systems & Incomplete Features
1. **Unbounded Freefall in Acrophobia:**
   When the player reaches max height (>360m) or ignores the look-down mechanic, the hoverboard disables kinematic state and drops infinitely through the city geometry without a graceful landing, recovery, or structured session summary.
2. **No Player Boundary Collision in Claustrophobia:**
   When walls contract, if the player stands in their path, the player mesh clips through the walls or is pushed out of the room geometry.
3. **Lack of Dual-Input Locomotion:**
   Claustrophobia and Aquaphobia only allow looking down at the ground to walk (`VRLookWalk.cs`). There is no support for VR controller thumbstick smooth locomotion, snap turning, or desktop keyboard (WASD) testing.
4. **Missing VR Comfort & Nausea Protection:**
   No snap/smooth turn toggles, no FOV reduction tunneling (comfort vignette) during movement or vertical height shifts, and no floor recenter calibration.
5. **Abrupt Scene Switching:**
   Scenes load instantaneously without screen fades, causing visual stutter and VR sickness during scene transitions.

### Priority 3 (P3) — Performance Hotspots & Polish
1. **Excessive Garbage Allocation & `GetComponent` in `Update()`:**
   `Getval.cs`, `Getval1.cs`, `Getval2.cs`, `Valuefix.cs`, and `ValChange.cs` call `GetComponent<TMP_InputField>()` and `GetComponent<TextMeshProUGUI>()` and perform string concatenations every single frame.
2. **Audio Unpooling & Deprecated APIs:**
   `Assets/vrlook.cs` invokes `AudioSource.PlayClipAtPoint` with unpooled audio instances. `Assets/Menu/FOLLOW.cs` calls deprecated `WWW.EscapeURL`.
3. **Unconfigured Identity Settings:**
   Company Name: `DefaultCompany`, Product Name: `My project (3)`, Application Identifier: blank.
4. **Missing Readme Documentation & Build Instructions:**
   Current `README.md` only documents downloading a legacy 2021 APK for Android phones.

---

## 4. Proposed Architectural Solutions

1. **Modern XR Subsystem:**
   - Remove `Assets/GoogleVR`.
   - Install `com.unity.xr.openxr` and `com.unity.xr.interaction.toolkit` (v2.5.4) via `Packages/manifest.json`.
   - Implement a unified, reusable `VRPlayerRig` prefab containing:
     - Main Camera with OpenXR Tracked Pose Driver.
     - Left and Right Controller Interactors (Ray for UI + Direct Interactor).
     - Smooth Locomotion + Snap/Smooth Turn Provider.
     - Tunneling Comfort Vignette.
     - Desktop Keyboard/Mouse Fallback (WASD movement + Mouse Look) for seamless testing in Unity Editor.
2. **Exposure Therapy Mechanics Preservation:**
   - Preserve the authentic **Look-Down Exposure Therapy Mechanic** in Acrophobia (`vrlook.cs`): gazing downward at the city triggers hoverboard ascension; looking away triggers descent.
   - Refine angle detection for 6DoF head tracking with smooth lerping.
   - Provide clean boundary clamping and a graceful session ending sequence at peak altitude.
3. **Patient Safety & Pause Menu:**
   - Create an in-game VR Floating Pause Menu accessible via VR Controller Menu/Y button or keyboard `Esc`.
   - Add an in-world physical emergency "Exit Session" beacon within player view for immediate comfort.
4. **Unified Settings & Session Persistence Architecture:**
   - Replace `Getval`/`Getval1`/`Getval2`/`Valuefix` with an event-driven `TherapySettingsManager` that loads and persists patient configurations via JSON/PlayerPrefs.
   - Create a structured `PatientSessionManager` recording timestamps, duration, therapy settings, and achievements into structured data with a scrollable history UI in the main menu.
5. **World-Space VR Main Menu:**
   - Convert `Assets/Menu/Menu.unity` Canvas from Screen Space Overlay to a comfortable 3D World Space panel with ray pointer interaction.
6. **VR Screen Transitions:**
   - Implement a smooth `VRFadeManager` providing gentle fade-to-black scene transitions to prevent headset visual stutter.

---

## 5. Verification Checklist for Subsequent Phases
- [ ] Unity 2022.3 LTS package manifest resolves with zero package conflicts.
- [ ] Zero compile errors and zero compiler warnings.
- [ ] All 4 scenes properly registered in `EditorBuildSettings.asset`.
- [ ] All 3 simulations playable via VR Controllers and Desktop Keyboard/Mouse.
- [ ] Patient settings persist across scene loads and app restarts.
- [ ] Pause menu and emergency exit button function cleanly in all simulations.
- [ ] Standalone build targets configured (Android ARM64 for Quest/Pico, Windows x64 for PCVR).
