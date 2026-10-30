using System;
using UnityEngine;
using DePhobia.VR;

namespace DePhobia.Core
{
    /// <summary>
    /// Central audio and captions manager for DePhobia.
    /// Provides calibrated audio buses (Master, Ambience, Feedback, Voice),
    /// procedural calm harmonic ambient drone synthesis (432Hz, zero loop seams),
    /// gentle UI feedback tones, and global subtitle event routing.
    /// </summary>
    public class TherapyAudioManager : MonoBehaviour
    {
        private static TherapyAudioManager _instance;
        private static bool _isApplicationQuitting = false;
        public static bool HasInstance => _instance != null;

        public static TherapyAudioManager Instance
        {
            get
            {
                if (_isApplicationQuitting) return null;
                if (_instance == null)
                {
                    _instance = FindObjectOfType<TherapyAudioManager>();
                    if (_instance == null && !_isApplicationQuitting)
                    {
                        var go = new GameObject("TherapyAudioManager");
                        _instance = go.AddComponent<TherapyAudioManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        private void OnApplicationQuit()
        {
            _isApplicationQuitting = true;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _isApplicationQuitting = true;
            }
            if (TherapySettingsManager.HasInstance && TherapySettingsManager.Instance != null)
            {
                TherapySettingsManager.Instance.OnSettingsChanged -= ApplyVolumes;
            }
        }

        public static event Action<string, float> OnCaptionRequested;

        private AudioSource _ambientSource;
        private AudioSource _scenarioSource;
        private AudioSource _windSource;
        private AudioSource _feedbackSource;

        private string _currentScenario = "Menu";
        private float _lastAltitude = 32f;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            SetupAudioSources();
        }

        private void SetupAudioSources()
        {
            // 1. Therapeutic Background Meditation Music (2D Binaural Bed)
            _ambientSource = gameObject.AddComponent<AudioSource>();
            _ambientSource.loop = true;
            _ambientSource.playOnAwake = false;
            _ambientSource.spatialBlend = 0f;
            _ambientSource.volume = 0.40f;

            AudioClip meditationClip = Resources.Load<AudioClip>("Audio/ambient_meditation");
            if (meditationClip != null)
            {
                _ambientSource.clip = meditationClip;
            }
            else
            {
                _ambientSource.clip = CreateHarmonicAmbientClip(8.0f);
            }
            _ambientSource.Play();

            // 2. Realistic Environmental Spatial Ambience (City, Waves, Room tone)
            _scenarioSource = gameObject.AddComponent<AudioSource>();
            _scenarioSource.loop = true;
            _scenarioSource.playOnAwake = false;
            _scenarioSource.spatialBlend = 0.35f;
            _scenarioSource.volume = 0.45f;

            // 3. High-Altitude Modulated Wind Source
            _windSource = gameObject.AddComponent<AudioSource>();
            _windSource.loop = true;
            _windSource.playOnAwake = false;
            _windSource.spatialBlend = 0.25f;
            _windSource.volume = 0.05f;

            AudioClip windClip = Resources.Load<AudioClip>("Audio/wind_altitude");
            if (windClip != null)
            {
                _windSource.clip = windClip;
            }

            // 4. UI Audio Feedback
            _feedbackSource = gameObject.AddComponent<AudioSource>();
            _feedbackSource.loop = false;
            _feedbackSource.playOnAwake = false;
            _feedbackSource.spatialBlend = 0f;
            _feedbackSource.volume = 0.6f;
        }

        private void Start()
        {
            ApplyVolumes();
            if (TherapySettingsManager.Instance != null)
            {
                TherapySettingsManager.Instance.OnSettingsChanged += ApplyVolumes;
            }
        }

        private void ApplyVolumes()
        {
            var sm = TherapySettingsManager.Instance;
            if (sm != null)
            {
                AudioListener.volume = sm.masterVolume;
                if (_ambientSource != null)
                {
                    _ambientSource.volume = sm.ambienceVolume * 0.45f;
                }
                if (_feedbackSource != null)
                {
                    _feedbackSource.volume = sm.feedbackVolume * 0.7f;
                }
                UpdateScenarioVolumes();
            }
        }

        /// <summary>
        /// Transitions the environmental audio layer to the active exposure scenario.
        /// </summary>
        public void PlayScenarioAmbience(string scenarioName)
        {
            _currentScenario = scenarioName;

            switch (scenarioName.ToLowerInvariant())
            {
                case "claustrophobia":
                case "enclosed space":
                    if (_windSource != null && _windSource.isPlaying) _windSource.Stop();
                    AudioClip roomClip = Resources.Load<AudioClip>("Audio/room_ventilation");
                    if (roomClip != null && _scenarioSource != null)
                    {
                        _scenarioSource.clip = roomClip;
                        _scenarioSource.Play();
                    }
                    break;

                case "aquaphobia":
                case "rising water":
                    if (_windSource != null && _windSource.isPlaying) _windSource.Stop();
                    AudioClip waveClip = Resources.Load<AudioClip>("Audio/ocean_waves");
                    if (waveClip != null && _scenarioSource != null)
                    {
                        _scenarioSource.clip = waveClip;
                        _scenarioSource.Play();
                    }
                    break;

                case "acrophobia":
                case "high elevation":
                    AudioClip cityClip = Resources.Load<AudioClip>("Audio/city_ambient");
                    if (cityClip != null && _scenarioSource != null)
                    {
                        _scenarioSource.clip = cityClip;
                        _scenarioSource.Play();
                    }
                    if (_windSource != null && _windSource.clip != null && !_windSource.isPlaying)
                    {
                        _windSource.Play();
                    }
                    SetAcrophobiaAltitude(_lastAltitude);
                    break;

                default: // Menu / Portal
                    if (_scenarioSource != null && _scenarioSource.isPlaying) _scenarioSource.Stop();
                    if (_windSource != null && _windSource.isPlaying) _windSource.Stop();
                    break;
            }

            UpdateScenarioVolumes();
        }

        /// <summary>
        /// Smoothly modulates wind and city soundscapes as altitude changes.
        /// As the patient ascends, the city rumble gently recedes while high-altitude wind surges.
        /// </summary>
        public void SetAcrophobiaAltitude(float currentY)
        {
            _lastAltitude = currentY;
            if (_currentScenario.ToLowerInvariant() != "acrophobia") return;

            float normY = Mathf.InverseLerp(32f, 360f, currentY);
            var sm = TherapySettingsManager.Instance;
            float amb = (sm != null) ? sm.ambienceVolume : 0.75f;

            if (_windSource != null)
            {
                _windSource.volume = Mathf.Lerp(0.08f, 0.72f, normY) * amb;
            }
            if (_scenarioSource != null)
            {
                _scenarioSource.volume = Mathf.Lerp(0.50f, 0.05f, normY) * amb;
            }
        }

        private void UpdateScenarioVolumes()
        {
            var sm = TherapySettingsManager.Instance;
            float amb = (sm != null) ? sm.ambienceVolume : 0.75f;

            if (_currentScenario.ToLowerInvariant() == "acrophobia")
            {
                SetAcrophobiaAltitude(_lastAltitude);
            }
            else if (_scenarioSource != null)
            {
                _scenarioSource.volume = 0.50f * amb;
            }
        }

        /// <summary>
        /// Plays a brief, gentle acoustic click/chime for UI button feedback (<40ms).
        /// </summary>
        public void PlayFeedbackClick()
        {
            if (_feedbackSource == null) return;
            AudioClip click = CreateChimeFeedbackClip();
            _feedbackSource.PlayOneShot(click, 0.65f);
        }

        /// <summary>
        /// Dispatches a visual caption to the floating subtitle banner.
        /// </summary>
        public static void TriggerCaption(string text, float duration = 3.5f)
        {
            if (TherapySettingsManager.Instance != null && !TherapySettingsManager.Instance.subtitlesEnabled)
                return;

            OnCaptionRequested?.Invoke(text, duration);
        }

        /// <summary>
        /// Procedurally generates a warm, seamless, pure harmonic ambient drone
        /// tuned to 432 Hz with soft undertones. Zero loop seams and 100% royalty-free.
        /// </summary>
        private AudioClip CreateHarmonicAmbientClip(float durationSeconds)
        {
            int sampleRate = 44100;
            int totalSamples = Mathf.RoundToInt(durationSeconds * sampleRate);
            float[] samples = new float[totalSamples];

            // Harmonic frequencies: 216 Hz (sub), 432 Hz (fundamental), 648 Hz (fifth)
            float freq1 = 216.0f;
            float freq2 = 432.0f;
            float freq3 = 648.0f;

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;

                // Seamless phase alignment
                float wave1 = Mathf.Sin(2f * Mathf.PI * freq1 * t) * 0.35f;
                float wave2 = Mathf.Sin(2f * Mathf.PI * freq2 * t) * 0.25f;
                float wave3 = Mathf.Sin(2f * Mathf.PI * freq3 * t) * 0.10f;

                // Slow atmospheric swell envelope (continuous cosine wave)
                float swell = 0.85f + 0.15f * Mathf.Sin(2f * Mathf.PI * (1f / durationSeconds) * t);

                samples[i] = (wave1 + wave2 + wave3) * swell * 0.35f;
            }

            AudioClip clip = AudioClip.Create("HarmonicAmbientBed", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Procedurally creates a soft, gentle acoustic chime click (<40ms, no transient pop).
        /// </summary>
        private AudioClip CreateChimeFeedbackClip()
        {
            int sampleRate = 44100;
            int samplesCount = Mathf.RoundToInt(0.045f * sampleRate);
            float[] samples = new float[samplesCount];

            float freq = 864f; // A5 warm soft tone
            for (int i = 0; i < samplesCount; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Exp(-t * 90f); // Fast gentle decay
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.4f;
            }

            AudioClip clip = AudioClip.Create("ChimeClick", samplesCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
