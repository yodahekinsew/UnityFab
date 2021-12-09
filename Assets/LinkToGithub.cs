using UnityEngine;
using System.Runtime.InteropServices;

public class LinkToGithub : MonoBehaviour
{
    public void OpenLink()
    {
#if !UNITY_EDITOR
        openWindow("https://github.com/yodahekinsew/UnityFab");
#endif
    }

    [DllImport("__Internal")]
    private static extern void openWindow(string url);
}
