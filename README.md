# DePhobia — VR Exposure Therapy Platform

[![Unity](https://img.shields.io/badge/Unity-2022.3%20LTS-blue.svg)](https://unity.com/)
[![OpenXR](https://img.shields.io/badge/Standard-OpenXR-green.svg)](https://www.khronos.org/openxr/)
[![Platforms](https://img.shields.io/badge/Platforms-Meta%20Quest%20|%20PCVR%20|%20Pico-orange.svg)](#supported-platforms)

**DePhobia** is an immersive virtual reality exposure therapy application designed to assist individuals in confronting and managing common phobic disorders through structured, progressive, and safe interactive simulations.

Developed with **Unity 2022.3 LTS** and built on the cross-platform **OpenXR** industry standard with the **XR Interaction Toolkit (XRI)**.

---

## 🧠 Exposure Therapy Simulations

### 1. 🚪 Claustrophobia (Fear of Enclosed Spaces)
- **Environment:** An enclosed room without external windows or doors.
- **Therapeutic Progression:** The walls progressively and steadily contract inward towards the patient based on clinical parameters (speed, contraction interval, and movement duration).
- **Therapeutic Goal:** Patients practice controlled breathing and distress tolerance in a confined space.
- **Safety Boundary:** Walls automatically stop at a safe physical boundary, preventing room collapse or physical clipping.

### 2. 🌊 Aquaphobia (Fear of Water)
- **Environment:** A tranquil tropical island surrounded by ocean tides.
- **Therapeutic Progression:** Water levels steadily rise over time according to custom timing parameters.
- **Therapeutic Goal:** Controlled gradual exposure to encroaching water in a scenic, non-threatening environment.
- **Safety Boundary:** Maximum water elevation is clamped to a safe threshold.

### 3. 🏙️ Acrophobia (Fear of Heights)
- **Environment:** An open hoverboard suspended hundreds of meters above a bustling metropolitan city.
- **Therapeutic Mechanic:** 
  - **Confronting Fear:** When the patient looks downward into the abyss (gaze pitch 30°–85°), the hoverboard rewards exposure by ascending smoothly into the sky.
  - **Avoidance / Fear Response:** If the patient looks away or glances upward for longer than the configured wait duration, the hoverboard pauses ascension and begins descending until the patient refocuses.
  - **Peak Altitude:** Upon reaching 360m, a session conclusion sequence activates with celebratory visual feedback.
- **Safety Boundary:** The hoverboard is clamped above ground level (`y >= 27.04m`), completely eliminating accidental free-fall.

---

## 🎮 Controls & Interaction

### VR Headsets (Meta Quest 2/3, Pico, SteamVR / PCVR)
| Action | Controller Input |
| :--- | :--- |
| **Locomotion** | **Left Thumbstick** (Smooth movement) |
| **Turning** | **Right Thumbstick** (Snap turn by default; 45° increments) |
| **UI Interaction** | **Right Trigger / Index Grip** (Ray Pointer) |
| **Pause & Safety Menu** | **Left Menu Button / Y Button** (Toggles 3D Floating Pause Menu) |
| **Emergency Rescue** | Look at or point to the **In-World Emergency Exit Beacon** |
| **Acrophobia Gaze** | Tilt head downward to ascend; look forward/away to descend |

### Desktop / Editor Testing Fallback (No Headset Required)
| Action | Keyboard / Mouse Input |
| :--- | :--- |
| **Movement** | `W`, `A`, `S`, `D` or Arrow Keys |
| **Turning** | `Q` (Turn Left) / `E` (Turn Right) or Mouse X |
| **Pause / Exit** | `Escape` or `P` key |
| **Interact / Click** | Left Mouse Click |

---

## 🛡️ Patient Safety & VR Comfort Features

- **FOV Tunneling Vignette:** Dynamic peripheral darkening during rapid locomotion or altitude changes eliminates vestibular motion sickness.
- **Snap Turn & Smooth Turn:** Customizable turning mode with configurable turn angle (30° / 45°).
- **In-Game Pause System:** Patients can pause the simulation at any point to view elapsed time and resume or exit safely.
- **Physical Emergency Exit Beacon:** An in-world beacon is positioned in every simulation, enabling immediate session termination without complex menu navigation.
- **Gentle Screen Fades:** All scene transitions utilize smooth VR fade-to-black sequences to prevent visual jarring and headset lag disorientation.

---

## 📊 Patient Data & Session Persistence

Session outcomes and therapy parameters are automatically tracked and saved:
- **Storage:** Persisted locally as structured JSON data (`session_history.json`) in `Application.persistentDataPath/PersonStats/`.
- **Metrics Tracked:** Session timestamp, duration, peak metrics (altitude reached, wall contraction cycles), completion status, and parameters used.
- **History Viewer:** Past therapy sessions are displayed in a clean table within the Main Menu.
- **Backwards Compatibility:** Legacy formatted text files (`ClausStats.txt`, `AquaStats.txt`, `AcroStats.txt`) are generated concurrently.

---

## 🚀 Setup & Build Instructions

### Prerequisites
- **Unity 2022.3 LTS** (e.g. `2022.3.40f1` or newer)
- **Modules Required:**
  - Android Build Support (OpenXR Standalone for Meta Quest / Pico)
  - Windows Build Support (for PCVR / SteamVR)

### Project Configuration
1. Open the project in Unity 2022.3 LTS via Unity Hub.
2. In **Project Settings > XR Plug-in Management**:
   - Enable **OpenXR** under Android and Standalone tabs.
   - In **OpenXR Feature Groups**, enable **Meta Quest Support** (Android) and **Oculus / SteamVR** (PC).
3. In **Project Settings > Player**:
   - Color Space: **Linear** (verified in `ProjectSettings.asset`).
   - Android Target Architecture: **ARM64** (v8a).
   - Minimum API Level: **Android 8.0 (API level 26)**.

### Build Scenes Order
All 4 scenes are pre-configured in `EditorBuildSettings.asset`:
- `0`: `Assets/Menu/Menu.unity`
- `1`: `Assets/Room/scenes/the last revelation.unity` (Claustrophobia)
- `2`: `Assets/Snowconesolid Assets/Super Beach Pack/Scenes/Island Example 3.unity` (Aquaphobia)
- `3`: `Assets/Geopipe/Pack01Demo/Pack01Demo01.unity` (Acrophobia)

---

## 📄 Documentation
- [AUDIT_REPORT.md](AUDIT_REPORT.md) — Comprehensive technical audit of legacy systems.
- [PLAN.md](PLAN.md) — Architectural modernization and repair plan.
- [CHANGELOG_AI.md](CHANGELOG_AI.md) — Detailed ledger of all code changes.
- [THIRD_PARTY_ASSETS.md](THIRD_PARTY_ASSETS.md) — Asset licenses and credits.

---

## ⚖️ License
Educational and clinical research platform. Third-party assets are subject to their respective licenses detailed in [THIRD_PARTY_ASSETS.md](THIRD_PARTY_ASSETS.md).
