using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WriteSizeToShader : MonoBehaviour
{
    public RectTransform rect;
    public Image image;

    void Update()
    {
        image.material.SetFloat("_XScale", rect.rect.width / rect.rect.height);
        // image.material.SetFloat("_YScale", );
    }
}
