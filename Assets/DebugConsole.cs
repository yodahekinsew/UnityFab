using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DebugConsole : MonoBehaviour
{
    public TextMeshProUGUI consoleText;
    public ScrollRect consoleScrollRect;
    public Color logColor;
    public Color errorColor;
    private string log = "";
    // private string output;
    private string stack;

    public void Clear()
    {
        log = "";
        // output = "";
        consoleText.text = "";
        StartCoroutine(ScrollToBottom());
    }

    void OnEnable()
    {
        Application.logMessageReceived += WriteToConsole;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= WriteToConsole;
    }

    public void WriteToConsole(string logString, string stackTrace, LogType type)
    {
        // Switch colors based on log type
        string hexColorString = "<#";
        if (type == LogType.Error) hexColorString += ColorUtility.ToHtmlStringRGB(errorColor);
        else hexColorString += ColorUtility.ToHtmlStringRGB(logColor);
        hexColorString += ">";

        log += "\n" + hexColorString + ">> " + logString;

        // output = log + "\n\n";
        consoleText.text = log;
        StartCoroutine(ScrollToBottom());
    }

    IEnumerator ScrollToBottom()
    {
        yield return null;
        consoleScrollRect.normalizedPosition = new Vector2(0, 0);
    }
}

