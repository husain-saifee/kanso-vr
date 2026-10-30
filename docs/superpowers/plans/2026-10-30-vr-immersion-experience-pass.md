# DePhobia VR Experience Pass — Phased Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:executing-plans` to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Transform DePhobia from a technically functional state machine into a living, tactile, spatially convincing physical place where absence of motion is eliminated and every interaction carries physical weight and acoustic presence.

**Architecture:** 
1. **Acoustic Spine & Room Resonance:** Couple `TherapyAudioManager` with dynamic `AudioReverbZone` profiles, real-time proximity low-pass filtering (compressing room walls, water submersion, high-altitude wind shear), and seamless 432 Hz harmonic bed continuity.
2. **Kinematic Weight & Micro-Inertia:** Replace raw linear transform translations (`+= speed * dt`) with critically damped spring dampers (`Mathf.SmoothDamp`), microscopic aerodynamic buoyancy (<0.18° sway, 1.2cm float), and wall friction/creak deceleration curves.
3. **Tactile Interaction & Point of Contact:** Provide a damped spatial reticle, 3D button physical depression on press, unscaled-time spring canvas arrivals (settle instead of pop), and dual-stage haptics (hover tick + click impulse).
4. **Continuous Spatial Transitions:** Cinema blackout with 350ms environmental audio pre-roll so the ears hear the room geometry before the eyes see it.

**Tech Stack:** Unity 2022.3 LTS, OpenXR, Unity Audio Engine (`AudioReverbZone`, `AudioLowPassFilter`), C# kinematic spring math.

## Global Constraints
- Target platform: OpenXR (Meta Quest 2/3/Pro, PCVR, Pico) + Desktop fallback.
- Strictly adhere to vestibular comfort: no sudden accelerations, tilt < 0.2°, no uncommanded horizontal camera displacement.
- Design System: Forest Mineral & Sage (`ThemeTokens.cs`), zero blue/violet tones.
- No feature creep: focus entirely on sensory coherence, felt weight, acoustic depth, and interaction feedback.

---

## Phase 1: Acoustic Spine & Dynamic Room Geometry (Depth of Place)
*The room should sound like it has walls before you see them.*

### Task 1.1: Dynamic Environmental Reverb & Acoustic Filtration in `TherapyAudioManager`
**Files:**
- Modify: `Assets/Scripts/Core/TherapyAudioManager.cs`
- Create / Inspect: `Assets/Resources/Audio/`

**Interfaces:**
- Produces: `TherapyAudioManager.SetRoomProximity(float normalizedProximity)`, `TherapyAudioManager.SetWaterSubmersion(float waterLevelRelative)`, `TherapyAudioManager.SetAcrophobiaAltitude(float currentY)`
- Consumes: Scene boundary changes and player height from simulation scripts.

- [x] **Step 1:** Add `AudioReverbZone` setup and dynamic `AudioLowPassFilter` management to `TherapyAudioManager`.
- [x] **Step 2:** Implement scenario-specific acoustic profiles:
  - **Claustrophobia:** Acoustic confinement mode (early reflections tighten, high frequencies dampen as walls contract).
  - **Aquaphobia:** Subtle surface shore lap spatial blend, plus soft low-pass submersion shift when water reaches chest height.
  - **Acrophobia:** Atmospheric pressure drop curve (gradual wind buffeting surge and distant low-frequency rumble falloff as altitude approaches 360m).
- [x] **Step 3:** Compile and verify zero audio bus clipping or static distortion.

---

## Phase 2: Environmental Kinematics & Calibrated Micro-Inertia (Physics of Belief)
*Things should arrive, not pop; settle, not stop; carry weight through anticipation and follow-through.*

### Task 2.1: Acrophobia Hoverboard Damped Springs & Micro-Buoyancy
**Files:**
- Modify: `Assets/vrlook.cs`

- [x] **Step 1:** Replace instant linear ascent/descent (`currentY += speed * Time.deltaTime`) with critically damped spring interpolation (`Mathf.SmoothDamp`).
- [x] **Step 2:** Add microscopic aerodynamic hover buoyancy (gentle 1.2cm vertical breathing oscillation and smooth <0.18° roll/pitch sway via Perlin noise) to convey floating in open air without vestibular discomfort.
- [x] **Step 3:** Implement organic gaze-away deceleration (settle to a soft stop over 0.6s rather than hard braking).

### Task 2.2: Claustrophobia Structural Resistance & Wall Deceleration
**Files:**
- Modify: `Assets/Room/Scripts/xposmove.cs`
- Modify: `Assets/Room/Scripts/xnegmove.cs`

- [x] **Step 1:** Replace constant velocity clamping with smooth deceleration curve as walls reach safe clamping limit (`-2.85m`).
- [x] **Step 2:** Link wall movement velocity to `TherapyAudioManager.SetRoomProximity` and trigger subtle mechanical friction / structural creak audio during travel.

### Task 2.3: Aquaphobia Water Tension & Wave Lap
**Files:**
- Modify: `Assets/MoreHeight1.cs`

- [x] **Step 1:** Replace stepped velocity jumps with gentle sinusoidal tide modulation on vertical rise.
- [x] **Step 2:** Hook water elevation to `TherapyAudioManager.SetWaterSubmersion` to smoothly trigger spatial shore lap sound when water crosses player ankle/knee threshold.

---

## Phase 3: Spatial Interaction, Pointer Reticle & 3D Button Depth (The Point of Contact)
*Every touch needs an answer — a reaction at the point of contact, a sound, a small physical acknowledgement.*

### Task 3.1: Damped Spatial Reticle & Hover Feedback in `VRPlayerRig`
**Files:**
- Modify: `Assets/Scripts/VR/VRPlayerRig.cs`
- Create: `Assets/Scripts/VR/VRSpatialReticle.cs`

- [ ] **Step 1:** Implement a subtle world-space reticle/pointer with micro-damping that projects onto UI canvases and in-world interactables.
- [ ] **Step 2:** Fire a light haptic tick (`amplitude = 0.08f, duration = 0.02f`) whenever the reticle enters or leaves a clickable element.

### Task 3.2: 3D Physical Button Depression & Tactile Resistance
**Files:**
- Create: `Assets/Scripts/VR/VRTactileButton.cs`
- Modify: `Assets/Scripts/Core/ThemeTokens.cs`

- [ ] **Step 1:** Implement `VRTactileButton` providing 2-3mm physical z-depression on press with spring return follow-through.
- [ ] **Step 2:** Synchronize click audio chime and distinct press haptic impulse (`0.25f, 0.06f`) at the exact point of bottoming out.

### Task 3.3: Spring-Physics Canvas Arrival & Dismissal
**Files:**
- Create: `Assets/Scripts/VR/VRSpringCanvas.cs`
- Modify: `Assets/Scripts/VR/VRInGamePauseMenu.cs`
- Modify: `Assets/Scripts/VR/TherapySessionRitual.cs`

- [ ] **Step 1:** Implement `VRSpringCanvas` using unscaled time (`Time.unscaledDeltaTime`) so panels smoothly scale/ease from 0.90 to 1.00 with organic damping even when paused.
- [ ] **Step 2:** Eliminate abrupt pop-in (`pauseCanvas.SetActive(true)`) and replace with smooth spring arrival and collapse dismissal.

---

## Phase 4: Transitions with Audio Pre-Roll & Final Presence Audit
*The room should sound like it has walls before you see them.*

### Task 4.1: Environmental Audio Pre-Roll in `VRFadeManager`
**Files:**
- Modify: `Assets/Scripts/VR/VRFadeManager.cs`

- [ ] **Step 1:** Implement 350ms environmental audio pre-roll: when loading a new simulation, initialize and fade in destination ambient acoustics before clearing the visual cinema black fade.
- [ ] **Step 2:** Maintain unbroken 432 Hz harmonic drone across the transition.

### Task 4.2: First 30-Second Presence Audit & Verification
- [ ] **Step 1:** Run full Unity compilation check across all modified scripts.
- [ ] **Step 2:** Verify headset-on checklist: do nothing for 60 seconds in each scene; verify room acoustics, physical motion settle, button contact physics, and transition pre-roll.
- [ ] **Step 3:** Formulate the final two honest lines: what bought back the most presence, and what still breaks.
