using System;
using UnityEngine;

namespace DePhobia.Core
{
    /// <summary>
    /// Central persistent manager for exposure therapy simulation parameters,
    /// audio volume, and VR comfort settings.
    /// Replaces legacy per-frame parsing in Update().
    /// </summary>
    public class TherapySettingsManager : MonoBehaviour
    {
        private static TherapySettingsManager _instance;
        public static TherapySettingsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("TherapySettingsManager");
                    _instance = go.AddComponent<TherapySettingsManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Claustrophobia Settings")]
        [Tooltip("Speed at which walls contract towards the patient (units/sec)")]
        public float clausSpeed = 0.05f;
        [Tooltip("Interval before walls begin contracting (seconds)")]
        public float clausMoveAfter = 3.0f;
        [Tooltip("Duration for each wall contraction cycle (seconds)")]
        public float clausMoveDuration = 2.0f;

        [Header("Aquaphobia Settings")]
        [Tooltip("Speed at which water rises (units/sec)")]
        public float aquaWaterSpeed = 0.06f;
        [Tooltip("Interval before water begins rising (seconds)")]
        public float aquaRiseAfter = 2.0f;
        [Tooltip("Duration for each water rise cycle (seconds)")]
        public float aquaRiseDuration = 2.0f;

        [Header("Acrophobia Settings")]
        [Tooltip("Max duration patient can look away before hoverboard descends (seconds)")]
        public float acroWaitTime = 4.0f;
        [Tooltip("Gaze duration required looking down to trigger ascension (seconds)")]
        public float acroStillFocusTime = 0.6f;
        [Tooltip("Enable ambient city sound effects")]
        public bool acroSoundEnabled = true;

        [Header("VR Comfort & Safety")]
        [Tooltip("Use snap turning instead of smooth turning to prevent motion sickness")]
        public bool snapTurn = true;
        [Tooltip("Angle per snap turn increment in degrees")]
        public float snapTurnAngle = 45f;
        [Tooltip("Enable peripheral vignette tunneling during movement")]
        public bool comfortVignetteEnabled = true;
        [Tooltip("Enable seated mode with auto camera height offset")]
        public bool seatedMode = false;
        [Tooltip("Master audio volume [0..1]")]
        public float masterVolume = 1.0f;

        // Legacy compatibility static accessors
        public static float ClausSpeed => Instance.clausSpeed;
        public static float ClausMoveAfter => Instance.clausMoveAfter;
        public static float ClausMoveDuration => Instance.clausMoveDuration;

        public static float AquaWaterSpeed => Instance.aquaWaterSpeed;
        public static float AquaRiseAfter => Instance.aquaRiseAfter;
        public static float AquaRiseDuration => Instance.aquaRiseDuration;

        public static float AcroWaitTime => Instance.acroWaitTime;
        public static float AcroStillFocusTime => Instance.acroStillFocusTime;
        public static bool AcroSoundEnabled => Instance.acroSoundEnabled;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }

        #region Safe Setters for UI Callbacks (Event Driven)

        public void SetClausSpeedFromInput(string value)
        {
            if (float.TryParse(value, out float parsed) && parsed > 0)
            {
                // In legacy UI, speed was entered as an integer (e.g. 5) and divided by 100
                clausSpeed = (parsed > 1f) ? parsed / 100f : parsed;
                SaveSettings();
            }
        }

        public void SetClausMoveAfterFromInput(string value)
        {
            if (float.TryParse(value, out float parsed) && parsed > 0)
            {
                clausMoveAfter = Mathf.Clamp(parsed, 0.5f, 60f);
                SaveSettings();
            }
        }

        public void SetClausMoveDurationFromInput(string value)
        {
            if (float.TryParse(value, out float parsed) && parsed > 0)
            {
                clausMoveDuration = Mathf.Clamp(parsed, 0.5f, 60f);
                SaveSettings();
            }
        }

        public void SetAquaSpeedFromInput(string value)
        {
            if (float.TryParse(value, out float parsed) && parsed > 0)
            {
                aquaWaterSpeed = (parsed > 1f) ? parsed / 100f : parsed;
                SaveSettings();
            }
        }

        public void SetAquaRiseAfterFromInput(string value)
        {
            if (float.TryParse(value, out float parsed) && parsed > 0)
            {
                aquaRiseAfter = Mathf.Clamp(parsed, 0.5f, 60f);
                SaveSettings();
            }
        }

        public void SetAquaRiseDurationFromInput(string value)
        {
            if (float.TryParse(value, out float parsed) && parsed > 0)
            {
                aquaRiseDuration = Mathf.Clamp(parsed, 0.5f, 60f);
                SaveSettings();
            }
        }

        public void SetAcroWaitTimeFromInput(string value)
        {
            if (float.TryParse(value, out float parsed) && parsed > 0)
            {
                acroWaitTime = Mathf.Clamp(parsed, 1.0f, 60f);
                SaveSettings();
            }
        }

        public void SetAcroStillFocusTimeFromInput(string value)
        {
            if (float.TryParse(value, out float parsed) && parsed > 0)
            {
                acroStillFocusTime = Mathf.Clamp(parsed, 0.1f, 10f);
                SaveSettings();
            }
        }

        public void SetAcroSoundEnabled(bool enabled)
        {
            acroSoundEnabled = enabled;
            SaveSettings();
        }

        public void SetSnapTurn(bool isSnap)
        {
            snapTurn = isSnap;
            SaveSettings();
        }

        public void SetComfortVignette(bool enabled)
        {
            comfortVignetteEnabled = enabled;
            SaveSettings();
        }

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            AudioListener.volume = masterVolume;
            SaveSettings();
        }

        #endregion

        #region Persistence

        public void SaveSettings()
        {
            PlayerPrefs.SetFloat("ClausSpeed", clausSpeed);
            PlayerPrefs.SetFloat("ClausMoveAfter", clausMoveAfter);
            PlayerPrefs.SetFloat("ClausMoveDuration", clausMoveDuration);

            PlayerPrefs.SetFloat("AquaWaterSpeed", aquaWaterSpeed);
            PlayerPrefs.SetFloat("AquaRiseAfter", aquaRiseAfter);
            PlayerPrefs.SetFloat("AquaRiseDuration", aquaRiseDuration);

            PlayerPrefs.SetFloat("AcroWaitTime", acroWaitTime);
            PlayerPrefs.SetFloat("AcroStillFocusTime", acroStillFocusTime);
            PlayerPrefs.SetInt("AcroSoundEnabled", acroSoundEnabled ? 1 : 0);

            PlayerPrefs.SetInt("SnapTurn", snapTurn ? 1 : 0);
            PlayerPrefs.SetFloat("SnapTurnAngle", snapTurnAngle);
            PlayerPrefs.SetInt("ComfortVignette", comfortVignetteEnabled ? 1 : 0);
            PlayerPrefs.SetInt("SeatedMode", seatedMode ? 1 : 0);
            PlayerPrefs.SetFloat("MasterVolume", masterVolume);

            PlayerPrefs.Save();
        }

        public void LoadSettings()
        {
            clausSpeed = PlayerPrefs.GetFloat("ClausSpeed", 0.05f);
            clausMoveAfter = PlayerPrefs.GetFloat("ClausMoveAfter", 3.0f);
            clausMoveDuration = PlayerPrefs.GetFloat("ClausMoveDuration", 2.0f);

            aquaWaterSpeed = PlayerPrefs.GetFloat("AquaWaterSpeed", 0.06f);
            aquaRiseAfter = PlayerPrefs.GetFloat("AquaRiseAfter", 2.0f);
            aquaRiseDuration = PlayerPrefs.GetFloat("AquaRiseDuration", 2.0f);

            acroWaitTime = PlayerPrefs.GetFloat("AcroWaitTime", 4.0f);
            acroStillFocusTime = PlayerPrefs.GetFloat("AcroStillFocusTime", 0.6f);
            acroSoundEnabled = PlayerPrefs.GetInt("AcroSoundEnabled", 1) == 1;

            snapTurn = PlayerPrefs.GetInt("SnapTurn", 1) == 1;
            snapTurnAngle = PlayerPrefs.GetFloat("SnapTurnAngle", 45f);
            comfortVignetteEnabled = PlayerPrefs.GetInt("ComfortVignette", 1) == 1;
            seatedMode = PlayerPrefs.GetInt("SeatedMode", 0) == 1;
            masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);

            AudioListener.volume = masterVolume;
        }

        #endregion
    }
}
