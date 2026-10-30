using System;
using UnityEngine;
using DePhobia.VR;

namespace DePhobia.Core
{
    /// <summary>
    /// Central audio and acoustic depth manager for DePhobia.
    /// Provides calibrated audio buses (Master, Ambience, Feedback, Wind, Scenario),
    /// dynamic room geometry via AudioReverbZone, proximity low-pass filtering,
    /// procedural calm harmonic ambient drone synthesis (432Hz, zero loop seams),
    /// micro-tactile UI hover/click ticks, and structural physical soundscapes.
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
        private AudioReverbZone _reverbZone;
        private AudioLowPassFilter _listenerLowPass;
        private AudioListener _currentListener;

        private string _currentScenario = "Menu";
        private float _lastAltitude = 32f;
        private float _roomProximity = 0f;
        private float _waterSubmersion = 0f;
        private float _currentCutoff = 22000f;
        private float _targetCutoff = 22000f;

        // Cached procedural audio clips
        private AudioClip _chimeClip;
        private AudioClip _hoverTickClip;
        private AudioClip _wallCreakClip;
        private AudioClip _waterLapClip;

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
            SetupAcousticZone();
            PreloadProceduralClips();
        }

        private void SetupAcousticZone()
        {
            _reverbZone = GetComponent<AudioReverbZone>();
            if (_reverbZone == null)
            {
                _reverbZone = gameObject.AddComponent<AudioReverbZone>();
            }
            _reverbZone.minDistance = 60f;
            _reverbZone.maxDistance = 300f;
            _reverbZone.reverbPreset = AudioReverbPreset.Livingroom;
        }

        private void PreloadProceduralClips()
        {
            _chimeClip = CreateChimeFeedbackClip();
            _hoverTickClip = CreateHoverTickClip();
            _wallCreakClip = CreateWallCreakClip();
            _waterLapClip = CreateWaterLapClip();
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

            // 4. UI Audio Feedback & Tactile Point-of-Contact
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

        private void Update()
        {
            UpdateListenerTracking();
            UpdateLowPassFiltration();
        }

        private void UpdateListenerTracking()
        {
            // Ensure ReverbZone center anchors to active player camera in world space
            if (Camera.main != null)
            {
                transform.position = Camera.main.transform.position;
            }

            // Ensure AudioListener has an AudioLowPassFilter attached
            if (_listenerLowPass == null)
            {
                var listener = FindObjectOfType<AudioListener>();
                if (listener != null)
                {
                    _currentListener = listener;
                    _listenerLowPass = listener.GetComponent<AudioLowPassFilter>();
                    if (_listenerLowPass == null)
                    {
                        _listenerLowPass = listener.gameObject.AddComponent<AudioLowPassFilter>();
                    }
                }
            }
        }

        private void UpdateLowPassFiltration()
        {
            if (_listenerLowPass != null)
            {
                _currentCutoff = Mathf.MoveTowards(_currentCutoff, _targetCutoff, 18000f * Time.unscaledDeltaTime);
                _listenerLowPass.cutoffFrequency = _currentCutoff;
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
        /// Transitions the environmental audio layer and acoustic reverb zone to the active exposure scenario.
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
                    if (_reverbZone != null) _reverbZone.reverbPreset = AudioReverbPreset.Room;
                    _targetCutoff = 22000f;
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
                    if (_reverbZone != null) _reverbZone.reverbPreset = AudioReverbPreset.Plain;
                    _targetCutoff = 22000f;
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
                    if (_reverbZone != null) _reverbZone.reverbPreset = AudioReverbPreset.City;
                    _targetCutoff = 22000f;
                    SetAcrophobiaAltitude(_lastAltitude);
                    break;

                default: // Menu / Portal Foyer
                    if (_scenarioSource != null && _scenarioSource.isPlaying) _scenarioSource.Stop();
                    if (_windSource != null && _windSource.isPlaying) _windSource.Stop();
                    if (_reverbZone != null) _reverbZone.reverbPreset = AudioReverbPreset.Livingroom;
                    _targetCutoff = 22000f;
                    break;
            }

            UpdateScenarioVolumes();
        }

        /// <summary>
        /// Modulates acoustic confinement as walls contract inward in Claustrophobia.
        /// As the space compresses, early reflections tighten and high frequencies soften,
        /// creating physical acoustic intimacy rather than a sterile silent room.
        /// </summary>
        /// <param name="normalizedProximity">0.0 = walls at starting width; 1.0 = walls at safe limit (2.85m)</param>
        public void SetRoomProximity(float normalizedProximity)
        {
            _roomProximity = Mathf.Clamp01(normalizedProximity);
            if (_currentScenario.ToLowerInvariant().Contains("claus") || _currentScenario.ToLowerInvariant().Contains("enclosed"))
            {
                // As walls press in, acoustic flutter dampens from 22kHz down to 6kHz
                _targetCutoff = Mathf.Lerp(22000f, 6000f, _roomProximity);

                if (_reverbZone != null)
                {
                    _reverbZone.reverbPreset = (_roomProximity > 0.65f) ? AudioReverbPreset.PaddedCell : AudioReverbPreset.Room;
                }
            }
        }

        /// <summary>
        /// Modulates acoustic submersion as water rises toward and past player eye level in Aquaphobia.
        /// </summary>
        /// <param name="waterLevelRelative">0.0 = feet; 0.5 = waist; 0.8 = chest; 1.0 = head level</param>
        public void SetWaterSubmersion(float waterLevelRelative)
        {
            _waterSubmersion = Mathf.Clamp01(waterLevelRelative);
            if (_currentScenario.ToLowerInvariant().Contains("aqua") || _currentScenario.ToLowerInvariant().Contains("water"))
            {
                if (_waterSubmersion > 0.65f)
                {
                    float subT = Mathf.InverseLerp(0.65f, 1.0f, _waterSubmersion);
                    _targetCutoff = Mathf.Lerp(22000f, 650f, subT);
                    if (_reverbZone != null)
                    {
                        _reverbZone.reverbPreset = (subT > 0.5f) ? AudioReverbPreset.Underwater : AudioReverbPreset.Plain;
                    }
                }
                else
                {
                    _targetCutoff = 22000f;
                    if (_reverbZone != null) _reverbZone.reverbPreset = AudioReverbPreset.Plain;
                }
            }
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

            if (_reverbZone != null)
            {
                _reverbZone.reverbPreset = (normY > 0.45f) ? AudioReverbPreset.Mountains : AudioReverbPreset.City;
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
        /// Plays a brief, gentle acoustic click/chime for UI button activation (<40ms).
        /// </summary>
        public void PlayFeedbackClick()
        {
            if (_feedbackSource == null) return;
            if (_chimeClip == null) _chimeClip = CreateChimeFeedbackClip();
            _feedbackSource.PlayOneShot(_chimeClip, 0.65f);
        }

        /// <summary>
        /// Plays an ultra-short, soft acoustic tick when the spatial pointer hovers over a tactile UI element (<15ms).
        /// </summary>
        public void PlayHoverTick()
        {
            if (_feedbackSource == null) return;
            if (_hoverTickClip == null) _hoverTickClip = CreateHoverTickClip();
            _feedbackSource.PlayOneShot(_hoverTickClip, 0.35f);
        }

        /// <summary>
        /// Plays a subtle structural strain/creak tone when physical walls or platform start moving.
        /// </summary>
        public void PlayWallCreak(float volume = 0.4f)
        {
            if (_feedbackSource == null) return;
            if (_wallCreakClip == null) _wallCreakClip = CreateWallCreakClip();
            _feedbackSource.PlayOneShot(_wallCreakClip, Mathf.Clamp01(volume));
        }

        /// <summary>
        /// Plays a gentle shore/water lap sound when water passes a bodily threshold.
        /// </summary>
        public void PlayWaterLap(float volume = 0.45f)
        {
            if (_feedbackSource == null) return;
            if (_waterLapClip == null) _waterLapClip = CreateWaterLapClip();
            _feedbackSource.PlayOneShot(_waterLapClip, Mathf.Clamp01(volume));
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

        /// <summary>
        /// Procedurally creates a delicate, micro-acoustic tick for UI hover (<12ms, 1400Hz pulse).
        /// </summary>
        private AudioClip CreateHoverTickClip()
        {
            int sampleRate = 44100;
            int samplesCount = Mathf.RoundToInt(0.012f * sampleRate);
            float[] samples = new float[samplesCount];

            float freq = 1400f;
            for (int i = 0; i < samplesCount; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Exp(-t * 350f);
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.25f;
            }

            AudioClip clip = AudioClip.Create("HoverTick", samplesCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Procedurally creates a low structural creak/tension groan (<220ms).
        /// </summary>
        private AudioClip CreateWallCreakClip()
        {
            int sampleRate = 44100;
            int samplesCount = Mathf.RoundToInt(0.22f * sampleRate);
            float[] samples = new float[samplesCount];

            for (int i = 0; i < samplesCount; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Sin(Mathf.PI * (t / 0.22f)); // bell curve
                float f1 = Mathf.Sin(2f * Mathf.PI * 58f * t);
                float f2 = Mathf.Sin(2f * Mathf.PI * 116f * t) * 0.5f;
                float f3 = Mathf.Sin(2f * Mathf.PI * 174f * t) * 0.25f;
                samples[i] = (f1 + f2 + f3) * env * 0.35f;
            }

            AudioClip clip = AudioClip.Create("WallCreak", samplesCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Procedurally creates a gentle wave lap sound (<350ms).
        /// </summary>
        private AudioClip CreateWaterLapClip()
        {
            int sampleRate = 44100;
            int samplesCount = Mathf.RoundToInt(0.35f * sampleRate);
            float[] samples = new float[samplesCount];

            // Pinkish filtered noise swell
            System.Random rand = new System.Random(42);
            float b0 = 0, b1 = 0, b2 = 0;

            for (int i = 0; i < samplesCount; i++)
            {
                float t = (float)i / sampleRate;
                float white = (float)(rand.NextDouble() * 2.0 - 1.0);

                // Simple 3-pole pink filter
                b0 = 0.99765f * b0 + white * 0.0990460f;
                b1 = 0.96300f * b1 + white * 0.2965164f;
                b2 = 0.57000f * b2 + white * 1.0526913f;
                float pink = (b0 + b1 + b2 + white * 0.1848f) * 0.05f;

                float envelope = Mathf.Sin(Mathf.PI * (t / 0.35f));
                samples[i] = pink * envelope * 0.5f;
            }

            AudioClip clip = AudioClip.Create("WaterLap", samplesCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
