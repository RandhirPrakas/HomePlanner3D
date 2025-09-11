using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Wall : MonoBehaviour
{
    public List<Opening> _allOpenings = new List<Opening>();

    [SerializeField] private WallPoint _startWallPoint;
    [SerializeField] private WallPoint _endWallPoint;

    [SerializeField] private float _wallLength;

    [SerializeField] private Room _parentRoom;

    public LineRenderer _lineRenderer;

    // World-space label
    [SerializeField] private TMP_Text _labelPrefab;
    private TMP_Text _labelInstance;

    // Colliders
    [SerializeField] private GameObject _colliderGO;
    public BoxCollider _boxCollider;

    #region Public Properties
    public WallPoint StartWallPoint { get => _startWallPoint; set => _startWallPoint = value; }
    public WallPoint EndWallPoint { get => _endWallPoint; set => _endWallPoint = value; }
    #endregion

    #region Getter And Setters
    public void SetParentRoom(Room room) => _parentRoom = room;

    public WallPoint GetStartWallPoint() => _startWallPoint;
    public void SetStartWallPoint(WallPoint newWallPoint) => _startWallPoint = newWallPoint;

    public WallPoint GetEndWallPoint() => _endWallPoint;
    public void SetEndWallPoint(WallPoint wallPoint) => _endWallPoint = wallPoint;

    public Vector3 GetStartPosition() => new Vector3(_startWallPoint._position.x, 0, _startWallPoint._position.z);
    public Vector3 GetEndPosition() => new Vector3(_endWallPoint._position.x, 0, _endWallPoint._position.z);

    public Room GetRoomParent() => _parentRoom;
    #endregion

    private void Start()
    {
        _labelPrefab = Resources.Load<TMP_Text>("Prefabs/Label/LabelPrefab");
    }

    public void SetStartAndEndPosition(WallPoint startPosition, WallPoint endPosition, Room room = null)
    {
        _startWallPoint = startPosition;
        _endWallPoint = endPosition;

        if (room != null)
            _parentRoom = room;

        startPosition.SetPosition(startPosition._position);

        InitLineRenderer();
        EnsureColliderGO();
        CreateLabel();
        UpdateFromPoints(true);
    }

    private void InitLineRenderer()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        if (_lineRenderer == null)
            _lineRenderer = gameObject.AddComponent<LineRenderer>();

        _lineRenderer.positionCount = 2;
        _lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _lineRenderer.material = Resources.Load<Material>("ProceduralMaterials/DefaultLRmaterial");
        _lineRenderer.startWidth = AppHelper._lrThickness;
        _lineRenderer.endWidth = AppHelper._lrThickness;
    }

    private bool _isUpdating = false;
    public void UpdateFromPoints(bool isUpdatingPoint = false)
    {
        if (_isUpdating) return;
        _isUpdating = true;

        try
        {
            if (_startWallPoint == null || _endWallPoint == null || _lineRenderer == null)
                return;

            Vector3 start = _startWallPoint._position;
            Vector3 end = _endWallPoint._position;

            start.y = 0.5f;
            end.y = 0.5f;

            _lineRenderer.SetPosition(0, start);
            _lineRenderer.SetPosition(1, end);

            _wallLength = Vector3.Distance(start, end);
            UpdateLabel(start, end);
            UpdateCollider(start, end);
            UpdateRoom();

            if (!isUpdatingPoint)
                UpdateConenctedWalls();
        }
        finally
        {
            _isUpdating = false;
        }
    }

    #region Wall Label

    private void CreateLabel()
    {
        if (_labelPrefab == null)
            _labelPrefab = Resources.Load<TMP_Text>("Prefabs/Wall/WallLengthLabel");

        if (_labelInstance != null)
            Destroy(_labelInstance.gameObject);

        _labelInstance = Instantiate(_labelPrefab, transform);
        _labelInstance.alignment = TextAlignmentOptions.Center;
        _labelInstance.fontSize = 10f;

        // Initialize position
        UpdateLabel(GetStartPosition(), GetEndPosition());
    }

    /*private void UpdateLabel(Vector3 start, Vector3 end)
    {
        if (_labelInstance == null) return;

        Vector3 center = (start + end) * 0.5f;
        float dirMul = AppHelper.IsClockwise(start, end)?1:-1;

        // Compute perpendicular direction on XZ plane
        Vector3 wallDir = (end - start).normalized * dirMul;
        Vector3 perpendicular = new Vector3(-wallDir.z, 0, wallDir.x);

        float offsetDistance = 1f; // distance from wall
        Vector3 labelPos = center + perpendicular * offsetDistance + Vector3.up * 0.5f;

        _labelInstance.transform.position = labelPos;

        // Rotate the label so it faces the camera or aligns nicely with wall
        float angle = Mathf.Atan2(wallDir.z, wallDir.x) * Mathf.Rad2Deg;
        _labelInstance.transform.rotation = Quaternion.Euler(90f, -angle, 0f);

        // Update text
        _labelInstance.text = _wallLength.ToString("F2") + " ft";
        _labelInstance.rectTransform.sizeDelta = new Vector2(_wallLength, 0.5f);
    }*/

    private void UpdateLabel(Vector3 start, Vector3 end)
    {
        if (_labelInstance == null) return;

        Vector3 center = (start + end) * 0.5f;
        float dirMul = AppHelper.IsClockwise(start, end) ? 1 : -1;

        // Compute perpendicular direction on XZ plane
        Vector3 wallDir = (end - start).normalized * dirMul;
        Vector3 perpendicular = new Vector3(-wallDir.z, 0, wallDir.x);

        float offsetDistance = 1f; // distance from wall
        Vector3 labelPos = center + perpendicular * offsetDistance + Vector3.up * 0.5f;

        _labelInstance.transform.position = labelPos;

        _labelInstance.rectTransform.sizeDelta = new Vector2(_wallLength, 1);
        float angle = Mathf.Atan2(wallDir.z, wallDir.x) * Mathf.Rad2Deg;
        _labelInstance.transform.rotation = Quaternion.Euler(90f, -angle, 0f);

        // We don't need to update the text since it seems you're using a separate text object
         _labelInstance.text = _wallLength.ToString("F2") + " ft";
    }

    public void DestroyLabel()
    {
        if (_labelInstance != null)
            Destroy(_labelInstance.gameObject);

        _labelInstance = null;
    }

    #endregion

    private void UpdateCollider(Vector3 start, Vector3 end)
    {
        if (_boxCollider == null) return;

        Vector3 mid = (start + end) * 0.5f;
        Vector3 dir = end - start;
        float length = dir.magnitude;
        if (length <= Mathf.Epsilon) return;

        _colliderGO.transform.SetPositionAndRotation(
            new Vector3(mid.x, start.y, mid.z),
            Quaternion.LookRotation(dir.normalized, Vector3.up)
        );

        _boxCollider.size = new Vector3(2.5f, 3f, length);
        _boxCollider.center = Vector3.zero;
    }

    private void EnsureColliderGO()
    {
        if (_colliderGO != null && _boxCollider != null) return;

        _colliderGO = new GameObject("WallCollider");
        _colliderGO.tag = "Wall";
        _colliderGO.transform.SetParent(transform, false);
        _boxCollider = _colliderGO.AddComponent<BoxCollider>();
        _boxCollider.size = new Vector3(1, 3f, 1f);
    }

    private void UpdateConenctedWalls()
    {
        foreach (Wall wall in _startWallPoint.GetConnectedWalls())
        {
            if (wall == this) continue;
            wall.UpdateFromPoints();
        }

        foreach (Wall wall in _endWallPoint.GetConnectedWalls())
        {
            if (wall == this) continue;
            wall.UpdateFromPoints();
        }
    }

    private void UpdateRoom()
    {
        if (_startWallPoint.GetConnectedRooms().Count == 0 || _endWallPoint.GetConnectedRooms().Count == 0)
            return;

        foreach (Room room in _startWallPoint.GetConnectedRooms())
            room.UpdateFloor();

        foreach (Room room in _endWallPoint.GetConnectedRooms())
            room.UpdateFloor();
    }

    public void DeleteWall()
    {
        foreach (var opening in new List<Opening>(_allOpenings))
            opening.Detach();

        if (WallManager.Instance._allWalls.Contains(this))
            WallManager.Instance._allWalls.Remove(this);

        DestroyLabel();
        Destroy(gameObject);
    }
}
