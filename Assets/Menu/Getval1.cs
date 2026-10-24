using UnityEngine;
using TMPro;
using DePhobia.Core;

/// <summary>
/// Event-driven UI controller for Aquaphobia configuration inputs.
/// Replaces per-frame float.Parse in Update() with event callbacks and eliminates coroutine spam.
/// </summary>
public class Getval1 : MonoBehaviour
{
    public static float watspeed => TherapySettingsManager.Instance.aquaWaterSpeed;
    public static float watermovetime => TherapySettingsManager.Instance.aquaRiseAfter;
    public static float watermoveduration => TherapySettingsManager.Instance.aquaRiseDuration;

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
                input.text = (sm.aquaWaterSpeed * 100f).ToString("F0");
                input.onEndEdit.AddListener(val => sm.SetAquaSpeedFromInput(val));
            }
        }

        if (watermovetim != null)
        {
            var input = watermovetim.GetComponent<TMP_InputField>();
            if (input != null)
            {
                input.text = sm.aquaRiseAfter.ToString("F1");
                input.onEndEdit.AddListener(val => sm.SetAquaRiseAfterFromInput(val));
            }
        }

        if (watermoveduratio != null)
        {
            var input = watermoveduratio.GetComponent<TMP_InputField>();
            if (input != null)
            {
                input.text = sm.aquaRiseDuration.ToString("F1");
                input.onEndEdit.AddListener(val => sm.SetAquaRiseDurationFromInput(val));
            }
        }
    }
}
