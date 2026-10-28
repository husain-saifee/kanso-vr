using UnityEngine;
using DePhobia.Core;
using DePhobia.VR;

/// <summary>
/// Scene lifecycle controller for Claustrophobia (Enclosed Space) simulation.
/// Initializes the patient session, manages the VR Player Rig, spawns the horizon respiration ring,
/// and conducts the calming start/end rituals under the Forest Mineral & Sage visual language.
/// </summary>
public class StartVr : MonoBehaviour
{
    private xposmove[] _posWalls;
    private xnegmove[] _negWalls;
    private bool _wallsActive = false;
    private bool _completionShown = false;

    private void Start()
    {
        // Enforce that walls never move before the start ritual finishes
        xposmove.sessionStarted = false;
        xnegmove.sessionStarted = false;

        PatientSessionManager.Instance.StartSession("Claustrophobia");

        _posWalls = FindObjectsOfType<xposmove>();
        _negWalls = FindObjectsOfType<xnegmove>();

        var sm = TherapySettingsManager.Instance;
        float calibratedSpeed = (sm != null) ? sm.clausSpeed : 0.05f;
        float moveAfter = (sm != null) ? sm.clausMoveAfter : 3.0f;
        float moveDuration = (sm != null) ? sm.clausMoveDuration : 2.0f;

        if (_posWalls != null)
        {
            foreach (var wall in _posWalls)
            {
                wall.speed = calibratedSpeed;
                wall.timerate = moveAfter;
                wall.vtime = moveDuration;
                wall.enabled = false;
                var rb = wall.GetComponent<Rigidbody>();
                if (rb != null && !rb.isKinematic) rb.velocity = Vector3.zero;
            }
        }

        if (_negWalls != null)
        {
            foreach (var wall in _negWalls)
            {
                wall.speed = calibratedSpeed;
                wall.timerate = moveAfter;
                wall.vtime = moveDuration;
                wall.enabled = false;
                var rb = wall.GetComponent<Rigidbody>();
                if (rb != null && !rb.isKinematic) rb.velocity = Vector3.zero;
            }
        }

        // Clean up any harsh mechanical hard drive audio sources
        var audioSources = FindObjectsOfType<AudioSource>();
        foreach (var src in audioSources)
        {
            if (src.clip != null && src.clip.name.ToLowerInvariant().Contains("hard drive"))
            {
                src.Stop();
                src.volume = 0f;
            }
        }

        // Disable legacy VRLookWalk if present to prevent duplicate CharacterControllers
        var lookWalk = FindObjectOfType<VRLookWalk>();
        if (lookWalk != null)
        {
            lookWalk.enabled = false;
        }

        // Ensure VR Rig exists at the player's origin
        var rig = FindObjectOfType<VRPlayerRig>();
        if (rig == null)
        {
            var rigGo = new GameObject("VRPlayerRig");
            rigGo.transform.position = transform.position;
            rigGo.transform.rotation = transform.rotation;

            rig = rigGo.AddComponent<VRPlayerRig>();
            var cam = Camera.main;
            if (cam != null)
            {
                cam.transform.SetParent(rigGo.transform, false);
                rig.headCamera = cam.transform;
            }
        }

        // Ensure horizon respiration ring and emergency beacon exist
        VRHorizonRespirationRing.EnsureRingInScene();
        VREmergencyExitBeacon.EnsureBeaconInScene(null, new Vector3(-1.1f, 1.4f, 1.8f));

        // Fade in from black smoothly
        VRFadeManager.Instance.FadeIn(0.6f, () =>
        {
            // Conduct calm Start Ritual
            TherapySessionRitual.ShowStartRitual("Enclosed Space", () =>
            {
                _wallsActive = true;
                xposmove.sessionStarted = true;
                xnegmove.sessionStarted = true;

                if (_posWalls != null)
                {
                    foreach (var wall in _posWalls)
                    {
                        wall.enabled = true;
                    }
                }

                if (_negWalls != null)
                {
                    foreach (var wall in _negWalls)
                    {
                        wall.enabled = true;
                    }
                }

                var r = FindObjectOfType<VRPlayerRig>();
                if (r != null) r.SendHapticImpulse(0.2f, 0.08f, UnityEngine.XR.XRNode.RightHand);
            });
        });
    }

    private void Update()
    {
        if (_wallsActive && !_completionShown)
        {
            // Check if any wall has reached the safe boundary (-2.85m and +2.85m)
            bool boundaryReached = false;

            if (_posWalls != null && _posWalls.Length > 0)
            {
                foreach (var wall in _posWalls)
                {
                    if (wall != null && wall.transform.position.x >= -2.85f)
                    {
                        boundaryReached = true;
                        break;
                    }
                }
            }

            if (_negWalls != null && _negWalls.Length > 0)
            {
                foreach (var wall in _negWalls)
                {
                    if (wall != null && wall.transform.position.x <= 2.85f)
                    {
                        boundaryReached = true;
                        break;
                    }
                }
            }

            if (boundaryReached)
            {
                _completionShown = true;

                var rig = FindObjectOfType<VRPlayerRig>();
                if (rig != null) rig.SendHapticImpulse(0.3f, 0.12f, UnityEngine.XR.XRNode.RightHand);

                float cycleDuration = TherapySettingsManager.Instance.clausMoveDuration + TherapySettingsManager.Instance.clausMoveAfter;
                int count = (cycleDuration > 0.1f) ? (int)(xnegmove.tmeas / cycleDuration) : 0;
                string summary = $"Safe Boundary Reached • Movement Cycles: {count}";

                TherapySessionRitual.ShowEndRitual("Enclosed Space", summary, () =>
                {
                    SaveAndReturn(completed: true);
                });
            }
        }

        // Update peak metric (wall movement cycle count)
        float duration = TherapySettingsManager.Instance.clausMoveDuration + TherapySettingsManager.Instance.clausMoveAfter;
        if (duration > 0.1f)
        {
            float wallCycles = xnegmove.tmeas / duration;
            PatientSessionManager.Instance.UpdatePeakMetric(wallCycles);
        }

        // Keyboard Escape or Controller Menu opens Pause Menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            var pauseMenu = FindObjectOfType<VRInGamePauseMenu>();
            if (pauseMenu != null)
            {
                pauseMenu.Pause();
            }
            else
            {
                SaveAndReturn(completed: false);
            }
        }
    }

    public void SaveAndReturn(bool completed = false)
    {
        xposmove.sessionStarted = false;
        xnegmove.sessionStarted = false;

        float cycleDuration = TherapySettingsManager.Instance.clausMoveDuration + TherapySettingsManager.Instance.clausMoveAfter;
        int count = (cycleDuration > 0.1f) ? (int)(xnegmove.tmeas / cycleDuration) : 0;
        string summary = $"Speed: {TherapySettingsManager.Instance.clausSpeed:F3}, Cycles: {count}";

        PatientSessionManager.Instance.EndAndSaveSession(completed: completed, extraParams: summary);
        VRFadeManager.Instance.FadeAndLoadScene(0, 0.5f);
    }
}
