# Kanso VR — Design System: Forest Mineral & Sage

**Version:** 2.0.0  
**Status:** Approved Source of Truth  
**Branch:** `redesign/forest-mineral-sage`  
**Application:** Therapeutic Virtual Reality (OpenXR / Meta Quest, PCVR & Desktop)  

---

## 1. Core Vision & Atmosphere

Kanso VR redesigns exposure therapy around composure, autonomy, and quiet dignity.
Each screen has a clear point of view: **the thing the user came to do is the loudest thing on it, and everything else is quiet.** Restraint, not emptiness. Confidence, not cleverness.

### 1.1 Aesthetic Foundations
- **Zero Blue / Violet Tones:** Completely purged of generic tech blues, cold violets, and purple gradients.
- **Organic Earth & Mineral:** Deep Earth Charcoal, Dark Spruce Moss, Lichen borders, Warm Off-White, Ash Olive, and calm Matcha Sage.
- **Physical Quietude:** Zero floating HUD numbers during active exposure. Somatic haptic feedback and spatial audio replace intrusive text popups.
- **Human Autonomy:** The patient is in complete control at every micro-moment. Instant pause, subtle horizon escape ring, and no punitive failure states.

---

## 2. Color Palette & Accessibility Specifications (WCAG 2.2 AAA)

All tokens deliver strict contrast compliance against their surfaces:

```
[ Earth Charcoal Canvas: #141614 ]
   └── [ Elevated Surface Panel: #1E221E (96% opacity) ]
          ├── [ Lichen Border: #343A34 (65% opacity) ]
          ├── [ Primary Typography: #F3F5F2 ] (Contrast: 14.2:1)
          ├── [ Secondary Typography: #9AA299 ] (Contrast: 7.5:1)
          ├── [ Matcha Sage Accent (Primary Action): #76936A ]
          ├── [ Gentle Exit Clay (Safe Return): #AD6E60 ]
          └── [ Inactive Surface / Preset Fill: #282D28 ]
```

### 2.1 Color Tokens (`ThemeTokens.cs`)
| Token Name | Hex Code | RGBA Float | Purpose & Usage |
| :--- | :--- | :--- | :--- |
| `CanvasBg` | `#141614` | `(0.078, 0.086, 0.078, 1.00)` | Deep earth charcoal room / sky baseline. |
| `SurfacePanel` | `#1E221E` | `(0.118, 0.133, 0.118, 0.96)` | World-space card and dialog container. |
| `SurfaceBorder` | `#343A34` | `(0.204, 0.227, 0.204, 0.65)` | Soft structural boundary outline (1.5px). |
| `SurfaceInactive`| `#282D28` | `(0.157, 0.176, 0.157, 0.90)` | Unselected tab and inactive preset fill. |
| `TextPrimary` | `#F3F5F2` | `(0.953, 0.961, 0.949, 1.00)` | Headers, titles, and active button text. |
| `TextSecondary` | `#9AA299` | `(0.604, 0.635, 0.600, 1.00)` | Descriptive instructions and metadata. |
| `TextMuted` | `#6C746B` | `(0.424, 0.455, 0.420, 1.00)` | Subtle footers, timestamps, and keys. |
| `AccentSage` | `#76936A` | `(0.463, 0.576, 0.416, 1.00)` | Primary "Begin Session" CTA and active tab. |
| `GentleExitClay` | `#AD6E60` | `(0.678, 0.431, 0.376, 1.00)` | Safe "Return to Menu" action (never red). |

---

## 3. Typography & Hierarchy System

- **Typeface:** Plus Jakarta Sans / Geometric Sans SDF.
- **Scale Hierarchy:**
  - `Display / Portal Title`: 28–32 pt (Semi-Bold)
  - `Header / Space Name`: 22–24 pt (Medium)
  - `Body / Description`: 15–16 pt (Regular, line spacing 1.35)
  - `Action Label`: 16–18 pt (Semi-Bold)
  - `Secondary Link`: 13–14 pt (Regular)
- **Formatting Rule:** Sentence case or Title Case for interactive elements. **All-caps shouting is strictly banned.**

---

## 4. Screen-by-Screen Point of View

### 4.1 Main Portal (Sanctuary)
- **Point of View:** The user came to select an exposure environment and begin. That action leads with undeniable clarity.
- **Layout:** Single central panel in 3D world space (`1.8m` from eye).
- **Top:** 3 clean tabs (`Enclosed Space`, `Rising Water`, `High Elevation`).
- **Middle:** Calibrated description + 3 difficulty presets (`Gentle`, `Standard`, `Deep`).
- **Bottom:** Prominent matcha sage `[ Begin Session ]` button.
- **Perimeter:** Quiet footer anchors for `[ Session History ]`, `[ Comfort & Audio ]`, and `[ Exit ]` that swap the card content in-place without nested popups.

### 4.2 Active Exposure (All 3 Spaces)
- **Point of View:** Somatic presence and nervous system regulation.
- **Hierarchy:** Zero floating HUD numbers in line of sight.
- **Escape Affordance:** Subtle horizon breathing ring at the bottom of the field of view. Looking down for 1.5s or tapping/clicking exits safely.
- **Feedback:** Gentle controller haptic pulses and spatial acoustic tones convey status; no distracting text toasts.

### 4.3 In-Game Pause Slate
- **Point of View:** A quiet breath and rapid choice.
- **Layout:** Minimalist two-button slate at eye level:
  - `[ Resume Session ]` (Matcha Sage `#76936A`)
  - `[ Return to Menu ]` (Umber Clay `#AD6E60`)
- **Comfort:** Recenter view and seated mode mapped to thumbstick click and `R` key, leaving the screen uncluttered.

### 4.4 Milestone Closure
- **Point of View:** Dignified reflection upon summit or safe boundary.
- **Flow:** Motion gently halts -> calm fade to clean summary card showing duration and peak metric -> single button to return to Sanctuary.
