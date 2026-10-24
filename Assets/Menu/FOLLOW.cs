using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Handles social external URLs and review feedback emails.
/// Migrated from obsolete WWW.EscapeURL to UnityWebRequest.EscapeURL.
/// </summary>
public class FOLLOW : MonoBehaviour
{
    public void GITHUB()
    {
        Application.OpenURL("http://www.github.com/manipta");
    }

    public void INSTA()
    {
        Application.OpenURL("http://www.instagram.com/wga_developer/");
    }

    public void Gmail()
    {
        string email = "manigarg2002@gmail.com";
        string subject = MyEscapeURL("Used Your App and Wanna Review it :)");
        string body = MyEscapeURL("AppName: DePhobia\nName:\nAddress:\n");
        Application.OpenURL("mailto:" + email + "?subject=" + subject + "&body=" + body);
    }

    private string MyEscapeURL(string url)
    {
        return UnityWebRequest.EscapeURL(url).Replace("+", "%20");
    }
}
