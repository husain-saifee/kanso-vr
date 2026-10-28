using UnityEngine;
using DePhobia.Core;
using DePhobia.VR;

/// <summary>
/// Scene lifecycle controller for Acrophobia (High Elevation) simulation.
/// Spawns the hoverboard in the true geographic center of the city (160, 32, 180),
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

        // 3. Position hoverboard directly in the center of the city (160, 32, 180) facing down Broadway canyon
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
        RenderSettings.fogStartDistance = 2f;
        RenderSettings.fogEndDistance = 85f;
        RenderSettings.fogColor = new Color(0.72f, 0.76f, 0.79f, 1.0f); // Atmospheric city perspective haze
    }

    private void UpdateAcrophobiaAltitudeFog()
    {
        if (_lookScript == null) return;

        float currentAltitude = _lookScript.transform.position.y;
        float progress = Mathf.InverseLerp(32f, 360f, currentAltitude);

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        // As altitude rises from 32m to 360m:
        // Fog distance expands smoothly to give panoramic aerial views of skyscraper tops,
        // while the street floor below (hundreds of meters away) remains completely shrouded in clouds!
        RenderSettings.fogStartDistance = Mathf.Lerp(2f, 15f, progress);
        RenderSettings.fogEndDistance = Mathf.Lerp(85f, 160f, progress);

        Color streetHaze = new Color(0.70f, 0.75f, 0.78f, 1.0f);
        Color skyCloud = new Color(0.84f, 0.88f, 0.91f, 1.0f);
        RenderSettings.fogColor = Color.Lerp(streetHaze, skyCloud, progress);
    }

    /// <summary>
    /// Generates a layered low-altitude sea-of-clouds ground mist deck at Y = 22m.
    /// When the player ascends into the sky and looks down, the streets below are
    /// shrouded in a soft, ethereal blanket of clouds, creating an authentic sense of extreme altitude.
    /// </summary>
    private void CreateGroundMistDeck()
    {
        var existing = GameObject.Find("AcrophobiaGroundMistDeck");
        if (existing != null) return;

        _groundMistDeck = new GameObject("AcrophobiaGroundMistDeck");
        _groundMistDeck.transform.position = new Vector3(vrlook.CENTER_SPAWN_X, 22f, vrlook.CENTER_SPAWN_Z);

        // Layer 1: Base ground mist blanket (covers entire city grid: 450m x 450m)
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
            mat.color = new Color(0.78f, 0.82f, 0.85f, 0.65f); // Soft cloud mist
            rend1.material = mat;
            rend1.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rend1.receiveShadows = false;
        }

        // Layer 2: Secondary offset mist sheet at Y = 25m for depth perception
        var layer2 = GameObject.CreatePrimitive(PrimitiveType.Plane);
        layer2.name = "GroundCloudLayer_2";
        layer2.transform.SetParent(_groundMistDeck.transform, false);
        layer2.transform.localPosition = new Vector3(15f, 3f, 15f);
        layer2.transform.localScale = new Vector3(40f, 1f, 40f);

        var col2 = layer2.GetComponent<Collider>();
        if (col2 != null) Destroy(col2);

        var rend2 = layer2.GetComponent<MeshRenderer>();
        if (rend2 != null)
        {
            var mat2 = new Material(Shader.Find("Sprites/Default"));
            mat2.color = new Color(0.82f, 0.86f, 0.89f, 0.45f); // Lighter mist veil
            rend2.material = mat2;
            rend2.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rend2.receiveShadows = false;
        }
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
