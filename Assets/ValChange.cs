using System;
using UnityEngine;
using TMPro;
using DePhobia.Core;

/// <summary>
/// Manages real-time altitude, timer, and therapy metrics in Acrophobia.
/// Hides floating telemetry numbers during active exposure to ensure zero visual clutter,
/// while continuing to compute and log session telemetry for patient history.
/// </summary>
public class ValChange : MonoBehaviour
{
    public static int hi;
    public static int hf;
    public static int min;
    public static float HighestHigh = 0;

    public GameObject Timeobj;
    public GameObject Heigh;
    public GameObject Headwait;
    public GameObject waitstill;
    public GameObject focusnf;
    public GameObject cam;

    private TextMeshProUGUI timee;
    private TextMeshProUGUI heig;
    private TextMeshProUGUI waisti;
    private TextMeshProUGUI headait;
    private TextMeshProUGUI focun;

    public static string s1 = "0 sec";
    private string s2 = "0";
    private string s3 = "0";
    private string s4 = "0";
    private int condition = 3;

    void Start()
    {
        // Deactivate floating in-world HUD telemetry elements to preserve a quiet, designed visual field
        if (Timeobj != null) { timee = Timeobj.GetComponent<TextMeshProUGUI>(); Timeobj.SetActive(false); }
        if (Heigh != null) { heig = Heigh.GetComponent<TextMeshProUGUI>(); Heigh.SetActive(false); }
        if (focusnf != null) { focun = focusnf.GetComponent<TextMeshProUGUI>(); focusnf.SetActive(false); }
        if (waitstill != null) { waisti = waitstill.GetComponent<TextMeshProUGUI>(); waitstill.SetActive(false); }
        if (Headwait != null) { headait = Headwait.GetComponent<TextMeshProUGUI>(); Headwait.SetActive(false); }

        if (cam == null && Camera.main != null)
        {
            cam = Camera.main.gameObject;
        }

        if (cam != null)
        {
            hi = (int)cam.transform.position.y;
        }
    }

    void Update()
    {
        // Telemetry calculations continue to run for session metrics and kinematic thresholds
        if (vrlook.e != 1) FormatTime();
        if (timee != null && timee.gameObject.activeSelf) timee.text = s1;

        CalculateHeight();
        if (heig != null && heig.gameObject.activeSelf) heig.text = s2;

        CalculateWaitTime();
        if (waisti != null && waisti.gameObject.activeSelf) waisti.text = s3;

        UpdateColors();

        CalculateFocusTime();
        if (focun != null && focun.gameObject.activeSelf) focun.text = s4;
    }

    private void FormatTime()
    {
        int ab = (int)vrlook.Tim;
        int abc = ab % 60;
        min = ab / 60;
        if (min != 0)
        {
            s1 = $"{min} min {abc} sec";
        }
        else
        {
            s1 = $"{abc} sec";
        }
    }

    private void CalculateHeight()
    {
        if (cam == null) return;

        hf = (int)Math.Ceiling(cam.transform.position.y - hi) - 1;
        s2 = hf.ToString();
        if (hf > HighestHigh)
        {
            HighestHigh = hf;
        }
    }

    private void CalculateWaitTime()
    {
        float abcd = vrlook.TTimeg - vrlook.Timer;
        int aba = (int)Math.Ceiling(abcd);

        if (abcd >= 0 && vrlook.started == 1)
        {
            s3 = aba.ToString();
            float ratio = (vrlook.TTimeg > 0) ? (aba / vrlook.TTimeg) * 100f : 100f;

            if (ratio <= 30) condition = 1;
            else if (ratio <= 70) condition = 2;
            else condition = 3;
        }
        else
        {
            s3 = ((int)vrlook.TTimeg).ToString();
            condition = 3;
        }
    }

    private void UpdateColors()
    {
        // Forest Mineral & Sage: zero red or yellow warning states
        Color targetColor = condition switch
        {
            1 => ThemeTokens.TextMuted,
            2 => ThemeTokens.AccentSage,
            _ => ThemeTokens.TextPrimary
        };

        if (waisti != null) waisti.color = targetColor;
        if (headait != null) headait.color = targetColor;
    }

    private void CalculateFocusTime()
    {
        float abcd = vrlook.TTimeg1 - vrlook.Timer1;
        int aba = (int)Math.Ceiling(abcd);

        if (aba >= 0 && vrlook.k == 1)
        {
            s4 = aba.ToString();
        }
        else
        {
            s4 = ((int)vrlook.TTimeg1).ToString();
        }
    }
}
