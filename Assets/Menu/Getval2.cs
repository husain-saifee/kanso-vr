using UnityEngine;
using TMPro;
using DePhobia.Core;

/// <summary>
/// Event-driven UI controller for Acrophobia configuration inputs.
/// Replaces per-frame float.Parse in Update() with event callbacks and eliminates debug spam.
/// </summary>
public class Getval2 : MonoBehaviour
{
    public static float waitingt => TherapySettingsManager.Instance.acroWaitTime;
    public static float focunotfall => TherapySettingsManager.Instance.acroStillFocusTime;
    public static string sound => TherapySettingsManager.Instance.acroSoundEnabled ? "0" : "1";

    public GameObject wspee;
    public GameObject watermovetim;
    public GameObject watermoveduratio;

    private void Start()
    {
        var sm = TherapySettingsManager.Instance;

        if (wspee != null)
        {
            var input = wspee.GetComponent<TMP_InputField>();
            if (input != null)
            {
                input.text = sm.acroWaitTime.ToString("F1");
                input.onEndEdit.AddListener(val => sm.SetAcroWaitTimeFromInput(val));
            }
        }

        if (watermovetim != null)
        {
            var input = watermovetim.GetComponent<TMP_InputField>();
            if (input != null)
            {
                input.text = sm.acroStillFocusTime.ToString("F1");
                input.onEndEdit.AddListener(val => sm.SetAcroStillFocusTimeFromInput(val));
            }
        }

        if (watermoveduratio != null)
        {
            var dropdown = watermoveduratio.GetComponent<TMP_Dropdown>();
            if (dropdown != null)
            {
                dropdown.value = sm.acroSoundEnabled ? 0 : 1;
                dropdown.onValueChanged.AddListener(idx => sm.SetAcroSoundEnabled(idx == 0));
            }
        }
    }
}
