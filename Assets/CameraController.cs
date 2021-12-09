using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    [Header("Camera")]
    public Camera m_Camera;
    public Vector3 m_WorldOrigin;
    [Range(0, 1)] public float m_OriginOffset;

    [Header("Zoom")]
    public float m_MinZoomDepth;
    public float m_MaxZoomDepth;
    public float m_ZoomDepth;
    public float m_ZoomSensitivity;
    [Range(0, 1)] public float m_DepthSmoothTime;
    private float m_DepthChange;
    private float m_DepthChangeVelocity;

    [Header("Rotation")]
    public float m_RotationSensitivity;
    [Range(0, 1)] public float m_AngleSmoothTime;
    private Vector3 m_LastMousePosition;
    private bool m_Rotating;
    private float angleX;
    private float angleY;
    private float angleVelocityX;
    private float angleVelocityY;

    void Update()
    {
        // Translate the world origin (Helps make room for the code input)
        Vector3 centerScreenPos = m_Camera.ScreenToWorldPoint(
            new Vector3(.5f * Screen.width, .5f * Screen.height, m_ZoomDepth)
            );
        Vector3 offsetScreenPos = m_Camera.ScreenToWorldPoint(
            new Vector3((.5f - .2f * m_OriginOffset) * Screen.width, .5f * Screen.height, m_ZoomDepth)
            );
        Vector3 worldOriginTranslation = offsetScreenPos - centerScreenPos;
        Vector3 translatedWorldOrigin = m_WorldOrigin + worldOriginTranslation;

        // Handle controlling the camera rotation
        bool mouseOverUI = EventSystem.current.IsPointerOverGameObject();
        if (Input.GetMouseButtonDown(0) && !mouseOverUI)
        {
            m_Rotating = true;
            angleVelocityX = 0;
            angleVelocityY = 0;
            angleX = 0;
            angleY = 0;
            // Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
        }
        if (m_Rotating && Input.GetMouseButtonUp(0))
        {
            m_Rotating = false;
            // Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        if (m_Rotating)
        {
            // print("Rotating");
            // print(Input.GetAxis("Mouse X") + ", " + Input.GetAxis("Mouse Y"));
            float targetAngleX = Input.GetAxis("Mouse X") * m_RotationSensitivity * Time.deltaTime;
            float targetAngleY = Input.GetAxis("Mouse Y") * m_RotationSensitivity * Time.deltaTime;
            // print(targetAngleX + ", " + targetAngleY);
            angleX = Mathf.SmoothDamp(angleX, targetAngleX, ref angleVelocityX, m_AngleSmoothTime);
            angleY = Mathf.SmoothDamp(angleY, targetAngleY, ref angleVelocityY, m_AngleSmoothTime);
            // print(angleX + ", " + angleY);
            transform.forward = Quaternion.AngleAxis(angleX, Vector3.up) * transform.forward;
            transform.forward = Quaternion.AngleAxis(-angleY, Vector3.right) * transform.forward;
        }

        // Handle the camera zoom
        if (!mouseOverUI)
        {
            float targetDepthChange = Input.mouseScrollDelta.y * m_ZoomSensitivity * Time.deltaTime;
            m_DepthChange = Mathf.SmoothDamp(m_DepthChange, targetDepthChange, ref m_DepthChangeVelocity, m_DepthSmoothTime);
            m_ZoomDepth -= m_DepthChange;
            m_ZoomDepth = Mathf.Clamp(m_ZoomDepth, m_MinZoomDepth, m_MaxZoomDepth);
        }

        transform.position = translatedWorldOrigin;
        m_Camera.transform.localPosition = m_ZoomDepth * Vector3.back;
    }
}
