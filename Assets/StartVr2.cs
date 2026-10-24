using UnityEngine;
using DePhobia.Core;
using DePhobia.VR;

/// <summary>
/// Scene lifecycle controller for Aquaphobia (Rising Water) simulation.
/// Initializes the patient session, manages the VR Player Rig, spawns the emergency beacon,
/// and conducts the calming start/end rituals.
/// </summary>
public class StartVr2 : MonoBehaviour
{
    private MoreHeight1 _waterScript;
    private bool _waterActive = false;
    private bool _completionShown = false;

    private void Start()
    {
        PatientSessionManager.Instance.StartSession("Aquaphobia");

        _waterScript = FindObjectOfType<MoreHeight1>();
        if (_waterScript != null) _waterScript.enabled = false;

        // Disable legacy VRLookWalk if present to prevent duplicate CharacterControllers
        var lookWalk = FindObjectOfType<VRLookWalk>();
        if (lookWalk != null)
        {
            lookWalk.enabled = false;
        }

        // Ensure VR Rig exists at the player's island spawn
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
        VREmergencyExitBeacon.EnsureBeaconInScene(null, new Vector3(-1.2f, 1.3f, 1.8f));

        // Fade in from black smoothly
        VRFadeManager.Instance.FadeIn(0.6f, () =>
        {
            // Conduct calm Start Ritual
            TherapySessionRitual.ShowStartRitual("Rising Water", () =>
            {
                _waterActive = true;
                if (_waterScript != null) _waterScript.enabled = true;

                TherapyAudioManager.TriggerCaption("Water levels will begin rising gently. Take slow, steady breaths.", 5.0f);
            });
        });
    }

    private void Update()
    {
        if (_waterActive && !_completionShown && _waterScript != null)
        {
            // Safe height boundary clamped around 1.75m
            if (_waterScript.transform.position.y >= 1.72f)
            {
                _completionShown = true;
                TherapyAudioManager.TriggerCaption("Water levels have reached safe threshold. Take a moment to breathe.", 5.0f);

                string summary = $"Safe Water Threshold Reached • Height: {_waterScript.transform.position.y:F2}m";
                TherapySessionRitual.ShowEndRitual("Rising Water", summary, () =>
                {
                    SaveAndReturn(completed: true);
                });
            }
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
        string summary = $"WaterSpeed: {TherapySettingsManager.Instance.aquaWaterSpeed:F3}, WaterRiseAfter: {TherapySettingsManager.Instance.aquaRiseAfter:F1}s";
        PatientSessionManager.Instance.EndAndSaveSession(completed: completed, extraParams: summary);
        VRFadeManager.Instance.FadeAndLoadScene(0, 0.5f);
    }
}
