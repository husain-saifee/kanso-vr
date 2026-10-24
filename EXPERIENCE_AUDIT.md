# DePhobia — Phase 1 Experience Audit Report
**Scope:** Whole-experience review across First-Run, Moment-to-Moment Feel, Failure Modes, Coherence, Accessibility, Comfort, Flow Structure, and Tone of Voice.  
**Evaluator:** Senior XR Experience Designer & Unity Engineer (Therapeutic & Accessibility XR Focus)  
**Branch:** `polish/experience`  
**Date:** October 2026  

---

## Executive Summary

The previous engineering pass successfully stabilized the project: Unity 2022.3 LTS compiles cleanly, OpenXR and XRI are configured, build scenes are registered, and core simulation logic functions without crashes.

However, from an **XR therapy, human factors, and emotional safety perspective**, the experience currently feels **assembled from disparate asset store kits and debug testbeds**, rather than designed as a cohesive, calm, and trustworthy therapeutic medical device. Critical issues exist:
- **P0 Safety/Comfort:** Loud sudden car/truck horn blares and grinding mechanical noises spike autonomic arousal; unparented camera rig bugs in Acrophobia break the core elevator mechanic; HUD countdowns turn flashing warning red; and the in-world emergency exit beacon is never instantiated in any scene.
- **P1 Accessibility:** Zero subtitles/captions exist; colors (red/yellow/white) are used alone to communicate urgency; seated mode and recenter functions are inaccessible from the UI; and pause menus rely on legacy controller button bindings that fail on modern OpenXR runtimes.
- **P2 Coherence & Polish:** The four scenes exhibit four completely conflicting art styles (retro low-poly room, cartoon beach, photorealistic GIS city, unstyled developer canvas); audio tracks range from upbeat corporate ukulele to buzzing hard drives; and UI copy is riddled with misspellings, raw float input fields, and personal social media links asking for patient contact details.

Below is the exhaustive, severity-tagged breakdown across every scene and flow, followed by the **Top 10 High-Impact Changes**.

---

## 1. Severity-Tagged Findings by Domain

### A. First-Run & First 60 Seconds
| Severity | Scene / Area | Finding | Impact & Context |
| :--- | :--- | :--- | :--- |
| **P0** | **Main Menu** | **Menu Canvas Disconnected from Camera:** `VRWorldSpaceCanvasHelper` hardcodes canvas world position to `(0, 1.5, 2.4)`, whereas `Main Camera` in `Menu.unity` is at `(695, 577, -11.418)`. | On first launch, the UI canvas spawns 700+ units away from the camera, resulting in an empty void or misplaced canvas depending on headset tracking origin. |
| **P1** | **Main Menu** | **No Grounding, Welcoming Sequence, or Hardware Check:** Experience immediately drops user into an unstyled menu with acoustic ukulele music (`bensound-creativeminds`). | A patient in acute distress is given no breath-in moment, no explanation of how DePhobia works, and no seated/standing height calibration. |
| **P2** | **Main Menu** | **Social Media & Privacy Violations (`FOLLOW.cs`):** Menu features buttons for `INSTAGRAM`, `GITHUB`, and a `GMAIL` mailto link auto-generating an email asking for patient "Name:" and "Address:". | Severe violation of medical data privacy and professional clinical expectations. Destroys clinical trust immediately. |
| **P2** | **Main Menu** | **Spelling Errors & Unfriendly Terminology:** Buttons and headers read `CLAUSTROPHOBHIA` (extra 'H'), `PERSON STATUS`, `STILL FOCUS TIME`, and `WATER RISE RATE`. | Signals neglect and confusion; patients and therapists feel they are using a half-finished student prototype. |

---

### B. Moment-to-Moment Feel & Cognitive Load
| Severity | Scene / Area | Finding | Impact & Context |
| :--- | :--- | :--- | :--- |
| **P0** | **Acrophobia** | **Arcade-Like Cognitive Overload & Arousal-Spiking HUD:** Real-time HUD displays 5 simultaneous floating numbers (`Height:`, `Time Elasped:`, `Wait Time:`, `Falling Focus Time:`, `Still Focus Time:`). | Floods the patient's visual field with high-frequency telemetry. When the patient looks away from heights, `Wait Time` counts down and turns **warning yellow and alarming red** (`ValChange.cs:123`), creating punitive panic. |
| **P0** | **Acrophobia** | **Flashing Red Text & Forced 60-Second Ending Trap:** Upon reaching 360m, `vrlook.cs:229` alternates text between bright red (`#B81D13`) and white every second (`MAXIMUM ALTITUDE REACHED!`), then traps the user in an unskippable 30-second countdown (`Session Concluding in 30s`). | Direct violation of therapy guardrails: flashing lights spike seizure/anxiety risk; forced waits with countdowns induce claustrophobic entrapment. |
| **P1** | **Claustrophobia** | **Total Ambiguity & Abrupt Sensory Start:** Spawns patient in a dark room with zero instructions, breathing cues, or status indicators. Walls begin contracting silently or with mechanical hums after an arbitrary delay. | Patient has no cognitive anchor for what is expected of them, how long the exposure lasts, or when it will stabilize. |
| **P1** | **Aquaphobia** | **Absence of Interaction Clarity & Boundary Feedback:** Water rises gradually, but patient has no clear objective or tactile feedback. Patient can walk through rising water with zero physical or perceptual response. | Weak therapeutic immersion; feels like a static asset showcase rather than guided gradual exposure. |
| **P2** | **All Simulations** | **Zero Interactive Audio/Haptic Acknowledgement:** Hovering or clicking buttons produces no sound and no controller haptic feedback. | Inputs feel spongy, unresponsive, and uncertain (<100ms acknowledgment target is unmet). |

---

### C. Failure Modes & Edge Cases
| Severity | Scene / Area | Finding | Impact & Context |
| :--- | :--- | :--- | :--- |
| **P0** | **All Simulations** | **Orphaned Camera & Decoupled Player Rig Bug:** `StartVr.cs`, `StartVr2.cs`, and `StartVr3.cs` instantiate a runtime `VRPlayerRig` at `(0,0,0)` and reparent `Camera.main` away from existing scene hierarchies. | **In Acrophobia, this unparents the camera from the hoverboard.** The hoverboard ascends while the patient's camera remains pinned near ground level. In Claustrophobia and Aquaphobia, duplicate `CharacterController` components conflict. |
| **P0** | **All Simulations** | **Emergency Exit Beacon Never Spawned:** Although `VREmergencyExitBeacon.cs` exists in scripts, it is **not placed in any scene** and is never dynamically instantiated. | Patients in distress who cannot find or press controller menu buttons have no in-world physical rescue beacon despite documentation claiming it exists. |
| **P1** | **All Simulations** | **Unreliable Pause Menu Input (`KeyCode.JoystickButton7`):** `VRInGamePauseMenu.cs` checks legacy Unity Input Manager `JoystickButton7`. | On Meta Quest and modern OpenXR runtimes, this button frequently fails to trigger, leaving patients unable to pause or exit mid-simulation. |
| **P1** | **All Simulations** | **No Headset Removal or Tracking Loss Safeguards:** No listeners for `Application.focusChanged`, `InputTracking.trackingLost`, or headset proximity sensor. | If a distressed user pulls the headset off, the simulation continues running in the background indefinitely. |

---

### D. Visual, Lighting & Material Coherence
| Severity | Scene / Area | Finding | Impact & Context |
| :--- | :--- | :--- | :--- |
| **P1** | **Cross-Project** | **Severe Aesthetic Disconnection (4 Clashing Worlds):**<br>• Menu: Flat Unity UI canvas in an empty blue-gray skybox.<br>• Claustrophobia: Dark, muddy, unlit brown indoor textures.<br>• Aquaphobia: Highly saturated, cartoon low-poly tropical beach.<br>• Acrophobia: Gritty, photorealistic GIS photogrammetry city. | Does not feel like one cohesive medical platform. Transitions between scenes feel like jarring jumps across disparate unrelated video games. |
| **P2** | **Lighting / Materials** | **Harsh Specular Reflections & Uncalibrated Exposure:** In Aquaphobia, legacy `Water Float` specular shaders cause bright shimmering reflections; in Claustrophobia, lighting is unnaturally dark with abrupt falloff. | Causes eye fatigue and visual strain over 10–20 minute sessions. |

---

### E. Audio Design & Acoustic Safety
| Severity | Scene / Area | Finding | Impact & Context |
| :--- | :--- | :--- | :--- |
| **P0** | **Acrophobia** | **Startling Car Horn & Traffic Blare Spikes (`vrlook.cs:212`):** Plays `mixkit-car-horn-718`, `720`, `1565`, and doppler car horns at volume **1.4** randomly directly at the user. | Acute autonomic arousal spike. In heights exposure therapy, sudden loud horn honks induce involuntary startle responses, directly endangering emotional safety. |
| **P0** | **Claustrophobia** | **Jarring Mechanical Hard-Drive Noise (`Room Tone - Small, Hard Drive Noise...`):** Ambient room audio is literally a buzzing, grinding computer hard-drive sound loop. | Irritating and disorienting rather than a calm, grounded acoustic baseline. |
| **P1** | **Cross-Project** | **No Audio Mixer Hierarchy or Category Sliders:** Audio is fired via `AudioSource.PlayClipAtPoint` or unrouted sources. | Master volume affects everything equally; user cannot mute sound effects while keeping voice guidance or ambient nature sounds. |
| **P2** | **Main Menu** | **Upbeat Corporate Ukulele Music (`bensound-creativeminds`):** Bright acoustic strums. | Incompatible with clinical calm and mindfulness. |

---

### F. Accessibility & Comfort Pass
| Severity | Scene / Area | Finding | Impact & Context |
| :--- | :--- | :--- | :--- |
| **P1** | **Cross-Project** | **Zero Subtitles or Audio Captions:** No speech, sound effect, or event captions exist. | Fails deaf and hard-of-hearing users, and violates WCAG / XR accessibility guidelines. |
| **P1** | **Cross-Project** | **Information Conveyed Solely via Color:** Acrophobia urgency relies exclusively on white → yellow → red text transitions; Pause Menu buttons rely on green/red distinction. | Inaccessible to colorblind patients (protanopia/deuteranopia) and inherently anxiety-inducing. |
| **P1** | **VR Comfort** | **Seated Mode & Recenter Inaccessible:** `seatedMode` exists in `TherapySettingsManager`, but has no UI toggle and no camera height recalibration logic. `VRPlayerRig.RecenterHeight()` is never hooked to any input. | Inaccessible to wheelchair users, seated patients, or users who drift from physical tracking origin. |
| **P2** | **VR Comfort** | **Snap Turn & Vignette Controls Not Exposed in UI:** While `VRComfortVignette` works, patients cannot adjust snap angle (30° vs 45°) or vignette intensity from the in-game pause menu. | Users prone to motion sickness cannot customize comfort parameters during a session. |

---

### G. Structure, Rituals & Flow Architecture
| Severity | Scene / Area | Finding | Impact & Context |
| :--- | :--- | :--- | :--- |
| **P1** | **All Simulations** | **Missing Start Ritual:** Entering any simulation immediately starts the mechanical progression with zero preparation, grounding breath, or "Ready to Begin" gate. | Exposure therapy requires patient self-regulation before stimulus onset; sudden starts induce avoidance. |
| **P1** | **All Simulations** | **Missing End Ritual & De-escalation:** Sessions either end abruptly with an immediate scene jump to menu, or trap the user in a forced countdown without a reflective de-escalation summary. | Patients return to the menu with lingering sympathetic nervous system arousal. |
| **P2** | **Navigation** | **Settings Hierarchy Clutter:** Main Menu contains 3 separate configuration submenus with raw float input boxes and confusing back navigation. | Unnecessary cognitive friction before beginning therapy. |

---

### H. Tone of Voice & Copy Analysis
| Severity | Scene / Area | Finding | Impact & Context |
| :--- | :--- | :--- | :--- |
| **P2** | **All UI Copy** | **All-Caps Shouting & Inconsistent Tone:** `PLAY`, `CLAUSTROPHOBHIA`, `ACROPHOBIA`, `WATER RISE DURATION(sec)`, `Thank You For Choosing Us :)`. | Cold, clinical, machine-generated, and contradictory. Switches between clinical coldness, shouting headers, and casual emojis. |
| **P2** | **Acrophobia** | **Misspelled Labels:** `Time Elasped:` instead of `Time Elapsed:`. | Appears unprofessional. |

---

## 2. The Gut Check: Intentional vs. Assembled

- **Main Menu (`Menu.unity`):** *Assembled.* Feels like an unstyled developer debug screen with misspellings, raw float input fields, and personal social links slapped onto a world canvas to an upbeat ukulele song.
- **Claustrophobia (`the last revelation.unity`):** *Assembled.* Feels like an eerie, abandoned tech demo; the harsh hard-drive hum and silent, sudden wall movement induce sensory disorientation rather than controlled exposure.
- **Aquaphobia (`Island Example 3.unity`):** *Assembled.* Beautiful environment asset pack, but completely hollow as a therapy experience: water rises arbitrarily without framing, purpose, or breathing anchors.
- **Acrophobia (`Pack01Demo01.unity`):** *Assembled.* The core exposure mechanic (looking down to ascend) is clever and therapeutically valid, but the execution feels like a stressful arcade mini-game with floating debug numbers, flashing red countdowns, and honking horns.

---

## 3. Top 10 High-Impact Changes (Felt-Quality-per-Effort)

1. **Fix Camera Rig Attachment & Decoupling Bug (P0):** Ensure `VRPlayerRig` stays properly anchored to the hoverboard in Acrophobia and unifies movement in Claustrophobia and Aquaphobia with zero duplicate CharacterControllers or camera pops.
2. **Eliminate All Arousal-Spiking Audio (P0):** Remove car/truck horn honks and hard-drive grinding hums; replace with soft, spatialized, looping therapeutic ambience (gentle mountain breeze, tranquil ocean tide, warm room acoustic bed) with zero audible seams.
3. **Remove Punitive & Flashing Visuals (P0):** Eliminate red countdown warnings, flashing red `MAXIMUM ALTITUDE REACHED!` text, and replace with calm, non-judgmental visual grounding (soft progress rings, gentle breathing prompts).
4. **Implement Gentle Start & End Rituals (P1):** Introduce a 3-breath preparation countdown before stimulus movement starts, and a calm, self-paced closure sequence allowing immediate exit or peaceful reflection.
5. **Calibrate Menu Spatial Canvas & Clean Up Layout (P0/P2):** Anchor the World Space Canvas dynamically directly in front of the patient at eye level; replace raw float inputs with intuitive preset buttons (Gentle, Moderate, Custom); fix all typos (`CLAUSTROPHOBIA`, `Time Elapsed`).
6. **Strip Social Media Links & Privacy Leaks (P2):** Remove GitHub, Instagram, and Gmail mailto links asking for patient address/name; keep application strictly local and HIPAA/privacy-conscious.
7. **Instantiate the In-World Emergency Beacon & Accessible Pause Menu (P0/P1):** Place a visible, serene emergency exit anchor in every simulation; upgrade pause menu activation to modern OpenXR primary/menu buttons, secondary buttons, and gaze-dwell rescue.
8. **Expose VR Comfort Settings in UI (P1):** Add clear in-session and main menu toggles for Seated/Standing mode, Snap/Smooth turn, Turn Angle, Vignette Intensity, and Recenter.
9. **Add Audio Subtitles / Captions & Volume Mixer (P1):** Create an on-screen caption system for speech and critical environmental sounds (water rising, wall movement), backed by a balanced audio mixer.
10. **Tone of Voice Polish Across All Copy (P2):** Draft `TONE_OF_VOICE.md` and rewrite every string in calm, supportive, non-clinical, non-shaming language following clear therapeutic guidelines.
