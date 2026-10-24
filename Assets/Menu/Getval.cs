using UnityEngine;
using TMPro;
using DePhobia.Core;

/// <summary>
/// Event-driven UI controller for Claustrophobia configuration inputs.
/// Replaces per-frame float.Parse in Update() with event callbacks and safe validation.
/// </summary>
public class Getval : MonoBehaviour
{
    public static float speed => TherapySettingsManager.Instance.clausSpeed;
    public static float wallmovetime => TherapySettingsManager.Instance.clausMoveAfter;
    public static float wallmoveduration => TherapySettingsManager.Instance.clausMoveDuration;

    public GameObject spee;
    public GameObject wallmovetim;
    public GameObject wallmoveduratio;

    private void Start()
    {
        var sm = TherapySettingsManager.Instance;

        if (spee != null)
        {
            var input = spee.GetComponent<TMP_InputField>();
            if (input != null)
            {
                // In original app, speed was displayed as (speed * 100)
                input.text = (sm.clausSpeed * 100f).ToString("F0");
                input.onEndEdit.AddListener(val => sm.SetClausSpeedFromInput(val));
            }
        }

        if (wallmovetim != null)
        {
            var input = wallmovetim.GetComponent<TMP_InputField>();
            if (input != null)
            {
                input.text = sm.clausMoveAfter.ToString("F1");
                input.onEndEdit.AddListener(val => sm.SetClausMoveAfterFromInput(val));
            }
        }

        if (wallmoveduratio != null)
        {
            var input = wallmoveduratio.GetComponent<TMP_InputField>();
            if (input != null)
            {
                input.text = sm.clausMoveDuration.ToString("F1");
                input.onEndEdit.AddListener(val => sm.SetClausMoveDurationFromInput(val));
            }
        }
    }
}
