using UnityEngine;
using UnityEngine.EventSystems;

public class TouchManager : MonoBehaviour
{
    private Vector2 _initialTouchPosition, _currentTouchPosition;
    private float _initialTouchTime, _currentTouchTime;

    private bool _isDragging = false;
    private float _tapThresholdTime = 0.3f;
    private float _dragThreshold = 5f;

    private void Update()
    {
        var currentSubState = GameManager.Instance.GetSubState();
        if (currentSubState == null) return;

#if UNITY_EDITOR
        // Prevent interactions through UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // --- Pinch simulation with mouse scroll wheel ---
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            currentSubState.OnPinch(scroll * 100f);
        }

        // --- Mouse drag handling ---
        if (Input.GetMouseButtonDown(0))
        {
            _initialTouchPosition = Input.mousePosition;
            _initialTouchTime = Time.time;
            _isDragging = false;
        }

        if (Input.GetMouseButton(0))
        {
            _currentTouchPosition = Input.mousePosition;

            if (!_isDragging && (_currentTouchPosition - _initialTouchPosition).magnitude > _dragThreshold)
            {
                _isDragging = true;
                Vector3 worldPos = ScreenToWorld(_initialTouchPosition);
                currentSubState.OnTouchStart(worldPos, _initialTouchPosition);
            }

            if (_isDragging)
            {
                Vector3 worldPos = ScreenToWorld(_currentTouchPosition);
                currentSubState.OnTouchHold(worldPos, _currentTouchPosition);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            _currentTouchTime = Time.time;

            if (_isDragging)
            {
                Vector3 worldPos = ScreenToWorld(Input.mousePosition);
                currentSubState.OnTouchEnd(worldPos, Input.mousePosition);
            }
            else if (IsTap())
            {
                HandleTap(Input.mousePosition);
            }
        }

#else
        if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            return;

        // --- Touch input ---
        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            Vector2 prevTouch0 = touch0.position - touch0.deltaPosition;
            Vector2 prevTouch1 = touch1.position - touch1.deltaPosition;

            float prevMagnitude = (prevTouch0 - prevTouch1).magnitude;
            float currentMagnitude = (touch0.position - touch1.position).magnitude;

            float deltaMagnitudeDiff = currentMagnitude - prevMagnitude;

            currentSubState.OnPinch(deltaMagnitudeDiff);
            return; // skip one-touch handling
        }

        if (Input.touchCount != 1) return;

        Touch touch = Input.GetTouch(0);

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            return;

        switch (touch.phase)
        {
            case TouchPhase.Began:
                _initialTouchPosition = touch.position;
                _initialTouchTime = Time.time;
                _isDragging = false;
                break;

            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                _currentTouchPosition = touch.position;

                if (!_isDragging && (_currentTouchPosition - _initialTouchPosition).magnitude > _dragThreshold)
                {
                    _isDragging = true;
                    Vector3 worldPos = ScreenToWorld(_initialTouchPosition);
                    currentSubState.OnTouchStart(worldPos, _initialTouchPosition);
                }

                if (_isDragging)
                {
                    Vector3 worldPos = ScreenToWorld(_currentTouchPosition);
                    currentSubState.OnTouchHold(worldPos, _currentTouchPosition);
                }
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                _currentTouchTime = Time.time;

                if (_isDragging)
                {
                    Vector3 worldPos = ScreenToWorld(touch.position);
                    currentSubState.OnTouchEnd(worldPos, touch.position);
                }
                else if (IsTap())
                {
                    HandleTap(touch.position);
                }
                break;
        }
#endif
    }

    private Vector3 ScreenToWorld(Vector2 screenPos)
    {
        float z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, z));
        worldPos.y = 0.1f;
        return worldPos;
    }

    private bool IsTap()
    {
#if UNITY_EDITOR
        float maxDistance = _dragThreshold;
#else
    float maxDistance = _dragThreshold * 3f;
#endif
        return (_currentTouchTime - _initialTouchTime <= _tapThresholdTime &&
                (_currentTouchPosition - _initialTouchPosition).magnitude < maxDistance);
    }

    private void HandleTap(Vector2 screenPos)
    {
        if (EventSystem.current != null)
        {
#if UNITY_EDITOR
            if (EventSystem.current.IsPointerOverGameObject())
                return;
#else
        if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            return;
#endif
        }
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        GameObject hitObject = null;
        Vector3 worldPos = Vector3.zero;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            hitObject = hit.collider.gameObject;
            worldPos = hit.point;
        }
        else
        {
            if (new Plane(Vector3.up, Vector3.zero).Raycast(ray, out float enter))
            {
                worldPos = ray.GetPoint(enter);
            }
        }

        AppEventHandler.InvokeOnTouchEnd(hitObject, worldPos, screenPos);
    }
}