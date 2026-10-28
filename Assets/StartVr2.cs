using UnityEngine;
using DePhobia.Core;
using DePhobia.VR;

/// <summary>
/// Scene lifecycle controller for Aquaphobia (Rising Water) simulation.
/// and conducts the calming start/end rituals under the Forest Mineral & Sage visual language.
/// </summary>
public class StartVr2 : MonoBehaviour
{
    private MoreHeight1 _waterScript;
    private bool _waterActive = false;
    private bool _completionShown = false;

    private void Start()
    {
        PatientSessionManager.Instance.StartSession("Aquaphobia");

        // Enable atmospheric coastal distance fog (Aquaphobia only)
        RenderSettings.fog = false;

        _waterScript = FindObjectOfType<MoreHeight1>();
        if (_waterScript != null) _waterScript.enabled = false;

        // Disable legacy VRLookWalk and duplicate CharacterControllers
        var lookWalk = FindObjectOfType<VRLookWalk>();
        if (lookWalk != null)
        {
            lookWalk.enabled = false;
        }

        var oldCc = GetComponent<CharacterController>();
        if (oldCc != null)
        {
            oldCc.enabled = false;
        }

        var oldRb = GetComponent<Rigidbody>();
        if (oldRb != null)
        {
            oldRb.isKinematic = true;
            oldRb.useGravity = false;
        }

        // Raycast down to find safe island sand elevation
        Vector3 spawnPos = new Vector3(-6f, 1.2f, 8f);
        if (Physics.Raycast(new Vector3(-6f, 25f, 8f), Vector3.down, out RaycastHit groundHit, 50f))
        {
            spawnPos = groundHit.point + Vector3.up * 0.15f;
        }

        // Ensure VR Rig exists at the player's island spawn
        var rig = FindObjectOfType<VRPlayerRig>();
        if (rig == null)
        {
            var rigGo = new GameObject("VRPlayerRig");
            rigGo.transform.position = spawnPos;
            rigGo.transform.rotation = transform.rotation;

            rig = rigGo.AddComponent<VRPlayerRig>();
            var cam = Camera.main;
            if (cam != null)
            {
                var camCols = cam.GetComponentsInChildren<Collider>();
                foreach (var col in camCols) Destroy(col);
                var camRb = cam.GetComponent<Rigidbody>();
                if (camRb != null) Destroy(camRb);

                cam.transform.SetParent(rigGo.transform, false);
                cam.transform.localPosition = new Vector3(0, 1.6f, 0);
                rig.headCamera = cam.transform;
            }
        }
        else
        {
            rig.transform.position = spawnPos;
        }

        VREmergencyExitBeacon.EnsureBeaconInScene(null, new Vector3(-1.2f, 1.3f, 1.8f));

        // Fade in from black smoothly
        VRFadeManager.Instance.FadeIn(0.6f, () =>
        {
            // Conduct calm Start Ritual
            TherapySessionRitual.ShowStartRitual("Rising Water", () =>
            {
                _waterActive = true;
                if (_waterScript != null) _waterScript.enabled = true;

                var r = FindObjectOfType<VRPlayerRig>();
                if (r != null) r.SendHapticImpulse(0.2f, 0.08f, UnityEngine.XR.XRNode.RightHand);
            });
        });
    }

    private void Update()
    {
        if (_waterActive && !_completionShown && _waterScript != null)
        {
            // Safe height boundary clamped around 1.72m (minimum 20s active exposure)
            if (_waterScript.transform.position.y >= 1.72f && PatientSessionManager.Instance.GetCurrentSessionDuration() > 20f)
            {
                _completionShown = true;

                var rig = FindObjectOfType<VRPlayerRig>();
                if (rig != null) rig.SendHapticImpulse(0.3f, 0.12f, UnityEngine.XR.XRNode.RightHand);

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



    private void OnDestroy()
    {
        RenderSettings.fog = false;
    }

    public void SaveAndReturn(bool completed = false)
    {
        RenderSettings.fog = false;
        string summary = $"WaterSpeed: {TherapySettingsManager.Instance.aquaWaterSpeed:F3}, WaterRiseAfter: {TherapySettingsManager.Instance.aquaRiseAfter:F1}s";
        PatientSessionManager.Instance.EndAndSaveSession(completed: completed, extraParams: summary);
        VRFadeManager.Instance.FadeAndLoadScene(0, 0.5f);
    }
}
