using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DePhobia.Core
{
    [Serializable]
    public class SessionRecord
    {
        public string date;
        public string time;
        public string phobiaType; // "Claustrophobia", "Aquaphobia", "Acrophobia"
        public float elapsedSeconds;
        public string durationFormatted;
        public string parametersSummary;
        public bool completed;
        public float peakMetric; // e.g., max height reached in Acrophobia, or wall count in Claustrophobia
    }

    [Serializable]
    public class SessionHistoryDatabase
    {
        public List<SessionRecord> records = new List<SessionRecord>();
    }

    /// <summary>
    /// Tracks active therapy session metrics and manages structured session persistence
    /// alongside backwards-compatible text file exports.
    /// </summary>
    public class PatientSessionManager : MonoBehaviour
    {
        private static PatientSessionManager _instance;
        private static bool _isApplicationQuitting = false;
        public static bool HasInstance => _instance != null;

        public static PatientSessionManager Instance
        {
            get
            {
                if (_isApplicationQuitting) return null;
                if (_instance == null)
                {
                    _instance = FindObjectOfType<PatientSessionManager>();
                    if (_instance == null && !_isApplicationQuitting)
                    {
                        var go = new GameObject("PatientSessionManager");
                        _instance = go.AddComponent<PatientSessionManager>();
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
        }

        private float _sessionStartTime;
        private string _activePhobiaType = "";
        private bool _isSessionActive = false;
        private float _peakMetric = 0f;

        private SessionHistoryDatabase _database = new SessionHistoryDatabase();
        private string StatsDirectory => Path.Combine(Application.persistentDataPath, "PersonStats");
        private string JsonDatabasePath => Path.Combine(StatsDirectory, "session_history.json");

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            LoadDatabase();
        }

        public void StartSession(string phobiaType)
        {
            _activePhobiaType = phobiaType;
            _sessionStartTime = Time.time;
            _isSessionActive = true;
            _peakMetric = 0f;
            Debug.Log($"[PatientSessionManager] Started {phobiaType} therapy session at Time={_sessionStartTime}");
        }

        public void UpdatePeakMetric(float value)
        {
            if (value > _peakMetric)
            {
                _peakMetric = value;
            }
        }

        public float GetCurrentSessionDuration()
        {
            if (!_isSessionActive) return 0f;
            return Time.time - _sessionStartTime;
        }

        public string FormatDuration(float totalSeconds)
        {
            int totalSec = Mathf.FloorToInt(totalSeconds);
            int minutes = totalSec / 60;
            int seconds = totalSec % 60;

            if (minutes > 0)
                return $"{minutes}m {seconds:00}s";
            return $"{seconds}s";
        }

        public void EndAndSaveSession(bool completed = false, string extraParams = "")
        {
            if (!_isSessionActive) return;

            float duration = GetCurrentSessionDuration();
            _isSessionActive = false;

            DateTime now = DateTime.Now;
            var record = new SessionRecord
            {
                date = now.ToString("yyyy-MM-dd"),
                time = now.ToString("HH:mm:ss"),
                phobiaType = _activePhobiaType,
                elapsedSeconds = duration,
                durationFormatted = FormatDuration(duration),
                completed = completed,
                peakMetric = _peakMetric,
                parametersSummary = extraParams
            };

            _database.records.Add(record);
            SaveDatabase();
            WriteLegacyTextRecord(record);

            Debug.Log($"[PatientSessionManager] Completed {_activePhobiaType} session. Duration: {record.durationFormatted}");
        }

        private void SaveDatabase()
        {
            try
            {
                if (!Directory.Exists(StatsDirectory))
                    Directory.CreateDirectory(StatsDirectory);

                string json = JsonUtility.ToJson(_database, true);
                File.WriteAllText(JsonDatabasePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PatientSessionManager] Error saving JSON database: {ex.Message}");
            }
        }

        private void LoadDatabase()
        {
            try
            {
                if (File.Exists(JsonDatabasePath))
                {
                    string json = File.ReadAllText(JsonDatabasePath);
                    _database = JsonUtility.FromJson<SessionHistoryDatabase>(json) ?? new SessionHistoryDatabase();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PatientSessionManager] Could not load JSON database: {ex.Message}");
                _database = new SessionHistoryDatabase();
            }
        }

        public string GetFormattedHistoryForUI(string phobiaFilter = "")
        {
            if (_database.records.Count == 0)
            {
                return "No therapy session data recorded yet.\nComplete a simulation to view patient history.";
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Date       Time     Phobia        Duration   Peak Metric");
            sb.AppendLine("---------------------------------------------------------");

            int count = 0;
            for (int i = _database.records.Count - 1; i >= 0; i--)
            {
                var r = _database.records[i];
                if (!string.IsNullOrEmpty(phobiaFilter) &&
                    !string.Equals(r.phobiaType, phobiaFilter, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                sb.AppendLine($"{r.date} {r.time} {r.phobiaType,-13} {r.durationFormatted,-10} {r.peakMetric:F1}");
                count++;
                if (count >= 15) break; // Display last 15 entries cleanly
            }

            if (count == 0)
                return $"No records found for {phobiaFilter}.";

            return sb.ToString();
        }

        private void WriteLegacyTextRecord(SessionRecord r)
        {
            try
            {
                if (!Directory.Exists(StatsDirectory))
                    Directory.CreateDirectory(StatsDirectory);

                string fileName = r.phobiaType switch
                {
                    "Claustrophobia" => "ClausStats.txt",
                    "Aquaphobia" => "AquaStats.txt",
                    "Acrophobia" => "AcroStats.txt",
                    _ => "GeneralStats.txt"
                };

                string filePath = Path.Combine(StatsDirectory, fileName);
                string line = $"{r.date} {r.time}\t{r.durationFormatted}\tCompleted: {r.completed}\tPeak: {r.peakMetric:F1}\t{r.parametersSummary}\n";

                if (!File.Exists(filePath))
                {
                    File.WriteAllText(filePath, "Timestamp\tDuration\tStatus\tPeakMetric\tParams\n\n");
                }
                File.AppendAllText(filePath, line);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PatientSessionManager] Error writing legacy stats: {ex.Message}");
            }
        }
    }
}
