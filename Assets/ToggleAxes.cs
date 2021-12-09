using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleAxes : MonoBehaviour
{
    public GameObject axes;

    public void Toggle()
    {
        axes.SetActive(!axes.active);
    }
}
