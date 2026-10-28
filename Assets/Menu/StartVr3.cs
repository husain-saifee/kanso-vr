using UnityEngine;
using DePhobia.Core;
using DePhobia.VR;

/// <summary>
/// Scene lifecycle controller for Acrophobia (High Elevation) simulation.
/// Initializes the patient session, anchors VRPlayerRig to the ascending hoverboard,
/// positions hoverboard safely over the city avenue, and conducts the calming start ritual
/// under the Forest Mineral & Sage visual language.
/// </summary>
public class StartVr3 : MonoBehaviour
{
    private vrlook _lookScript;

    private void Start()
    {
        PatientSessionManager.Instance.StartSession("Acrophobia");

        // Enable atmospheric city perspective distance fog (Acrophobia only)
        EnableAcrophobiaDistanceFog();

        _lookScript = FindObjectOfType<vrlook>();
        var cam = Camera.main;

        // Strip any conflicting Rigidbody or Collider on Camera.main from legacy scene
        if (cam != null)
        {
            var camRb = cam.GetComponent<Rigidbody>();
            if (camRb != null) Destroy(camRb);
            var camCol = cam.GetComponent<Collider>();
            if (camCol != null) Destroy(camCol);
        }

        // Reposition hoverboard deep into the city center avenue (228, 32, 145) surrounded by 350m skyscrapers
        if (_lookScript != null)
        {
            _lookScript.transform.position = new Vector3(228f, 32f, 145f);
            _lookScript.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

            var hoverRb = _lookScript.GetComponent<Rigidbody>();
            if (hoverRb != null)
            {
                hoverRb.useGravity = false;
                hoverRb.isKinematic = true;
                hoverRb.constraints = RigidbodyConstraints.FreezeAll;
            }
        }

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

            var cc = rig.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            if (cam != null)
            {
                cam.transform.SetParent(rigGo.transform, false);
                cam.transform.localPosition = new Vector3(0, 1.6f, 0);
                rig.headCamera = cam.transform;
            }
        }
        else
        {
            rig.enableLocomotion = false;
            rig.enableGravity = false;
            var cc = rig.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
        }

        if (_lookScript != null && cam != null)
        {
            _lookScript.vrCamera = cam.transform;
            _lookScript.enabled = false; // Hold until Start Ritual completes
        }

        // Ensure horizon respiration ring and emergency beacon exist
        VRHorizonRespirationRing.EnsureRingInScene();
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
                }
                var r = FindObjectOfType<VRPlayerRig>();
                if (r != null) r.SendHapticImpulse(0.2f, 0.08f, UnityEngine.XR.XRNode.RightHand);
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

    private void EnableAcrophobiaDistanceFog()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 35f;
        RenderSettings.fogEndDistance = 260f;
        RenderSettings.fogColor = new Color(0.62f, 0.66f, 0.64f, 1.0f); // Atmospheric city perspective haze
    }

    private void OnDestroy()
    {
        RenderSettings.fog = false;
    }

    public void SaveAndReturn()
    {
        RenderSettings.fog = false;
        bool completed = (vrlook.e == 1);
        string summary = $"WaitTime: {TherapySettingsManager.Instance.acroWaitTime:F1}s, FocusTime: {TherapySettingsManager.Instance.acroStillFocusTime:F1}s, PeakAltitude: {ValChange.HighestHigh:F1}m";

        PatientSessionManager.Instance.EndAndSaveSession(completed: completed, extraParams: summary);
        VRFadeManager.Instance.FadeAndLoadScene(0, 0.5f);
    }
}
