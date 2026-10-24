using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DePhobia.Core;
using DePhobia.VR;

/// <summary>
/// Controller for DePhobia Main Menu.
/// Enforces Direction C design system, applies TONE_OF_VOICE.md across all UI labels,
/// manages session history display, comfort toggles, and safe scene transitions.
/// </summary>
public class mainmenu : MonoBehaviour
{
    public GameObject a;
    public GameObject b;
    public GameObject c;
    public static int flag = 0;

    public GameObject tex;
    private TextMeshProUGUI t;

    private void Start()
    {
        if (tex != null)
        {
            t = tex.GetComponent<TextMeshProUGUI>();
        }

        // Ensure Canvas is positioned comfortably in 3D World Space for VR
        var canvas = FindObjectOfType<Canvas>();
        if (canvas != null && canvas.GetComponent<VRWorldSpaceCanvasHelper>() == null)
        {
            canvas.gameObject.AddComponent<VRWorldSpaceCanvasHelper>();
        }

        // Apply TONE_OF_VOICE.md and hide unauthorized external links
        ApplyToneOfVoiceAndPrivacyRules();

        // Soften or mute any legacy upbeat ukulele audio
        var audioSources = FindObjectsOfType<AudioSource>();
        foreach (var src in audioSources)
        {
            if (src.clip != null && src.clip.name.ToLowerInvariant().Contains("bensound"))
            {
                src.volume = 0.2f;
            }
        }

        // Fade in main menu gently
        VRFadeManager.Instance.FadeIn(0.6f);
    }

    private void ApplyToneOfVoiceAndPrivacyRules()
    {
        // 1. Hide external social links to protect patient privacy (FOLLOW.cs)
        var followContainer = GameObject.Find("FOLLOWOPT");
        if (followContainer != null)
        {
            followContainer.SetActive(false);
        }

        var followBtn = GameObject.Find("Follow");
        if (followBtn != null)
        {
            followBtn.SetActive(false);
        }

        // 2. Iterate through all TextMeshPro labels and clean up misspellings and tone
        var tmpLabels = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
        foreach (var label in tmpLabels)
        {
            if (label == null || string.IsNullOrEmpty(label.text)) continue;

            string original = label.text.Trim();

            if (original.Contains("DePhobia"))
            {
                label.text = "Kanso VR";
            }
            else if (original.Contains("CLAUSTROPHOBHIA") || original.Contains("cLAUS"))
            {
                label.text = "Claustrophobia";
            }
            else if (original == "AQUAPHOBIA")
            {
                label.text = "Aquaphobia";
            }
            else if (original == "ACROPHOBIA")
            {
                label.text = "Acrophobia";
            }
            else if (original == "PERSON STATUS")
            {
                label.text = "Session History";
            }
            else if (original == "STATUS")
            {
                label.text = "History";
            }
            else if (original == "PLAY")
            {
                label.text = "Begin";
            }
            else if (original == "SETTINGS")
            {
                label.text = "Settings";
            }
            else if (original == "BACK")
            {
                label.text = "Back";
            }
            else if (original == "WATER RISE RATE")
            {
                label.text = "Water Rise Speed";
            }
            else if (original == "WATER RISE AFTER(sec)")
            {
                label.text = "Rise Interval (sec)";
            }
            else if (original == "WATER RISE DURATION(sec)")
            {
                label.text = "Rise Duration (sec)";
            }
            else if (original == "WALL MOVE DURATION(sec)")
            {
                label.text = "Move Duration (sec)";
            }
            else if (original == "STILL FOCUS TIME")
            {
                label.text = "Focus Hold Duration (sec)";
            }
            else if (original == "SOUND ENABLE/DISABLE")
            {
                label.text = "Sound On / Off";
            }
            else if (original.Contains("Thank You For Choosing Us"))
            {
                label.text = "Closing Session...";
            }

            // Direction C Typography Styling
            if (label.color == Color.white || label.color == Color.black)
            {
                label.color = new Color(0.957f, 0.953f, 0.973f, 1f); // TextPrimary #F4F3F8
            }
        }
    }

    public void PlayClaus()
    {
        TherapyAudioManager.Instance.PlayFeedbackClick();
        VRFadeManager.Instance.FadeAndLoadScene(1, 0.5f);
    }

    public void PlayAqua()
    {
        TherapyAudioManager.Instance.PlayFeedbackClick();
        VRFadeManager.Instance.FadeAndLoadScene(2, 0.5f);
    }

    public void PlayAcro()
    {
        TherapyAudioManager.Instance.PlayFeedbackClick();
        VRFadeManager.Instance.FadeAndLoadScene(3, 0.5f);
    }

    public void SelectGentlePreset(string phobiaType)
    {
        TherapySettingsManager.Instance.ApplyPreset(phobiaType, "Gentle");
        TherapyAudioManager.Instance.PlayFeedbackClick();
    }

    public void SelectStandardPreset(string phobiaType)
    {
        TherapySettingsManager.Instance.ApplyPreset(phobiaType, "Standard");
        TherapyAudioManager.Instance.PlayFeedbackClick();
    }

    public void SelectIntensePreset(string phobiaType)
    {
        TherapySettingsManager.Instance.ApplyPreset(phobiaType, "Intense");
        TherapyAudioManager.Instance.PlayFeedbackClick();
    }

    public void ToggleSnapTurn()
    {
        var sm = TherapySettingsManager.Instance;
        sm.SetSnapTurn(!sm.snapTurn);
        TherapyAudioManager.Instance.PlayFeedbackClick();
    }

    public void ToggleComfortVignette()
    {
        var sm = TherapySettingsManager.Instance;
        sm.SetComfortVignette(!sm.comfortVignetteEnabled);
        TherapyAudioManager.Instance.PlayFeedbackClick();
    }

    public void ToggleSeatedMode()
    {
        var sm = TherapySettingsManager.Instance;
        sm.SetSeatedMode(!sm.seatedMode);
        TherapyAudioManager.Instance.PlayFeedbackClick();
    }

    public void ToggleSubtitles()
    {
        var sm = TherapySettingsManager.Instance;
        sm.SetSubtitlesEnabled(!sm.subtitlesEnabled);
        TherapyAudioManager.Instance.PlayFeedbackClick();
    }

    public void Quit()
    {
        TherapyAudioManager.Instance.PlayFeedbackClick();

        if (b != null) b.SetActive(true);
        if (a != null) a.SetActive(false);
        if (c != null) c.SetActive(true);

        flag = 1;
        StartCoroutine(QuitRoutine(1.2f));
    }

    private IEnumerator QuitRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("[DePhobia] Application quit requested safely.");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void CLAUSSTAT()
    {
        TherapyAudioManager.Instance.PlayFeedbackClick();
        EnsureTextComponent();
        if (t == null) return;

        string history = PatientSessionManager.Instance.GetFormattedHistoryForUI("Claustrophobia");
        t.text = history;
    }

    public void AQUASTAT()
    {
        TherapyAudioManager.Instance.PlayFeedbackClick();
        EnsureTextComponent();
        if (t == null) return;

        string history = PatientSessionManager.Instance.GetFormattedHistoryForUI("Aquaphobia");
        t.text = history;
    }

    public void ACROSTAT()
    {
        TherapyAudioManager.Instance.PlayFeedbackClick();
        EnsureTextComponent();
        if (t == null) return;

        string history = PatientSessionManager.Instance.GetFormattedHistoryForUI("Acrophobia");
        t.text = history;
    }

    private void EnsureTextComponent()
    {
        if (t == null && tex != null)
        {
            t = tex.GetComponent<TextMeshProUGUI>();
        }
    }
}
