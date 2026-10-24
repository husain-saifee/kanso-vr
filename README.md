# DePhobia — VR Exposure Therapy Platform

[![Unity](https://img.shields.io/badge/Unity-2022.3%20LTS-blue.svg)](https://unity.com/)
[![OpenXR](https://img.shields.io/badge/Standard-OpenXR-green.svg)](https://www.khronos.org/openxr/)
[![Platforms](https://img.shields.io/badge/Platforms-Meta%20Quest%20|%20PCVR%20|%20Pico-orange.svg)](#supported-platforms)
[![Accessibility](https://img.shields.io/badge/WCAG-2.2%20AAA-success.svg)](ACCESSIBILITY.md)
[![Privacy](https://img.shields.io/badge/Telemetry-Zero%20Data%20Collection-blue.svg)](#privacy--data-protection)

**DePhobia** is an immersive virtual reality exposure therapy application designed to assist individuals in confronting and managing common phobic conditions through structured, progressive, and emotionally safe interactive simulations.

Engineered with **Unity 2022.3 LTS** on the cross-platform **OpenXR** industry standard with the **XR Interaction Toolkit (XRI)**.

---

## 🎨 Design Philosophy: Soft & Abstract Horizon (Direction C)

DePhobia prioritizes composure, emotional safety, predictability, and dignity:
- **Atmospheric Palette:** Deep twilight backdrops (`#181824`), elevated surfaces (`#242338`), dusk lavender focus (`#9B98E6`), and dawn amber accents (`#E8B89E`). High contrast ratio of **14.8:1** (exceeds WCAG AAA).
- **Acoustic Peace:** Harmonic ambient bed (432 Hz), zero startle audio, zero car horns, and zero mechanical buzzing.
- **Supportive Language:** No failure states, no punitive red warnings, no forced countdowns, and no clinical jargon.

---

## 🧠 Exposure Therapy Simulations

### 1. 🚪 Claustrophobia (Enclosed Space)
- **Environment:** An enclosed room without external windows.
- **Therapeutic Progression:** The walls progressively contract inward towards the patient based on clinical parameters (speed, interval, and movement duration).
- **Safety Boundary:** Walls automatically stop at a safe physical threshold (`2.85m`), completely eliminating room collapse or clipping.
- **Containment Ritual:** Begins with a guided 3-breath visualizer; concludes with a calm de-escalation summary.

### 2. 🌊 Aquaphobia (Rising Water)
- **Environment:** A tranquil coastal island surrounded by gentle tides.
- **Therapeutic Progression:** Water levels rise gradually over time according to custom timing parameters.
- **Safety Boundary:** Maximum water elevation is clamped to a safe height threshold (`1.72m`).
- **Comfort:** Ambient tide audio with zero loop seams; dynamic FOV comfort vignette during locomotion.

### 3. 🏙️ Acrophobia (High Elevation)
- **Environment:** An open hoverboard ascending above a metropolitan skyline.
- **Therapeutic Mechanic:**
  - **Confronting Fear:** Looking downward into the abyss (gaze pitch 30°–75°) smoothly ascends the hoverboard into the sky.
  - **Avoidance / Rest:** Looking forward or away pauses ascension and begins a gentle descent until the patient refocuses.
  - **Peak Altitude:** Upon reaching 360m summit, a serene closure sequence activates without flashing lights or forced countdowns.
- **Safety Boundary:** The hoverboard is clamped at ground level (`y >= 27.04m`), completely eliminating accidental free-fall.

---

## 🎮 Controls & Interaction

### VR Headsets (Meta Quest 2 / 3 / Pro, Pico, SteamVR / PCVR)
| Action | Controller Input |
| :--- | :--- |
| **Locomotion** | **Left Thumbstick** (Smooth movement; disabled on hoverboard for safety) |
| **Turning** | **Right Thumbstick** (Snap turn default; 45° increments) |
| **UI Interaction** | **Right Trigger / Index Grip** (Ray Pointer with gentle tactile click) |
| **Pause & Safety Menu** | **Left Menu Button or Y/B Button** (Toggles 3D Floating Pause Menu) |
| **Recenter View** | **Right Thumbstick Click or Pause Menu** (Recenters yaw and seated height) |
| **Emergency Rescue** | Look at or point to the **In-World Emergency Exit Beacon** (2.0s gaze or click) |
| **Acrophobia Gaze** | Tilt head downward to ascend; look forward to pause |

### Desktop / Editor Testing Fallback
| Action | Keyboard / Mouse Input |
| :--- | :--- |
| **Movement** | `W`, `A`, `S`, `D` or Arrow Keys |
| **Turning** | `Q` (Turn Left) / `E` (Turn Right) |
| **Recenter** | `R` key |
| **Pause / Exit** | `Escape` or `P` key |
| **Interact / Click** | Left Mouse Click |

---

## 🛡️ Patient Safety, Comfort & Accessibility

- **Universal Subtitles & Captions:** High-contrast captions (`TherapyCaptionUI.cs`) display visual equivalents for speech and environmental events.
- **Seated & Standing Modes:** Seated mode offsets camera height (`+0.35m`) so seated/wheelchair users experience full immersion without neck strain.
- **FOV Tunneling Vignette:** Peripheral darkening during movement eliminates vestibular motion sickness.
- **In-Game Pause System:** Patients can pause at any point to view elapsed time, recenter, toggle seated mode, or exit safely.
- **In-World Emergency Exit Beacon:** An in-world beacon is instantiated in every simulation, enabling immediate session termination without menu navigation.
- **Gentle Screen Fades:** All scene transitions utilize smooth VR fade-to-black sequences (`VRFadeManager.cs`).

---

## 🔒 Privacy & Data Protection

- **Zero Telemetry:** No analytics SDKs, no external network calls, and no data tracking.
- **Local Persistence Only:** Patient session outcomes are saved strictly to local JSON (`session_history.json`) on the headset in `Application.persistentDataPath/PersonStats/`.
- **Zero Third-Party Phoning:** All social media links and external email templates have been completely removed.

---

## 📄 Documentation

- [EXPERIENCE_AUDIT.md](EXPERIENCE_AUDIT.md) — Comprehensive Phase 1 whole-experience design & technical audit.
- [DESIGN_SYSTEM.md](DESIGN_SYSTEM.md) — Source of truth design tokens, typography scale, motion language, and audio hierarchy.
- [TONE_OF_VOICE.md](TONE_OF_VOICE.md) — Copy rules and terminology mapping for non-shaming therapeutic interaction.
- [ACCESSIBILITY.md](ACCESSIBILITY.md) — WCAG 2.2 AAA compliance matrix and comfort specifications.
- [PLAYTEST_SCRIPT.md](PLAYTEST_SCRIPT.md) — 20-minute human manual test script with expected outcomes.
- [THIRD_PARTY_ASSETS.md](THIRD_PARTY_ASSETS.md) — Asset licenses and credits.
- [CHANGELOG_AI.md](CHANGELOG_AI.md) — Ledger of all engineering and polish modifications.
