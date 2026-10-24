using UnityEngine;
using DePhobia.Core;
using DePhobia.VR;

/// <summary>
/// Scene lifecycle controller for Acrophobia simulation.
/// Initializes the patient session, manages the VR Player Rig, and records stats.
/// </summary>
public class StartVr3 : MonoBehaviour
{
    private void Start()
    {
        PatientSessionManager.Instance.StartSession("Acrophobia");

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
        bool completed = (vrlook.e == 1);
        string summary = $"WaitTime: {TherapySettingsManager.Instance.acroWaitTime:F1}s, FocusTime: {TherapySettingsManager.Instance.acroStillFocusTime:F1}s, MaxHeight: {ValChange.HighestHigh:F1}m";

        PatientSessionManager.Instance.EndAndSaveSession(completed: completed, extraParams: summary);
        VRFadeManager.Instance.FadeAndLoadScene(0, 0.4f);
    }
}
