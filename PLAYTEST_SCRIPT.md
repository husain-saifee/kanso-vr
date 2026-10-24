# DePhobia — 20-Minute Human Playtest Script (`PLAYTEST_SCRIPT.md`)

**Target Platforms:** Meta Quest 2 / 3 / Pro (OpenXR Standalone) & PCVR (SteamVR / Link)  
**Tester Profile:** Experience Designer, Clinical Therapist, or QA Tester  
**Duration:** ~20 minutes  
**Pass Criteria:** Zero crashes, zero red flashing alerts, zero startle audio, pause/exit reachable in <2 seconds from any point.

---

## Pre-Test Checklist
- [ ] Headset battery > 50%; tracking boundary configured.
- [ ] Audio volume set to 50% on headset.
- [ ] Tester begins in a comfortable seated position.

---

## Step 1: First-Run & Main Menu (Minutes 0–3)
| Step | Action | Expected Behavior | Failure Looks Like |
| :--- | :--- | :--- | :--- |
| **1.1** | Launch application from headset library. | Screen fades in smoothly from black (`0.6s`). Main Menu canvas appears directly at eye level (1.8m in front). | Screen pops white/black; canvas is behind the user or 700m away in the void. |
| **1.2** | Inspect UI typography and labels. | Clean titles: *"Claustrophobia"*, *"Aquaphobia"*, *"Acrophobia"*, *"Session History"*. Zero misspellings. No Instagram/GitHub/Gmail buttons. | Any text reading `CLAUSTROPHOBHIA`, all-caps shouting, or personal social media links. |
| **1.3** | Listen to background audio. | Calm, subtle harmonic ambient bed (432 Hz). No upbeat corporate ukulele. | Loud, cheerful ukulele music playing over clinical menus. |
| **1.4** | Open Comfort Settings; toggle Seated Mode. | Camera height gently adjusts to comfortable seated eye level without clipping. | View remains stuck at standing height or snaps erratically. |

---

## Step 2: Claustrophobia Simulation (Minutes 3–7)
| Step | Action | Expected Behavior | Failure Looks Like |
| :--- | :--- | :--- | :--- |
| **2.1** | Select "Claustrophobia". | Smooth fade to black; loads room scene. Ambient Start Ritual card appears: *"Welcome to Enclosed Space"*. Animated breathing ring pulses. Walls do not move yet. | Walls begin closing immediately before user is ready; hard drive buzzing sound plays. |
| **2.2** | Follow breathing ring for 2 cycles; click "Begin Session". | Card fades out. Caption banner appears: `[Walls will begin moving gently]`. Walls start contracting at configured pace. | No caption; sudden jump scare or abrupt wall snap. |
| **2.3** | Test In-World Emergency Beacon. | Look 30° left: soft luminous beacon is visible: *"Return to Menu"*. Gaze dwell for 2.0s or click. | Beacon is missing; user feels trapped with no visible exit. |
| **2.4** | Test Pause Menu. | Press controller Menu button (or Y/B button / Escape). Simulation freezes; pause menu appears right in front. Select "Resume". | Simulation fails to pause; walls keep closing during pause; button is bright red. |
| **2.5** | Allow walls to reach safe boundary. | Walls stop at safe threshold (`2.85m`). End Ritual card appears gently: *"Session Completed Gently"*. Select "Return to Menu". | Walls collapse through user; forced exit without closure. |

---

## Step 3: Aquaphobia Simulation (Minutes 7–11)
| Step | Action | Expected Behavior | Failure Looks Like |
| :--- | :--- | :--- | :--- |
| **3.1** | Select "Aquaphobia". | Smooth fade into scenic tropical island. Start Ritual card appears. Water does not rise until "Begin Session" is selected. | Water rises immediately without breathing pause. |
| **3.2** | Select "Begin Session". Walk forward using thumbstick. | FOV comfort vignette smoothly darkens peripheral vision during movement. Ocean waves loop with zero clicks/pops. | Nausea/motion sickness from un-vignetted motion; audible loop seam clicks. |
| **3.3** | Test Recenter Shortcut. | Move to a new position; click Right Thumbstick (or R key). View and height instantly recalibrate to current orientation. | Canvas or orientation drifts away and cannot be reset. |
| **3.4** | Test Safe Exit. | Press Menu button; select "Save & Return to Menu". Returns gracefully to Main Menu. | Abrupt scene cut; crash to desktop. |

---

## Step 4: Acrophobia Simulation (Minutes 11–17)
| Step | Action | Expected Behavior | Failure Looks Like |
| :--- | :--- | :--- | :--- |
| **4.1** | Select "Acrophobia". | Fades into city view. User is standing on the hoverboard. Start Ritual card appears. Hoverboard stays stationary. | User spawns on ground 200m away from hoverboard; car horns blare immediately. |
| **4.2** | Select "Begin Session". Look downward into city abyss (pitch 30°–75°). | Hoverboard ascends smoothly into the sky. **User and camera ascend with the hoverboard.** Soft wind ambience plays. Zero car or truck horns. | Hoverboard flies up into the sky leaving the user's camera behind; loud car horns blare. |
| **4.3** | Look straight forward (away from drop). | Hoverboard holds position, then begins calm descent. HUD shows gentle lavender/neutral text. **Zero red text.** | HUD numbers turn bright alarm red; warning buzzers play. |
| **4.4** | Look down again until reaching 360m summit. | Hoverboard gently stabilizes at summit. Calm End Ritual card appears: *"Summit reached (360m). Take a moment to breathe."* Zero flashing red text; zero 60-second forced countdown locks. | Text flashes bright red/white `MAXIMUM ALTITUDE REACHED!`; 60-second countdown traps the user. |
| **4.5** | Select "Save & Return to Menu". | Smooth fade-to-black back to Main Menu. | Abrupt cut or frozen state. |

---

## Step 5: Session History Verification (Minutes 17–20)
| Step | Action | Expected Behavior | Failure Looks Like |
| :--- | :--- | :--- | :--- |
| **5.1** | Click "Session History" in Main Menu. | Table lists the 3 completed sessions with friendly dates, formatted durations, and peak metrics. | Table is blank; crashes with null reference; unformatted raw floats. |
| **5.2** | Inspect local storage path (`PersonStats`). | `session_history.json` contains valid, structured JSON without any network leaks or telemetry. | File missing or malformed; external network requests detected. |

---

## Sign-off
- **Safety Pass:** [PASS / FAIL]
- **Accessibility Pass:** [PASS / FAIL]
- **Comfort & Motion Pass:** [PASS / FAIL]
- **Aesthetic Direction C Fidelity:** [PASS / FAIL]
- **Notes / Observations:** _______________________________
