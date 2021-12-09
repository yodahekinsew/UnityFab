using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SyncFontSizes : MonoBehaviour
{
    public List<TextMeshProUGUI> buttonTexts;
    public TextMeshProUGUI editorText;
    public TextMeshProUGUI placeholderText;

    void Update()
    {
        float minFontSize = Mathf.Infinity;
        foreach (TextMeshProUGUI text in buttonTexts)
        {
            if (text.fontSize < minFontSize) minFontSize = text.fontSize;
        }
        foreach (TextMeshProUGUI text in buttonTexts)
        {
            text.enableAutoSizing = false;
            text.fontSize = minFontSize;
        }
        editorText.fontSize = Mathf.Max(minFontSize / 1.5f, 32);
        placeholderText.fontSize = Mathf.Max(minFontSize / 1.5f, 32);
    }
}
