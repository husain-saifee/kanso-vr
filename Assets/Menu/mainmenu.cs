using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using DePhobia.Core;
using DePhobia.VR;

/// <summary>
/// Controller for DePhobia Main Menu.
/// Routes scene transitions with VR fades, displays patient stats, and handles settings.
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

        // Ensure Canvas is in comfortable 3D World Space for VR
        var canvas = FindObjectOfType<Canvas>();
        if (canvas != null && canvas.GetComponent<VRWorldSpaceCanvasHelper>() == null)
        {
            canvas.gameObject.AddComponent<VRWorldSpaceCanvasHelper>();
        }

        // Fade in main menu
        VRFadeManager.Instance.FadeIn(0.5f);
    }

    public void PlayClaus()
    {
        VRFadeManager.Instance.FadeAndLoadScene(1, 0.4f);
    }

    public void PlayAqua()
    {
        VRFadeManager.Instance.FadeAndLoadScene(2, 0.4f);
    }

    public void PlayAcro()
    {
        VRFadeManager.Instance.FadeAndLoadScene(3, 0.4f);
    }

    public void Quit()
    {
        if (b != null) b.SetActive(true);
        if (a != null) a.SetActive(false);
        if (c != null) c.SetActive(true);

        flag = 1;
        StartCoroutine(QuitRoutine(1.5f));
    }

    private IEnumerator QuitRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("[DePhobia] Application quit requested.");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void CLAUSSTAT()
    {
        EnsureTextComponent();
        if (t == null) return;

        string history = PatientSessionManager.Instance.GetFormattedHistoryForUI("Claustrophobia");
        t.text = history;
    }

    public void AQUASTAT()
    {
        EnsureTextComponent();
        if (t == null) return;

        string history = PatientSessionManager.Instance.GetFormattedHistoryForUI("Aquaphobia");
        t.text = history;
    }

    public void ACROSTAT()
    {
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
