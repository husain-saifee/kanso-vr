using UnityEngine;
using DePhobia.Core;
using DePhobia.VR;

/// <summary>
/// Scene lifecycle controller for Acrophobia (High Elevation) simulation.
/// Initializes the patient session, anchors VRPlayerRig to the ascending hoverboard,
/// spawns the in-world emergency exit beacon, and conducts the calming start ritual.
/// </summary>
public class StartVr3 : MonoBehaviour
{
    private vrlook _lookScript;

    private void Start()
    {
        PatientSessionManager.Instance.StartSession("Acrophobia");

        _lookScript = FindObjectOfType<vrlook>();
        var cam = Camera.main;

        // Ensure VR Rig is properly anchored to the hoverboard
        var rig = FindObjectOfType<VRPlayerRig>();
        if (rig == null)
        {
            var rigGo = new GameObject("VRPlayerRig");
            if (_lookScript != null)
            {
                // Anchor to hoverboard so the patient moves together with elevation
                rigGo.transform.SetParent(_lookScript.transform, false);
                rigGo.transform.localPosition = Vector3.zero;
            }

            rig = rigGo.AddComponent<VRPlayerRig>();
            rig.enableLocomotion = false; // Hoverboard governs platform ascension
            rig.enableGravity = false;

            if (cam != null)
            {
                cam.transform.SetParent(rigGo.transform, false);
                cam.transform.localPosition = new Vector3(0, 1.6f, 0);
                rig.headCamera = cam.transform;
            }
        }

        if (_lookScript != null && cam != null)
        {
            _lookScript.vrCamera = cam.transform;
            _lookScript.enabled = false; // Hold until Start Ritual completes
        }

        // Anchor in-world emergency exit beacon to the hoverboard
        if (_lookScript != null)
        {
            VREmergencyExitBeacon.EnsureBeaconInScene(_lookScript.transform, new Vector3(-0.85f, 1.25f, 1.2f));
        }
        else
        {
            VREmergencyExitBeacon.EnsureBeaconInScene();
        }

        // Fade in smoothly from black
        VRFadeManager.Instance.FadeIn(0.6f, () =>
        {
            // Conduct calm 3-breath start ritual before height mechanics commence
            TherapySessionRitual.ShowStartRitual("High Elevation", () =>
            {
                if (_lookScript != null)
                {
                    _lookScript.enabled = true;
                    TherapyAudioManager.TriggerCaption("Look downward to ascend gently. Look forward to pause.", 5.0f);
                }
            });
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            var pauseMenu = FindObjectOfType<VRInGamePauseMenu>();
            if (pauseMenu != null)
            {
                pauseMenu.Pause();
            }
            else
            {
                SaveAndReturn();
            }
        }
    }

    public void SaveAndReturn()
    {
        bool completed = (vrlook.e == 1);
        string summary = $"WaitTime: {TherapySettingsManager.Instance.acroWaitTime:F1}s, FocusTime: {TherapySettingsManager.Instance.acroStillFocusTime:F1}s, PeakAltitude: {ValChange.HighestHigh:F1}m";

        PatientSessionManager.Instance.EndAndSaveSession(completed: completed, extraParams: summary);
        VRFadeManager.Instance.FadeAndLoadScene(0, 0.5f);
    }
}
