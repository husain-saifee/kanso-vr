using UnityEngine;
using DePhobia.Core;
using DePhobia.VR;

/// <summary>
/// Scene lifecycle controller for Claustrophobia (Enclosed Space) simulation.
/// Initializes the patient session, manages the VR Player Rig, spawns the emergency beacon,
/// and conducts the calming start/end rituals.
/// </summary>
public class StartVr : MonoBehaviour
{
    private xposmove _posWall;
    private xnegmove _negWall;
    private bool _wallsActive = false;
    private bool _completionShown = false;

    private void Start()
    {
        PatientSessionManager.Instance.StartSession("Claustrophobia");

        _posWall = FindObjectOfType<xposmove>();
        _negWall = FindObjectOfType<xnegmove>();

        if (_posWall != null) _posWall.enabled = false;
        if (_negWall != null) _negWall.enabled = false;

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

        // Ensure in-world emergency exit beacon exists
        VREmergencyExitBeacon.EnsureBeaconInScene(null, new Vector3(-1.1f, 1.4f, 1.8f));

        // Fade in from black smoothly
        VRFadeManager.Instance.FadeIn(0.6f, () =>
        {
            // Conduct calm Start Ritual
            TherapySessionRitual.ShowStartRitual("Enclosed Space", () =>
            {
                _wallsActive = true;
                if (_posWall != null) _posWall.enabled = true;
                if (_negWall != null) _negWall.enabled = true;

                TherapyAudioManager.TriggerCaption("Walls will begin moving gently. Take slow, steady breaths.", 5.0f);
            });
        });
    }

    private void Update()
    {
        if (_wallsActive && !_completionShown)
        {
            // Check if walls reached the safe boundary (-2.8m and +2.8m)
            if (_posWall != null && _posWall.transform.position.x >= -2.85f &&
                _negWall != null && _negWall.transform.position.x <= 2.85f)
            {
                _completionShown = true;
                TherapyAudioManager.TriggerCaption("Walls have stabilized safely. Take a moment to breathe.", 5.0f);

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
        float cycleDuration = TherapySettingsManager.Instance.clausMoveDuration + TherapySettingsManager.Instance.clausMoveAfter;
        int count = (cycleDuration > 0.1f) ? (int)(xnegmove.tmeas / cycleDuration) : 0;
        string summary = $"Speed: {TherapySettingsManager.Instance.clausSpeed:F3}, Cycles: {count}";

        PatientSessionManager.Instance.EndAndSaveSession(completed: completed, extraParams: summary);
        VRFadeManager.Instance.FadeAndLoadScene(0, 0.5f);
    }
}
