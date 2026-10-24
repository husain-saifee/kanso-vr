using System;
using UnityEngine;
using TMPro;

/// <summary>
/// Manages real-time altitude, timer, and therapy condition HUD in Acrophobia.
/// Optimized to cache components and eliminate null-reference paths.
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
        if (Timeobj != null) timee = Timeobj.GetComponent<TextMeshProUGUI>();
        if (Heigh != null) heig = Heigh.GetComponent<TextMeshProUGUI>();
        if (focusnf != null) focun = focusnf.GetComponent<TextMeshProUGUI>();
        if (waitstill != null) waisti = waitstill.GetComponent<TextMeshProUGUI>();
        if (Headwait != null) headait = Headwait.GetComponent<TextMeshProUGUI>();

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
        if (vrlook.e != 1) FormatTime();
        if (timee != null) timee.text = s1;

        CalculateHeight();
        if (heig != null) heig.text = s2;

        CalculateWaitTime();
        if (waisti != null) waisti.text = s3;

        UpdateColors();

        CalculateFocusTime();
        if (focun != null) focun.text = s4;
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
        Color targetColor = condition switch
        {
            1 => new Color32(184, 29, 19, 255), // Red - warning
            2 => new Color32(239, 183, 0, 255), // Yellow - caution
            _ => Color.white
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
