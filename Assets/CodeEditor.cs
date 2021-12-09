using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CodeEditor : MonoBehaviour
{
    public DebugConsole m_DebugConsole;
    public TextAsset m_SceneFile;

    [Header("DSL Parser")]
    public UnityDSL m_DSLParser;
    public TextMeshProUGUI m_InputText;

    [Header("Camera Controller")]
    public CameraController m_CameraController;

    [Header("Code Editor")]
    public TextMeshProUGUI m_EditorToggle;
    public RectTransform m_EditorRect;
    public float m_EditorTransitionDuration;
    [Space(5)]
    public Vector2 m_OpenAnchorMin;
    public Vector2 m_OpenAnchorMax;
    [Space(5)]
    public Vector2 m_CloseAnchorMin;
    public Vector2 m_CloseAnchorMax;
    private bool showing = false;

    private void Awake()
    {
        // Force editor to be showing
        m_EditorRect.anchorMax = m_OpenAnchorMax;
        m_EditorRect.anchorMin = m_OpenAnchorMin;
        m_CameraController.m_OriginOffset = 1;
        m_EditorToggle.text = "<";
        showing = true;
        ParseScene(); // Parse the scene with starting text
    }

    public void ParseScene()
    {
        m_DebugConsole.Clear();
        string dslText = m_InputText.text;
        m_DSLParser.ParseDSL(dslText);
    }

    public void ClearScene()
    {
        m_DebugConsole.Clear();
        m_DSLParser.ClearDSL();
    }

    public void ExportScene()
    {
        WebDownloadHelper.InitiateDownload("UnityFabScene.txt", m_InputText.text);
    }

    public void ToggleCodeEditor()
    {
        if (showing) HideCodeEditor();
        else ShowCodeEditor();
    }
    private void ShowCodeEditor()
    {
        if (showing) return;
        showing = true;
        // DOTween.To(() => m_CameraController.m_OriginOffset, x => m_CameraController.m_OriginOffset = x,
        //     1, m_EditorTransitionDuration);
        // m_EditorRect.DOAnchorMax(m_OpenAnchorMax, m_EditorTransitionDuration);
        // m_EditorRect.DOAnchorMin(m_OpenAnchorMin, m_EditorTransitionDuration);
        m_EditorToggle.text = "<";
        StartCoroutine(ShowingCodeEditor());
    }
    private void HideCodeEditor()
    {
        if (!showing) return;
        showing = false;
        // DOTween.To(() => m_CameraController.m_OriginOffset, x => m_CameraController.m_OriginOffset = x,
        //     0, m_EditorTransitionDuration);
        // m_EditorRect.DOAnchorMax(m_CloseAnchorMax, m_EditorTransitionDuration);
        // m_EditorRect.DOAnchorMin(m_CloseAnchorMin, m_EditorTransitionDuration);
        m_EditorToggle.text = ">";
        StartCoroutine(HidingCodeEditor());
    }

    IEnumerator ShowingCodeEditor()
    {
        float t = 0;
        while (t < 1)
        {
            t = Mathf.Clamp01(t + Time.deltaTime / m_EditorTransitionDuration);
            m_EditorRect.anchorMax = Vector2.Lerp(m_CloseAnchorMax, m_OpenAnchorMax, t);
            m_EditorRect.anchorMin = Vector2.Lerp(m_CloseAnchorMin, m_OpenAnchorMin, t);
            m_CameraController.m_OriginOffset = t;
            yield return null;
        }
        m_EditorRect.anchorMax = m_OpenAnchorMax;
        m_EditorRect.anchorMin = m_OpenAnchorMin;
        m_CameraController.m_OriginOffset = 1;
    }

    IEnumerator HidingCodeEditor()
    {
        float t = 0;
        while (t < 1)
        {
            t = Mathf.Clamp01(t + Time.deltaTime / m_EditorTransitionDuration);
            m_EditorRect.anchorMax = Vector2.Lerp(m_OpenAnchorMax, m_CloseAnchorMax, t);
            m_EditorRect.anchorMin = Vector2.Lerp(m_OpenAnchorMin, m_CloseAnchorMin, t);
            m_CameraController.m_OriginOffset = 1 - t;
            yield return null;
        }
        m_EditorRect.anchorMax = m_CloseAnchorMax;
        m_EditorRect.anchorMin = m_CloseAnchorMin;
        m_CameraController.m_OriginOffset = 0;
    }
}

public class WebDownloadHelper
{
    // Source: http://stackoverflow.com/a/27284736/1607924
    static string scriptTemplate = @"
             var link = document.createElement(""a"");
             link.download = '{0}';
             link.href = 'data:application/octet-stream;charset=utf-8;base64,{1}';
             document.body.appendChild(link);
             link.click();
             document.body.removeChild(link);
             delete link;
         ";

    public static void InitiateDownload(string aName, byte[] aData)
    {
        string base64 = System.Convert.ToBase64String(aData);
        string script = string.Format(scriptTemplate, aName, base64);
        Application.ExternalEval(script);
    }

    public static void InitiateDownload(string aName, string aData)
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(aData);
        InitiateDownload(aName, data);
    }
}
