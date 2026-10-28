# Forest Mineral & Sage UI Redesign Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Completely redesign the DePhobia UI across the Main Menu, active exposure sessions, pause menu, and completion states into a calm, confident, restraint-driven experience styled under the "Forest Mineral & Sage" design language with zero blue or violet tones.

**Architecture:** 
- Centralized `ThemeTokens.cs` defines the palette and typography tokens.
- `mainmenu.cs` drives a single-surface central panel in 3D world space with 3 top tabs (`Enclosed Space`, `Rising Water`, `High Elevation`), inline intensity presets (`Gentle`, `Standard`, `Deep`), and a prominent `Begin Session` action, with in-place swaps for History and Settings.
- Active exposure HUD clutter is eliminated in favor of somatic haptic/audio cues and an ethereal horizon breathing ring with gaze-dwell emergency exit.
- In-game pause is reduced to a strict, elegant two-button slate (`Resume`, `Return to Menu`).
- Milestones trigger an immediate calm fade to a concise closure card.

**Tech Stack:** Unity 2022.3 LTS, C#, Unity UI (uGUI), TextMeshPro, OpenXR.

## Global Constraints
- Zero blue or violet shades anywhere in the UI.
- Palette: Earth Charcoal (`#141614`), Dark Spruce Moss (`#1E221E`), Lichen Border (`#343A34`), Warm Off-White (`#F3F5F2`), Ash Olive (`#9AA299`), Matcha Sage (`#76936A`), Umber Clay (`#AD6E60`).
- Typography: Sentence case / Title Case, zero shouting ALL CAPS, zero exclamation marks, high-contrast readable type scale.
- Zero floating HUD numbers during active exposure.
- Surgical changes only: preserve existing simulation physics, boundaries, and scene hierarchy contracts.

---

### Task 1: Theme Tokens & Typography System

**Files:**
- Create: `Assets/Scripts/Core/ThemeTokens.cs`
- Modify: `DESIGN_SYSTEM.md`

**Interfaces:**
- Produces: `ThemeTokens` static class with `CanvasBg`, `SurfacePanel`, `SurfaceBorder`, `TextPrimary`, `TextSecondary`, `AccentSage`, `GentleExitClay`, `SurfaceInactive`.

- [ ] **Step 1: Create `ThemeTokens.cs`**
Implement centralized color tokens and UI styling helpers for Forest Mineral & Sage.

- [ ] **Step 2: Update `DESIGN_SYSTEM.md`**
Update design system documentation to replace Direction C with the approved Forest Mineral & Sage specification.

- [ ] **Step 3: Verification**
Verify script compiles with zero syntax errors.

---

### Task 2: Main Sanctuary Portal (Single Center Panel with 3 Tabs)

**Files:**
- Modify: `Assets/Menu/mainmenu.cs`
- Modify: `Assets/Scripts/VR/VRWorldSpaceCanvasHelper.cs`

**Interfaces:**
- Consumes: `ThemeTokens`, `TherapySettingsManager`, `PatientSessionManager`, `VRFadeManager`.
- Produces: Event-driven 3-tab portal (`SelectTab(int index)`), inline intensity presets (`SetPreset(string preset)`), `StartSelectedSession()`, `ShowHistory()`, `ShowSettings()`, `ShowMainPanel()`.

- [ ] **Step 1: Rewrite `mainmenu.cs` to dynamically construct and control the clean single-surface Sanctuary UI**
Construct the single center card with 3 tabs at top, clear description, 3 intensity buttons, and the big Sage `Begin Session` button. Provide in-place swap views for History and Comfort/Settings. Decommission legacy float inputs and social media links.

- [ ] **Step 2: Verify `VRWorldSpaceCanvasHelper.cs` calibration**
Ensure canvas is positioned at `1.8m` distance at eye level (`y = 0` relative to eye) with sharp uniform scale `0.0022f`.

- [ ] **Step 3: Verification**
Verify compilation and test tab switching, preset selection, in-place view swaps, and scene launching.

---

### Task 3: In-Session Visual Restraint & Horizon Respiration Ring

**Files:**
- Modify: `Assets/ValChange.cs`
- Modify: `Assets/vrlook.cs`
- Create: `Assets/Scripts/VR/VRHorizonRespirationRing.cs`
- Modify: `Assets/Scripts/VR/VREmergencyExitBeacon.cs`
- Modify: `Assets/Scripts/VR/TherapyCaptionUI.cs`

**Interfaces:**
- Consumes: `ThemeTokens`, `PatientSessionManager`, `VRFadeManager`.
- Produces: `VRHorizonRespirationRing` providing passive 4.5s breathing guide + 1.5s gaze-dwell safe exit.

- [ ] **Step 1: Strip floating HUD numbers from Acrophobia in `ValChange.cs` and `vrlook.cs`**
Hide or deactivate floating text meshes (`Height:`, `Time Elasped:`, `Wait Time:`, `Falling Focus Time:`, `Still Focus Time:`). Keep underlying altitude and timer calculations active for session logging and kinematics.

- [ ] **Step 2: Create `VRHorizonRespirationRing.cs`**
Render a subtle, ethereal breathing ring at the lower edge of the FOV. Cosine wave breathing rhythm (4.5s). Looking down at it for 1.5s smoothly transitions the user back to the menu.

- [ ] **Step 3: Update `TherapyCaptionUI.cs` and `VREmergencyExitBeacon.cs`**
Silence unsolicited toast popups during active therapy; defer to subtle somatic haptics and ambient sound cues.

- [ ] **Step 4: Verification**
Verify scripts compile cleanly; check that active exposure runs with zero text obstruction.

---

### Task 4: Minimalist Two-Button Pause Screen & Milestone Closure

**Files:**
- Modify: `Assets/Scripts/VR/VRInGamePauseMenu.cs`
- Modify: `Assets/Scripts/VR/TherapySessionRitual.cs`
- Modify: `Assets/Room/Scripts/StartVr.cs`
- Modify: `Assets/StartVr2.cs`
- Modify: `Assets/Menu/StartVr3.cs`

**Interfaces:**
- Consumes: `ThemeTokens`, `VRPlayerRig`, `PatientSessionManager`, `VRFadeManager`.
- Produces: Clean 2-button Pause Slate (`Resume`, `Return to Menu`) and immediate calm fade to milestone summary card.

- [ ] **Step 1: Refactor `VRInGamePauseMenu.cs`**
Render a minimalist two-button slate styled in Forest Mineral & Sage. Prominent Sage `Resume Session` button, quiet Umber Clay `Return to Menu` button, and elapsed time. Recenter view and seated mode mapped to thumbstick click and `R` key.

- [ ] **Step 2: Update milestone completion in `StartVr.cs`, `StartVr2.cs`, `vrlook.cs`, and `TherapySessionRitual.cs`**
When safe boundary/summit is reached, smoothly halt motion, trigger gentle haptic pulse, and calmly fade to a clean summary card showing duration and peak metric with a single button to return to the Sanctuary menu.

- [ ] **Step 3: Verification**
Verify pause triggering via Esc/Menu button, smooth resumption, and graceful milestone closure.

---

### Task 5: Final Coherence Audit & Verification

**Files:**
- Inspect all modified files across `Assets/`.
- Verify build scenes and script bindings.

- [ ] **Step 1: Check compilation and console output across all scenes.**
- [ ] **Step 2: Perform regression testing on locomotion, camera rig, and session persistence.**
- [ ] **Step 3: Prepare the two-line closing insight.**
