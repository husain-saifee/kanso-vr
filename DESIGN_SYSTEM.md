# Kanso VR — Design System: Soft & Abstract Horizon (Direction C)

**Version:** 1.0.0  
**Status:** Approved Source of Truth  
**Branch:** `polish/experience`  
**Application:** Therapeutic Virtual Reality (OpenXR / Meta Quest & PCVR)  

---

## 1. Core Vision & Psychological Framing

Kanso VR is designed as a calm, composable, and emotionally safe exposure therapy environment. It provides structured, predictable exposure to phobic stimuli without spectacle, pressure, or gamified stress.

### 1.1 Adjectives
- **Must Evoke:** *Spacious*, *Unburdened*, *Atmospheric*, *Tranquil*, *Composed*.
- **Must Never Evoke:** *Confining*, *Arousal-Spiking*, *Punitive*, *Sharp*, *Arcade-like*, *Sterile*.

### 1.2 Aesthetic Reference Points
- **James Turrell (Skyspaces):** Soft, boundaryless light gradients; seamless perceptual depth; light as physical presence that slows respiration.
- **Monument Valley (Ustwo Games):** Minimalist geometry softened by atmospheric haze; pastel twilight palettes that reduce cognitive fatigue.
- **Hiroshi Yoshimura & Brian Eno:** Ambient acoustics using warm harmonic resonance, open space, and silence as active comfort states.

---

## 2. Color Palette & Accessibility Specifications

All color values are verified for contrast against their respective backgrounds in compliance with WCAG 2.2 Level AAA standards (minimum 7:1 for body copy).

```
[ Deep Twilight Backdrop: #181824 ]
   └── [ Elevated Surface Panel: #242338 (92% opacity) ]
          ├── [ Accent Border: #3A3854 (50% opacity) ]
          ├── [ Primary Text: #F4F3F8 ] (Contrast: 14.8:1)
          ├── [ Subtitle Text: #B6B3CE ] (Contrast: 7.2:1)
          ├── [ Dusk Lavender Accent: #9B98E6 ] (Interactive / Focus)
          ├── [ Dawn Amber Accent: #E8B89E ] (Soft Highlight)
          ├── [ Serene Guidance: #7E9F94 ] (Confirmation / Reassurance)
          └── [ Gentle Notice: #D48D8D ] (Exit / Caution; Never Red)
```

### 2.1 Color Tokens
| Token Name | Hex Code | RGBA Float | Purpose & Usage |
| :--- | :--- | :--- | :--- |
| `CanvasBackground` | `#181824` | `(0.094, 0.094, 0.141, 1.00)` | Deep twilight room / sky baseline. |
| `SurfacePanel` | `#242338` | `(0.141, 0.137, 0.220, 0.92)` | World-space card and dialog container. |
| `SurfaceBorder` | `#3A3854` | `(0.227, 0.220, 0.329, 0.50)` | Soft boundary separator (1.5px width). |
| `TextPrimary` | `#F4F3F8` | `(0.957, 0.953, 0.973, 1.00)` | Headers, titles, and active button text. |
| `TextSecondary` | `#B6B3CE` | `(0.714, 0.702, 0.808, 1.00)` | Descriptive instructions, metadata, timestamps. |
| `AccentLavender` | `#9B98E6` | `(0.608, 0.596, 0.902, 1.00)` | Selected state, focus indicators, progress fill. |
| `AccentDawnAmber`| `#E8B89E` | `(0.910, 0.722, 0.620, 1.00)` | Warm focal anchors, breathing ring cue. |
| `StateSerene` | `#7E9F94` | `(0.494, 0.624, 0.580, 1.00)` | Affirmative actions ("Begin", "Resume"). |
| `StateGentleExit`| `#D48D8D` | `(0.831, 0.553, 0.553, 1.00)` | "Return to Menu", "Pause", Emergency exit. |

### 2.2 Color Accessibility Rules
1. **Never Color Alone:** Any state change (e.g., focus, selection, pause) must combine color with shape change, icon shift, or clear text labels.
2. **Strict Ban on Punitive Red:** Never use `#FF0000` or `#B81D13`. For exits or safety notices, use muted `StateGentleExit` (`#D48D8D`).
3. **No Flashing Colors:** No alternating color sequences or pulsing red text anywhere in the project.

---

## 3. Typography & Spatial Layout System

### 3.1 Type Scale (TextMeshPro)
- **Font:** SDF Geometric Sans (Liberation Sans SDF / Inter).
- **Scale Hierarchy:**
  - `Display / Title`: 28–32 pt (Semi-Bold, tracking +0.02)
  - `Header`: 22–24 pt (Medium, tracking +0.01)
  - `Body`: 16–18 pt (Regular, line spacing 1.35)
  - `Caption / Subtext`: 13–14 pt (Regular, tracking +0.03)
- **Formatting Rule:** Sentence case or Title Case for interactive buttons. **All-caps shouting is strictly prohibited.**

### 3.2 World-Space Canvas Calibration
- **Spatial Position:** Floating comfortably directly in the user's field of view:
  ```csharp
  Vector3 forward = vrCamera.forward; forward.y = 0;
  Vector3 targetPos = vrCamera.position + forward.normalized * 1.8f;
  targetPos.y = vrCamera.position.y; // Centered at eye level
  ```
- **Scale:** Uniform `Vector3.one * 0.0022f` for high-density legibility in VR lenses.
- **Panel Geometry:** Rounded corners (`16px` radius equivalent), soft dark elevation shadow.
- **Interaction Spacing:** Minimum button height `48px` (scaled) with at least `16px` margin between interactive elements to prevent accidental misclicks.

---

## 4. Motion Language & Animation Presets

Motion in DePhobia is designed to slow autonomic heart rate and stabilize vestibular balance. Sudden translations, snappiness, or elastic bounces are forbidden.

| Preset Name | Duration | Curve / Formula | Application |
| :--- | :--- | :--- | :--- |
| `breathe` | `4.5s` | Cosine Ease-In-Out (`Mathf.SmoothStep`) | Resting focus indicators, breathing visualizer ring, ambient light undulating. |
| `settle` | `0.55s` | SmoothDamp (`smoothTime = 0.18s`) | Dialog appearance, altitude ascent smoothing, camera height adjustments. |
| `acknowledge` | `0.12s` | Scale dip to `0.96x` with linear recovery | Ray-pointer click or controller trigger confirmation. |
| `fadeScreen` | `0.60s` | Linear unscaled alpha transition | VRFadeManager transitions between scenes and resets. |

### Motion Guardrails
- No screen shake under any condition.
- No involuntary camera rotations or sudden pitch shifts.
- FOV comfort vignette automatically deepens during any forward locomotion or altitude descent.

---

## 5. Audio Architecture & Acoustic Palette

### 5.1 Volume Hierarchy & Target Loudness
All audio passes through an explicit Unity Audio Mixer with calibrated bus levels:
- **Master Bus:** Target `-14 LUFS` maximum ceiling.
- **Ambience Bus (`-18 LUFS`):** Warm harmonic drone pads, subtle mountain wind, tranquil looping ocean tide. Loop points must be zero-crossing and seam-free.
- **Feedback Bus (`-24 LUFS`):** Soft acoustic wood/chime clicks (<40ms duration) for UI interactions.
- **Voice / Guidance Bus (`-16 LUFS`):** Clear, warm, centered guidance with high spatial intelligibility.

### 5.2 Audio Discipline & Rules
1. **Rule for Silence:** Silence is a designed comfort state. During contemplation intervals or when stimuli are stationary, background audio drops to near-silence.
2. **Zero Arousal Spikes:** Car horns, sirens, alarms, mechanical buzzes, screeches, and loud pops are **completely banned**.
3. **Audio Captions:** All critical environmental sounds (e.g. `[Water rising gently]`, `[Walls stabilizing]`) must trigger a synchronized visual subtitle box.

---

## 6. Interaction States

Every interactive element in DePhobia must exhibit six unmistakable, consistent states:

1. **Idle:** Surface panel tone (`#242338`), muted border (`#3A3854`), secondary text (`#B6B3CE`).
2. **Hover / Focused:** Border illuminates to Lavender (`#9B98E6`), text brightens to primary white (`#F4F3F8`), subtle `1.02x` scale settle.
3. **Pressed:** Gentle `0.96x` scale dip, `acknowledge` audio tone, soft haptic click on VR controller.
4. **Active / Selected:** Background shifts to soft Dusk Lavender tint with serene check indicator.
5. **Disabled / Unavailable:** Opacity lowered to `0.35`, raycast disabled, non-clickable.
6. **Emergency Beacon:** Soft luminous amber/lavender beacon in-world, constantly visible, triggered by gaze dwelling (2.0s) or direct pointer click.

---

## 7. Compliance Verification Checklist
Before shipping any scene or UI component, verify:
- [ ] Contrast ratio is >= 7:1 against background.
- [ ] Zero strings in ALL-CAPS or using exclamation points.
- [ ] No red `#FF0000` or warning yellow `#FFFF00` colors used.
- [ ] Canvas is positioned relative to `Camera.main` at 1.8m, never hardcoded to absolute origin.
- [ ] Audio clip has no audible click/pop and routes through the Audio Mixer.
- [ ] Sound has an accompanying visual subtitle event.
