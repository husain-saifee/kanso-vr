using UnityEngine;
using DePhobia.Core;
using DePhobia.VR;

/// <summary>
/// Scene lifecycle controller for Acrophobia (High Elevation) simulation.
/// Spawns the hoverboard in the central avenue canyon at (200, 32, 315) with 11m+ wall clearance,
/// anchors VRPlayerRig, creates an expansive low-altitude ground cloud/mist deck below,
/// and dynamically calculates atmospheric altitude fog as the patient ascends.
/// </summary>
public class StartVr3 : MonoBehaviour
{
    private vrlook _lookScript;
    private GameObject _groundMistDeck;

    private void Start()
    {
        PatientSessionManager.Instance.StartSession("Acrophobia");
        TherapyAudioManager.Instance.PlayScenarioAmbience("Acrophobia");

        // 1. Configure baseline Acrophobia atmospheric distance fog
        EnableAcrophobiaDistanceFog();

        // 2. Generate procedural low-altitude sea-of-clouds ground mist deck covering the street grid below
        CreateGroundMistDeck();

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

        // 3. Position hoverboard in the open avenue (298, 32, 86) with clear airspace and wide wall clearance
        if (_lookScript != null)
        {
            _lookScript.transform.position = new Vector3(vrlook.CENTER_SPAWN_X, 32f, vrlook.CENTER_SPAWN_Z);
            _lookScript.transform.rotation = Quaternion.Euler(0f, 45f, 0f);

            var hoverRb = _lookScript.GetComponent<Rigidbody>();
            if (hoverRb != null)
            {
                hoverRb.useGravity = false;
                hoverRb.isKinematic = true;
                hoverRb.constraints = RigidbodyConstraints.FreezeAll;
            }

            // Ensure no colliders on the hoverboard snag or create invisible wall collisions with building geometry
            var hoverColliders = _lookScript.GetComponentsInChildren<Collider>();
            foreach (var col in hoverColliders)
            {
                col.isTrigger = true;
            }
        }

        // 4. Ensure VR Rig is properly anchored to the hoverboard
        var rig = FindObjectOfType<VRPlayerRig>();
        if (rig == null)
        {
            var rigGo = new GameObject("VRPlayerRig");
            if (_lookScript != null)
            {
                rigGo.transform.SetParent(_lookScript.transform, false);
                rigGo.transform.localPosition = Vector3.zero;
            }

            rig = rigGo.AddComponent<VRPlayerRig>();
            rig.enableLocomotion = false;
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
            // Conduct calm start ritual before height mechanics commence
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
        // Dynamically update atmospheric fog as hoverboard ascends
        UpdateAcrophobiaAltitudeFog();

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
        RenderSettings.fogStartDistance = 60f;
        RenderSettings.fogEndDistance = 350f;
        RenderSettings.fogColor = new Color(0.76f, 0.80f, 0.83f, 1.0f); // Soft atmospheric perspective haze
    }

    private void UpdateAcrophobiaAltitudeFog()
    {
        if (_lookScript == null) return;

        float currentAltitude = _lookScript.transform.position.y;
        float progress = Mathf.InverseLerp(32f, 360f, currentAltitude);

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        // As altitude rises from 32m to 360m:
        // Fog distance expands generously so surrounding skyscrapers remain clear and striking,
        // while the distant horizon softly blends into atmospheric skyline haze
        RenderSettings.fogStartDistance = Mathf.Lerp(60f, 120f, progress);
        RenderSettings.fogEndDistance = Mathf.Lerp(350f, 550f, progress);

        Color streetHaze = new Color(0.75f, 0.79f, 0.82f, 1.0f);
        Color skyCloud = new Color(0.85f, 0.89f, 0.92f, 1.0f);
        RenderSettings.fogColor = Color.Lerp(streetHaze, skyCloud, progress);
    }

    /// <summary>
    /// Generates a gentle low-altitude cloud mist sheet resting down near street level (Y = 17.5m).
    /// When looking down from height, streets below are softly shrouded without obstructing
    /// surrounding skyscrapers or nearby architecture.
    /// </summary>
    private void CreateGroundMistDeck()
    {
        var existing = GameObject.Find("AcrophobiaGroundMistDeck");
        if (existing != null) return;

        _groundMistDeck = new GameObject("AcrophobiaGroundMistDeck");
        _groundMistDeck.transform.position = new Vector3(vrlook.CENTER_SPAWN_X, 17.5f, vrlook.CENTER_SPAWN_Z);

        // Soft, ethereal ground cloud blanket resting right above street floor
        var layer1 = GameObject.CreatePrimitive(PrimitiveType.Plane);
        layer1.name = "GroundCloudLayer_1";
        layer1.transform.SetParent(_groundMistDeck.transform, false);
        layer1.transform.localPosition = Vector3.zero;
        layer1.transform.localScale = new Vector3(45f, 1f, 45f);

        var col1 = layer1.GetComponent<Collider>();
        if (col1 != null) Destroy(col1);

        var rend1 = layer1.GetComponent<MeshRenderer>();
        if (rend1 != null)
        {
            var mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = new Color(0.82f, 0.86f, 0.90f, 0.20f); // Light, ethereal ground mist
            rend1.material = mat;
            rend1.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rend1.receiveShadows = false;
        }
    }

    private void OnDestroy()
    {
        RenderSettings.fog = false;
    }

    public void SaveAndReturn()
    {
        RenderSettings.fog = false;
        TherapyAudioManager.Instance.PlayScenarioAmbience("Menu");
        bool completed = (vrlook.e == 1);
        string summary = $"WaitTime: {TherapySettingsManager.Instance.acroWaitTime:F1}s, FocusTime: {TherapySettingsManager.Instance.acroStillFocusTime:F1}s, PeakAltitude: {ValChange.HighestHigh:F1}m";

        PatientSessionManager.Instance.EndAndSaveSession(completed: completed, extraParams: summary);
        VRFadeManager.Instance.FadeAndLoadScene(0, 0.5f);
    }
}
