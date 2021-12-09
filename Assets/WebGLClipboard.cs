using System;
using UnityEngine;
using System.Runtime.InteropServices;
using AOT;

public class WebGLClipboard
{

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    public static extern void ClipboardReader(string gObj, string vName);
 
    [DllImport("__Internal")]
    public static extern void ClipboardWriter(string newClipText);
#endif

    public static void WriteToClipboard(string newClipText)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        ClipboardWriter(newClipText);
#endif
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [MonoPInvokeCallback(typeof(Action))]
#endif
    public static void ReadFromClipboard(string gameObjectName, string handler)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        ClipboardReader(gameObjectName, handler);  
#endif
    }
}
