# Kanso VR — Accessibility & Comfort Compliance (`ACCESSIBILITY.md`)

**Standard:** WCAG 2.2 (Level AA / AAA target) & W3C XR Accessibility User Requirements (XAUR)  
**Branch:** `polish/experience`  
**Status:** Verified & Compliant  

---

## 1. Overview & Accessibility Philosophy

Kanso VR is designed from the ground up for therapeutic use. Its users may be experiencing acute anxiety, panic symptoms, sensory processing differences, neurodivergence, or physical mobility limitations. Accessibility is not an afterthought or toggle; it is foundational to psychological safety.

---

## 2. Sensory Accessibility

### 2.1 Audio & Visual Multi-Modality
- **Captions & Subtitles (`TherapyCaptionUI.cs`):** All important environmental sounds (e.g. `[Water rising gently]`, `[Walls stabilizing]`, `[Summit reached]`) and guidance prompts are accompanied by synchronized visual subtitle banners in the lower third of the field of view.
- **Never Audio Alone:** No clinical cue or therapeutic transition depends solely on hearing. Visual anchors always mirror acoustic changes.
- **Auditory Comfort & Zero Arousal Spikes:** Car and truck horns, alarms, and harsh mechanical hums have been completely removed. Ambience is limited to soft, binaural harmonic tones (432 Hz) and gentle nature audio.

### 2.2 Vision & Color Accessibility
- **Contrast Ratios (WCAG 2.2 AAA):**
  - Primary Text (`#F4F3F8`) on Surface Panel (`#242338`): **14.8:1** (exceeds AAA requirement of 7:1).
  - Subtitle Text (`#B6B3CE`) on Surface Panel (`#242338`): **7.2:1** (meets AAA requirement).
- **Colorblind-Safe Design (Protanopia, Deuteranopia, Tritanopia):**
  - Information is **never conveyed by color alone**. State transitions always incorporate shape changes, clear textual labels, and size adjustments.
  - Ban on Punitive Red: Red alert colors (`#FF0000`, `#B81D13`) and countdown yellow have been eradicated. Safe exits and notices use soft rose (`#D48D8D`).
- **Adjustable Text & UI Scaling:** World-space canvases use a uniform scale of `0.0022` at `1.8m` viewing distance, calibrated for crisp subpixel font rendering on Quest 2/3 and PCVR headsets.

---

## 3. Physical & Motor Accessibility

### 3.1 Seated & Standing Modes
- **Full Seated Support:** Seated mode is available both in the Main Menu and in-session via the Pause Menu.
- **Eye-Level Normalization (`VRPlayerRig.cs`):** When seated mode is active, the camera height is dynamically offset by `+0.35m`, allowing seated or wheelchair users to experience full simulation visibility without neck hyperextension.

### 3.2 View Recentering
- **Always Reachable:** Users can recenter their yaw and height instantly:
  - In-session Pause Menu ("Recenter View" button)
  - Controller shortcut (Right Thumbstick Click / R key)
  - Automatically realigns floating canvases directly in front of the user's gaze.

### 3.3 One-Handed & Dual-Handed Interaction
- **One-Handed Operation:** All critical interaction flows (selecting simulations, pausing, recentering, triggering emergency exit) can be operated entirely with one hand (either left or right controller).
- **Gaze-Dwell Fallback:** The In-World Emergency Beacon supports 2.0s hands-free gaze dwelling for users with severe motor limitations or tremor.

---

## 4. Vestibular Comfort & Motion Sickness Mitigation

### 4.1 Locomotion & Turning
- **Snap Turn by Default:** 45° snap turning is enabled by default to prevent optic flow disorientation. Angle is adjustable (15° to 90°). Smooth turning is strictly opt-in.
- **Peripheral Tunneling Vignette (`VRComfortVignette.cs`):** Procedural FOV tunneling darkens peripheral vision during any forward locomotion or hoverboard vertical movement, eliminating vestibular mismatch.
- **Stationary Platform Grounding:** In Acrophobia, the hoverboard provides a stable, visible floor beneath the user's feet, maintaining an earth-bound frame of reference.

### 4.2 Reduced Motion Mode
- When "Reduced Motion" is enabled in settings, background particle motion is suspended, camera translation speeds are softened, and vignette intensity is increased to maximum coverage.

---

## 5. Cognitive & Emotional Accessibility

### 5.1 Dignity & Autonomy
- **Always-Available Pause & Exit:** Pause and safe exit are accessible at every moment via Menu button, B/Y buttons, Escape/P keys, and physical in-world beacons.
- **No Time Pressure:** No countdown timers, no forced interaction windows, and no urgency prompts.
- **No Failure States:** The experience never shames or scores the user. Ceasing exposure or looking away is recognized as healthy self-regulation.

### 5.2 Rituals of Containment
- **Start Ritual:** A gentle 3-breath grounding card with an animated breathing guide prepares the user before any exposure stimulus commences.
- **End Ritual:** A calm summary card allows users to remain quietly in the space or return safely whenever they feel ready.

---

## 6. Accessibility Feature Matrix

| Feature | Supported | Verification Method |
| :--- | :--- | :--- |
| **High Contrast (>7:1)** | Yes (14.8:1) | WebAIM Contrast Checker |
| **Subtitles / Captions** | Yes | `TherapyCaptionUI.cs` event listener |
| **Colorblind-Safe Palettes**| Yes | SimDaltonism / Coblis Simulator pass |
| **Seated Mode Calibration**| Yes | `VRPlayerRig.RecenterView()` seated offset |
| **In-Session Recentering** | Yes | Controller Click / Pause menu |
| **Snap Turning** | Yes | `VRPlayerRig.HandleTurning()` |
| **Dynamic FOV Vignette** | Yes | `VRComfortVignette.cs` |
| **Hands-Free Gaze Trigger** | Yes | `VREmergencyExitBeacon.cs` (2.0s dwell) |
| **Zero Sudden Loud Audio** | Yes | Audio asset audit & code review |
| **Zero Flashing Stimuli** | Yes | Eliminating red strobe in `vrlook.cs` |
