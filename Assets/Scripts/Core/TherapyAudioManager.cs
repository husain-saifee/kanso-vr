using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using DePhobia.VR;

namespace DePhobia.Core
{
    /// <summary>
    /// Central audio and acoustic depth manager for DePhobia.
    /// Strictly manages audio separation between Menu (calm meditation bed) and Exposure Sessions
    /// (environmental spatial acoustics only, zero menu music overlap, zero session audio leaking into menu).
    /// Provides calibrated audio buses (Master, Ambience, Feedback, Wind, Scenario),
    /// dynamic room geometry via AudioReverbZone, proximity low-pass filtering,
    /// and clean scene-transition synchronization.
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

        private AudioSource _ambientSource;   // Menu background music (strictly menu only)
        private AudioSource _scenarioSource;  // Environmental room tone, waves, city rumble
        private AudioSource _windSource;      // Acrophobia high-altitude wind
        private AudioSource _feedbackSource;  // UI ticks and structural cues
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
        private AudioClip _cityProceduralFallback;

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

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.buildIndex == 0 || scene.name.ToLowerInvariant().Contains("menu"))
            {
                // Returned to Menu: Cleanly stop all session environmental audio and resume menu music
                PlayScenarioAmbience("Menu");

                // Deactivate any rogue scene AudioSources (such as legacy bensound)
                SilenceRogueAudioSources();
            }
            else
            {
                // In active exposure session: ensure menu music is strictly stopped
                if (_ambientSource != null)
                {
                    _ambientSource.Stop();
                }

                // Deactivate any rogue scene AudioSources in simulation environments
                SilenceRogueAudioSources();
            }
        }

        /// <summary>
        /// Silences and disables any stray scene-level AudioSources in newly loaded scenes
        /// (e.g. legacy bensound in Menu, duplicate OceanWaterArea wave loops in Aquaphobia,
        /// or room tone in Claustrophobia) so that TherapyAudioManager remains the sole audio authority.
        /// </summary>
        public void SilenceRogueAudioSources()
        {
            var sources = FindObjectsOfType<AudioSource>();
            foreach (var src in sources)
            {
                if (src == null) continue;
                if (src == _ambientSource || src == _scenarioSource || src == _windSource || src == _feedbackSource)
                    continue;

                // Stop and disable any stray scene-level background tracks
                src.Stop();
                src.volume = 0f;
                src.playOnAwake = false;
                src.enabled = false;
            }
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
            _cityProceduralFallback = CreateCityAmbientClip(6.0f);
        }

        private void SetupAudioSources()
        {
            // 1. Menu Background Meditation Bed (Only active in Menu/Foyer, NOT in sessions)
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

            // Only start ambient music if initialized within the Menu scene
            var currentScene = SceneManager.GetActiveScene();
            if (currentScene.buildIndex == 0 || currentScene.name.ToLowerInvariant().Contains("menu"))
            {
                _ambientSource.Play();
            }

            // 2. Realistic Environmental Spatial Ambience (City, Waves, Room tone - active in sessions)
            _scenarioSource = gameObject.AddComponent<AudioSource>();
            _scenarioSource.loop = true;
            _scenarioSource.playOnAwake = false;
            _scenarioSource.spatialBlend = 0.35f;
            _scenarioSource.volume = 0.45f;

            // 3. High-Altitude Modulated Wind Source (Acrophobia session only)
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
            SilenceRogueAudioSources();
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
                if (_feedbackSource != null)
                {
                    _feedbackSource.volume = sm.feedbackVolume * 0.7f;
                }
                UpdateScenarioVolumes();
            }
        }

        /// <summary>
        /// Transitions the environmental audio layer and acoustic reverb zone to the active exposure scenario.
        /// Strictly ensures that Menu music does not play in session, and session audio does not linger in Menu.
        /// </summary>
        public void PlayScenarioAmbience(string scenarioName)
        {
            _currentScenario = scenarioName;

            var sm = TherapySettingsManager.Instance;
            float amb = (sm != null) ? sm.ambienceVolume : 0.75f;

            switch (scenarioName.ToLowerInvariant())
            {
                case "claustrophobia":
                case "enclosed space":
                    // Stop menu music completely
                    if (_ambientSource != null) _ambientSource.Stop();
                    if (_windSource != null) _windSource.Stop();

                    AudioClip roomClip = Resources.Load<AudioClip>("Audio/room_ventilation");
                    if (roomClip != null && _scenarioSource != null)
                    {
                        _scenarioSource.clip = roomClip;
                        _scenarioSource.volume = 0.50f * amb;
                        _scenarioSource.Play();
                    }
                    if (_reverbZone != null) _reverbZone.reverbPreset = AudioReverbPreset.Room;
                    _targetCutoff = 22000f;
                    SilenceRogueAudioSources();
                    break;

                case "aquaphobia":
                case "rising water":
                    // Stop menu music completely
                    if (_ambientSource != null) _ambientSource.Stop();
                    if (_windSource != null) _windSource.Stop();

                    AudioClip waveClip = Resources.Load<AudioClip>("Audio/ocean_waves");
                    if (waveClip != null && _scenarioSource != null)
                    {
                        _scenarioSource.clip = waveClip;
                        _scenarioSource.volume = 0.50f * amb;
                        _scenarioSource.Play();
                    }
                    if (_reverbZone != null) _reverbZone.reverbPreset = AudioReverbPreset.Plain;
                    _targetCutoff = 22000f;
                    SilenceRogueAudioSources();
                    break;

                case "acrophobia":
                case "high elevation":
                    // Stop menu music completely
                    if (_ambientSource != null) _ambientSource.Stop();

                    AudioClip cityClip = Resources.Load<AudioClip>("Audio/city_ambient");
                    if (cityClip == null) cityClip = _cityProceduralFallback;

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
                    SilenceRogueAudioSources();
                    break;

                default: // Menu / Portal Foyer
                    // Stop ALL session sounds immediately and clear clips
                    if (_scenarioSource != null)
                    {
                        _scenarioSource.Stop();
                        _scenarioSource.clip = null;
                    }
                    if (_windSource != null)
                    {
                        _windSource.Stop();
                        _windSource.clip = null;
                    }

                    // Resume clean Menu meditation music
                    if (_ambientSource != null)
                    {
                        _ambientSource.volume = amb * 0.45f;
                        if (!_ambientSource.isPlaying)
                        {
                            _ambientSource.Play();
                        }
                    }

                    if (_reverbZone != null) _reverbZone.reverbPreset = AudioReverbPreset.Livingroom;
                    _targetCutoff = 22000f;
                    SilenceRogueAudioSources();
                    break;
            }

            UpdateScenarioVolumes();
        }

        /// <summary>
        /// Modulates acoustic confinement as walls contract inward in Claustrophobia.
        /// </summary>
        public void SetRoomProximity(float normalizedProximity)
        {
            _roomProximity = Mathf.Clamp01(normalizedProximity);
            if (_currentScenario.ToLowerInvariant().Contains("claus") || _currentScenario.ToLowerInvariant().Contains("enclosed"))
            {
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
                if (_ambientSource != null) _ambientSource.Stop();
            }
            else if (_currentScenario.ToLowerInvariant() == "menu")
            {
                if (_scenarioSource != null) _scenarioSource.Stop();
                if (_windSource != null) _windSource.Stop();
                if (_ambientSource != null) _ambientSource.volume = amb * 0.45f;
            }
            else
            {
                // Active session (Claustrophobia or Aquaphobia)
                if (_ambientSource != null) _ambientSource.Stop();
                if (_scenarioSource != null)
                {
                    _scenarioSource.volume = 0.50f * amb;
                }
            }
        }

        public void PlayFeedbackClick()
        {
            if (_feedbackSource == null) return;
            if (_chimeClip == null) _chimeClip = CreateChimeFeedbackClip();
            _feedbackSource.PlayOneShot(_chimeClip, 0.65f);
        }

        public void PlayHoverTick()
        {
            if (_feedbackSource == null) return;
            if (_hoverTickClip == null) _hoverTickClip = CreateHoverTickClip();
            _feedbackSource.PlayOneShot(_hoverTickClip, 0.35f);
        }

        public void PlayWallCreak(float volume = 0.4f)
        {
            if (_feedbackSource == null) return;
            if (_wallCreakClip == null) _wallCreakClip = CreateWallCreakClip();
            _feedbackSource.PlayOneShot(_wallCreakClip, Mathf.Clamp01(volume));
        }

        public void PlayWaterLap(float volume = 0.45f)
        {
            if (_feedbackSource == null) return;
            if (_waterLapClip == null) _waterLapClip = CreateWaterLapClip();
            _feedbackSource.PlayOneShot(_waterLapClip, Mathf.Clamp01(volume));
        }

        public static void TriggerCaption(string text, float duration = 3.5f)
        {
            if (TherapySettingsManager.Instance != null && !TherapySettingsManager.Instance.subtitlesEnabled)
                return;

            OnCaptionRequested?.Invoke(text, duration);
        }

        private AudioClip CreateHarmonicAmbientClip(float durationSeconds)
        {
            int sampleRate = 44100;
            int totalSamples = Mathf.RoundToInt(durationSeconds * sampleRate);
            float[] samples = new float[totalSamples];

            float freq1 = 216.0f;
            float freq2 = 432.0f;
            float freq3 = 648.0f;

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float wave1 = Mathf.Sin(2f * Mathf.PI * freq1 * t) * 0.35f;
                float wave2 = Mathf.Sin(2f * Mathf.PI * freq2 * t) * 0.25f;
                float wave3 = Mathf.Sin(2f * Mathf.PI * freq3 * t) * 0.10f;
                float swell = 0.85f + 0.15f * Mathf.Sin(2f * Mathf.PI * (1f / durationSeconds) * t);
                samples[i] = (wave1 + wave2 + wave3) * swell * 0.35f;
            }

            AudioClip clip = AudioClip.Create("HarmonicAmbientBed", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateChimeFeedbackClip()
        {
            int sampleRate = 44100;
            int samplesCount = Mathf.RoundToInt(0.045f * sampleRate);
            float[] samples = new float[samplesCount];

            float freq = 864f;
            for (int i = 0; i < samplesCount; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Exp(-t * 90f);
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.4f;
            }

            AudioClip clip = AudioClip.Create("ChimeClick", samplesCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

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

        private AudioClip CreateWallCreakClip()
        {
            int sampleRate = 44100;
            int samplesCount = Mathf.RoundToInt(0.22f * sampleRate);
            float[] samples = new float[samplesCount];

            for (int i = 0; i < samplesCount; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Sin(Mathf.PI * (t / 0.22f));
                float f1 = Mathf.Sin(2f * Mathf.PI * 58f * t);
                float f2 = Mathf.Sin(2f * Mathf.PI * 116f * t) * 0.5f;
                float f3 = Mathf.Sin(2f * Mathf.PI * 174f * t) * 0.25f;
                samples[i] = (f1 + f2 + f3) * env * 0.35f;
            }

            AudioClip clip = AudioClip.Create("WallCreak", samplesCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateWaterLapClip()
        {
            int sampleRate = 44100;
            int samplesCount = Mathf.RoundToInt(0.35f * sampleRate);
            float[] samples = new float[samplesCount];

            System.Random rand = new System.Random(42);
            float b0 = 0, b1 = 0, b2 = 0;

            for (int i = 0; i < samplesCount; i++)
            {
                float t = (float)i / sampleRate;
                float white = (float)(rand.NextDouble() * 2.0 - 1.0);
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

        private AudioClip CreateCityAmbientClip(float durationSeconds)
        {
            int sampleRate = 44100;
            int totalSamples = Mathf.RoundToInt(durationSeconds * sampleRate);
            float[] samples = new float[totalSamples];

            System.Random rand = new System.Random(1337);
            float lowPass = 0f;

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float white = (float)(rand.NextDouble() * 2.0 - 1.0);

                // Heavily low-passed distant urban hum (rumble under 80Hz)
                lowPass = lowPass * 0.985f + white * 0.015f;
                float rumble = Mathf.Sin(2f * Mathf.PI * 45f * t) * 0.15f + Mathf.Sin(2f * Mathf.PI * 62f * t) * 0.08f;

                samples[i] = (lowPass * 2.2f + rumble) * 0.30f;
            }

            AudioClip clip = AudioClip.Create("CityAmbientRumble", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
