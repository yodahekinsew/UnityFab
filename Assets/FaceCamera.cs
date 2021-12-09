using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    Transform cameraTransform;

    void Start()
    {
        cameraTransform = Camera.main.transform;
    }


    void Update()
    {
        transform.forward = (transform.position - cameraTransform.position).normalized;
    }
}
