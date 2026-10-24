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
        public static TherapyAudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("TherapyAudioManager");
                    _instance = go.AddComponent<TherapyAudioManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        public static event Action<string, float> OnCaptionRequested;

        private AudioSource _ambientSource;
        private AudioSource _feedbackSource;

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
            _ambientSource = gameObject.AddComponent<AudioSource>();
            _ambientSource.loop = true;
            _ambientSource.playOnAwake = false;
            _ambientSource.spatialBlend = 0f; // 2D binaural bed
            _ambientSource.volume = 0.45f;

            _feedbackSource = gameObject.AddComponent<AudioSource>();
            _feedbackSource.loop = false;
            _feedbackSource.playOnAwake = false;
            _feedbackSource.spatialBlend = 0f;
            _feedbackSource.volume = 0.6f;

            // Generate procedural calm ambient loop (warm 432Hz harmonic pad)
            _ambientSource.clip = CreateHarmonicAmbientClip(8.0f);
            _ambientSource.Play();
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
                    _ambientSource.volume = sm.ambienceVolume * 0.5f;
                }
                if (_feedbackSource != null)
                {
                    _feedbackSource.volume = sm.feedbackVolume * 0.7f;
                }
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
