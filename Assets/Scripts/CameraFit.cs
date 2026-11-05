using UnityEngine;

/// <summary>Keeps the entire level visible in wide and narrow game windows.</summary>
[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class CameraFit : MonoBehaviour
{
    [SerializeField] private float minimumHalfHeight = 2.8f;
    [SerializeField] private float minimumHalfWidth = 4.9f;
    private Camera targetCamera;

    private void OnEnable() => targetCamera = GetComponent<Camera>();
    private void LateUpdate()
    {
        if (targetCamera == null) targetCamera = GetComponent<Camera>();
        if (targetCamera.aspect > 0f)
            targetCamera.orthographicSize = Mathf.Max(minimumHalfHeight, minimumHalfWidth / targetCamera.aspect);
    }
}
