using UnityEngine;

public class MoveWallState : ICameraSubState
{
    private Wall _activeWall;
    private Vector3 _lastWallPosition;
    private bool _isDragging = false;

    private Vector3 _direction;
    public MoveWallState(Wall wall)
    {
        SetActiveWall(wall);
    }

    public void SetActiveWall(Wall wall)
    {
        _activeWall = wall;
        GameManager.Instance._activeWall = _activeWall;

        Vector3 wallVector = _activeWall.GetEndPosition() - _activeWall.GetStartPosition();
        _direction = new Vector3(-wallVector.z, 0.1f, wallVector.x).normalized;
        Debug.Log($"Active wall set to {_activeWall.name}");
    }

    public void Enter()
    {
        Debug.Log("Entered MoveWallState");
        _isDragging = false;
        GameManager.Instance._uiManager.SetDrawButtonActive(false);
    }

    public void Exit()
    {
        Debug.Log("Exited MoveWallState");
    }

    public void Update() { }

    public void OnTouchStart(Vector3 worldPos, Vector2 screenPos)
    {
        // Start drag only if touching current wall
        if (_activeWall == null)
        {
            GetActiveWall(screenPos);
        }
        _lastWallPosition = worldPos;
        _isDragging = true;
    }

    public void OnTouchHold(Vector3 worldPos, Vector2 screenPos)
    {
        if (!_isDragging || _activeWall == null) return;

        worldPos.y = 0.1f;
        Vector3 delta = worldPos - _lastWallPosition;
        delta.y = 0;

        Vector3 distance = Vector3.Dot(delta, _direction) * _direction;
        if (distance.magnitude > 0.01f)
        {
            MoveWall(distance);
            _lastWallPosition = worldPos;
        }
    }

    public void OnTouchEnd(Vector3 worldPos, Vector2 screenPos)
    {
        _isDragging = false;
        _activeWall = null;
    }

    private void MoveWall(Vector3 positionOffset)
    {
        if (_activeWall == null) return;

        _activeWall.StartWallPoint.transform.position += positionOffset;
        _activeWall.StartWallPoint.SetPosition(_activeWall.StartWallPoint.transform.position);

        _activeWall.EndWallPoint.transform.position += positionOffset;
        _activeWall.EndWallPoint.SetPosition(_activeWall.EndWallPoint.transform.position);

        _activeWall.UpdateFromPoints();
    }

    public void Init(Vector3 worldPos, Vector2 screenPos)
    {
        throw new System.NotImplementedException();
    }

    private void GetActiveWall(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                Wall wall = hit.collider.GetComponentInParent<Wall>();
                if (wall != null && wall != _activeWall)
                {
                    SetActiveWall(wall);
                }
            }
        }
    }
}
