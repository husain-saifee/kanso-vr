using UnityEngine;
using DePhobia.Core;
using DePhobia.VR;

/// <summary>
/// Scene lifecycle controller for Claustrophobia simulation.
/// Initializes the patient session, manages the VR Player Rig, and records stats.
/// </summary>
public class StartVr : MonoBehaviour
{
    private void Start()
    {
        PatientSessionManager.Instance.StartSession("Claustrophobia");

        // Ensure VR Rig exists in scene
        var rig = FindObjectOfType<VRPlayerRig>();
        if (rig == null)
        {
            var rigGo = new GameObject("VRPlayerRig");
            rig = rigGo.AddComponent<VRPlayerRig>();
            var cam = Camera.main;
            if (cam != null)
            {
                cam.transform.SetParent(rigGo.transform, false);
                rig.headCamera = cam.transform;
            }
        }

        // Fade in from black smoothly
        VRFadeManager.Instance.FadeIn(0.5f);
    }

    private void Update()
    {
        // Update peak metric (wall movement cycle count)
        float cycleDuration = TherapySettingsManager.Instance.clausMoveDuration + TherapySettingsManager.Instance.clausMoveAfter;
        if (cycleDuration > 0.1f)
        {
            float wallCycles = xnegmove.tmeas / cycleDuration;
            PatientSessionManager.Instance.UpdatePeakMetric(wallCycles);
        }

        // Keyboard Escape or Controller Menu opens Pause Menu / Saves Session
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
        float cycleDuration = TherapySettingsManager.Instance.clausMoveDuration + TherapySettingsManager.Instance.clausMoveAfter;
        int count = (cycleDuration > 0.1f) ? (int)(xnegmove.tmeas / cycleDuration) : 0;
        string summary = $"Speed: {TherapySettingsManager.Instance.clausSpeed:F3}, Cycles: {count}";

        PatientSessionManager.Instance.EndAndSaveSession(completed: false, extraParams: summary);
        VRFadeManager.Instance.FadeAndLoadScene(0, 0.4f);
    }
}
