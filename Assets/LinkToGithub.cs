using UnityEngine;
using System.Runtime.InteropServices;

public class LinkToGithub : MonoBehaviour
{
    public void OpenLink()
    {
#if !UNITY_EDITOR
        openWindow("http://unity3d.com");
#endif
    }

    [DllImport("__Internal")]
    private static extern void openWindow(string url);
}
