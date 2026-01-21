using System;
using System.Collections;
using UnityEngine;

public class PerspCam : CameraManager
{
    [Header("Orbit Settings")]
    [SerializeField] private Vector3 _target = Vector3.zero;
    [SerializeField] private float _distance = 10f;
    [SerializeField] private float _zoomSpeed = 0.1f;
    [SerializeField] private float _rotationSpeed = 0.2f;
    [SerializeField] private float _panSpeed = 0.005f;
    [SerializeField] private float _lerpSpeed = 8f;

    [Header("Pitch Clamp")]
    [SerializeField] private float _minPitch = 10f;
    [SerializeField] private float _maxPitch = 80f;

    private float _yaw = 0f;
    private float _pitch = 45f;
    private float _targetYaw;
    private float _targetPitch;
    private float _targetDistance;

    private Vector2 _lastTouchPos;

    private Camera _cam;
    
    [Header("Smooth Angle Rotate")]
    public float smoothTime = 0.4f;
    public float minHeightAboveFloor = 0.5f;
    private LayerMask floorMask = 1 << 7;
    private Coroutine frameRoutine;

    
    #region Getter and Setter

    public float GetCurrentDistance()
    {
        return _distance;
    }
    #endregion

    private void Awake()
    {
        _cam = GetComponent<Camera>();
    }
    
    private void Start()
    {
        _targetYaw = _yaw;
        _targetPitch = _pitch;
        _targetDistance = _distance;
    }

    public void SetInitialTouchPosition(Vector2 screenPos)
    {
        _lastTouchPos = screenPos;
    }

    public void RotateCamera(Vector2 screenPos)
    {
        Vector2 delta = screenPos - _lastTouchPos;

        _targetYaw += delta.x * _rotationSpeed;
        _targetPitch -= delta.y * _rotationSpeed;

        _lastTouchPos = screenPos;
    }

    public void PanCamera(Vector2 screenPos)
    {
        Vector2 delta = screenPos - _lastTouchPos;

        // Right and up directions in world space based on current camera orientation
        Vector3 right = _cam.transform.right;
        Vector3 up = _cam.transform.up;

        // Apply delta
        Vector3 panMovement = (-right * delta.x + -up * delta.y) * _panSpeed * _distance;
        _target += panMovement;

        _lastTouchPos = screenPos;
    }

    public void ZoomCamera(float delta)
    {
        _targetDistance -= delta * _zoomSpeed;
        _targetDistance = Mathf.Clamp(_targetDistance, 15f, 80f);
    }

    public void UpdateCamera()
    {
        if (_cam == null || _cam.orthographic) return;

        _yaw = Mathf.Lerp(_yaw, _targetYaw, Time.deltaTime * _lerpSpeed);
        _pitch = Mathf.Lerp(_pitch, _targetPitch, Time.deltaTime * _lerpSpeed);
        _distance = Mathf.Lerp(_distance, _targetDistance, Time.deltaTime * _lerpSpeed);

        _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);
        
        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 offset = rotation * new Vector3(0, 0, -_distance);
        _cam.transform.position = _target + offset;
        _cam.transform.LookAt(_target);
    }
    
    public void SimulateCamera()
    {
        if (_cam == null || _cam.orthographic) return;
        
        // Find the bound of Room Generated then Setting Camera According;
        
    }

    public void PanCameraByDelta(Vector2 delta)
    {
        Vector3 forward = _cam.transform.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 right = _cam.transform.right;

        Vector3 panMovement = (-right * delta.x + -forward * delta.y) * _panSpeed * _distance * Time.deltaTime;
        _target += panMovement;
    }

    #region  CAMERA FRAMING
    
    /// <summary>
    /// Frames a 3D object AND its world-space UI so both are fully visible.
    /// </summary>
    public void FrameObjectWithWorldUI(GameObject target, RectTransform worldUI)
    {
        if (target == null || worldUI == null)
            return;

        if (frameRoutine != null)
            StopCoroutine(frameRoutine);

        frameRoutine = StartCoroutine(FrameRoutine(target, worldUI));
    }

    private  IEnumerator FrameRoutine(GameObject target, RectTransform worldUI)
    {
        /* ---------- 1. CALCULATE OBJECT BOUNDS ---------- */
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers)
            bounds.Encapsulate(r.bounds);

        /* ---------- 2. INCLUDE WORLD UI BOUNDS ---------- */
        Vector3[] corners = new Vector3[4];
        worldUI.GetWorldCorners(corners);
        for (int i = 0; i < 4; i++)
            bounds.Encapsulate(corners[i]);

        /* ---------- 3. CENTER BIAS (UI NEEDS MORE SPACE ABOVE) ---------- */
        Vector3 focusPoint = bounds.center;
        focusPoint += Vector3.up * bounds.extents.y * 0.25f;

        /* ---------- 4. CAMERA DISTANCE BASED ON FOV ---------- */
        float radius = bounds.extents.magnitude;
        float fovRad = _cam.fieldOfView * Mathf.Deg2Rad;
        float distance = radius / Mathf.Sin(fovRad / 2f);
        distance *= 1.1f; // padding

        /* ---------- 5. NICE VIEW ANGLE (EDITOR-LIKE) ---------- */
        Vector3 viewDir = new Vector3(1f, 1.2f, -1f).normalized;
        Vector3 targetPos = focusPoint + viewDir * distance;

        /* ---------- 6. FLOOR SAFETY ---------- */
        if (Physics.Raycast(targetPos, Vector3.down, out RaycastHit hit, 10f, floorMask))
        {
            targetPos.y = Mathf.Max(targetPos.y, hit.point.y + minHeightAboveFloor);
        }

        /* ---------- 7. SMOOTH MOVE + ROTATE ---------- */
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / smoothTime;

            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(
                startRot,
                Quaternion.LookRotation(focusPoint - transform.position),
                t
            );

            yield return null;
        }
    }
    
    public void ReframeAfterScale(GameObject target, RectTransform worldUI)
    {
        if (target == null || worldUI == null)
            return;

        // 1️⃣ Force bounds update (important after scaling)
        Canvas.ForceUpdateCanvases();
        Physics.SyncTransforms();

        // 2️⃣ Reposition world UI above the object
        Vector3 topPoint = GetTopMostWorldPoint(target);
        float padding = (topPoint - target.transform.position).magnitude * 0.55f;
        worldUI.position = topPoint + Vector3.up * padding;

        // 3️⃣ Re-frame camera using existing logic
        FrameObjectWithWorldUI(target, worldUI);
    }
    
    Vector3 GetTopMostWorldPoint(GameObject target)
    {
        Renderer[] rs = target.GetComponentsInChildren<Renderer>();
        float maxY = float.MinValue;
        Vector3 top = target.transform.position;

        foreach (Renderer r in rs)
        {
            if (r.bounds.max.y > maxY)
            {
                maxY = r.bounds.max.y;
                top = new Vector3(r.bounds.center.x, r.bounds.max.y, r.bounds.center.z);
            }
        }

        return top;
    }
    
    #endregion // CAMERA FRAMING

}
