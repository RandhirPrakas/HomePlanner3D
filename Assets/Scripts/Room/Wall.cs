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

    [SerializeField] private List<Room> _parentRooms = new List<Room>();

    public LineRenderer _lineRenderer;

    public Material _material;

    // World-space label
    private List<TMP_Text> _labelInstances = new List<TMP_Text>();

    // Colliders
    [SerializeField] private GameObject _colliderGO;
    public BoxCollider _boxCollider;

    private List<GameObject> _wallSegmentColliders = new List<GameObject>();

    #region Public Properties
    public WallPoint StartWallPoint { get => _startWallPoint; set => _startWallPoint = value; }
    public WallPoint EndWallPoint { get => _endWallPoint; set => _endWallPoint = value; }

    public List<GameObject> WallSegmentColliders { get => _wallSegmentColliders; }
    #endregion

    #region Getter And Setters
    public void AddParentRoom(Room room)
    {
        if (!_parentRooms.Contains(room))
        {
            _parentRooms.Add(room);
        }
    }

    public WallPoint GetStartWallPoint() => _startWallPoint;
    public void SetStartWallPoint(WallPoint newWallPoint) => _startWallPoint = newWallPoint;

    public WallPoint GetEndWallPoint() => _endWallPoint;
    public void SetEndWallPoint(WallPoint wallPoint) => _endWallPoint = wallPoint;

    public Vector3 GetStartPosition() => new Vector3(_startWallPoint._position.x, 0, _startWallPoint._position.z);
    public Vector3 GetEndPosition() => new Vector3(_endWallPoint._position.x, 0, _endWallPoint._position.z);

    public List<Room> GetRoomParent() => _parentRooms;

    #endregion


    public void SetStartAndEndPosition(WallPoint startPosition, WallPoint endPosition, Room room = null)
    {
        _startWallPoint = startPosition;
        _endWallPoint = endPosition;

        startPosition.SetPosition(startPosition._position);

        InitLineRenderer();
        EnsureColliderGO();
        //CreateLabel();
        UpdateFromPoints(true);
    }

    private void InitLineRenderer()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        if (_lineRenderer == null)
            _lineRenderer = gameObject.AddComponent<LineRenderer>();

        _lineRenderer.positionCount = 2;
        _lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _lineRenderer.material = Constants.DEFAULT_LINERENDERER_MATERIAL;
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
            UpdateSegmentLabels();
            UpdateCollider(start, end);
            UpdateRoom();

            foreach(Opening opening in _allOpenings)
            {
                opening.UpdatePositionAndRotation();
            }

            if (!isUpdatingPoint)
                UpdateConenctedWalls();
        }
        finally
        {
            _isUpdating = false;
        }
    }

    #region Wall Label

    private void UpdateSegmentLabels()
    {
        Vector3 wallStart = GetStartPosition();
        Vector3 wallEnd = GetEndPosition();
        _allOpenings.Sort((a, b) => a.NormalizedPosition.CompareTo(b.NormalizedPosition));

        Vector3 currentSegmentStart = wallStart;
        int labelIndex = 0;

        foreach (Opening opening in _allOpenings)
        {
            if (opening == null) continue;

            Vector3 openingStart = opening.OpeningStartPoint;
            Vector3 openingEnd = opening.OpeningEndPoint;

            UpdateSingleLabel(labelIndex, currentSegmentStart, openingStart);
            labelIndex++;

            currentSegmentStart = openingEnd;
        }

        UpdateSingleLabel(labelIndex, currentSegmentStart, wallEnd);
        labelIndex++;

        for (int i = labelIndex; i < _labelInstances.Count; i++)
        {
            _labelInstances[i].gameObject.SetActive(false);
        }
    }

    private void UpdateSingleLabel(int index, Vector3 segmentStart, Vector3 segmentEnd)
    {
        float segmentLength = Vector3.Distance(segmentStart, segmentEnd);
        if (segmentLength < 0.1f)
        {
            if (index < _labelInstances.Count)
            {
                _labelInstances[index].gameObject.SetActive(false);
            }
            return;
        }

        TMP_Text label;
        // Create a new label instance if we don't have enough
        if (index >= _labelInstances.Count)
        {
            label = Instantiate(Constants.DEFAULT_WALL_LENGTH_LABEL, transform);
            _labelInstances.Add(label);
        }
        else
        {
            label = _labelInstances[index];
        }

        label.gameObject.SetActive(true);
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 10f;

        // --- Position and Rotate the label (same logic as before, but for the segment) ---
        Vector3 center = (segmentStart + segmentEnd) * 0.5f;
        Vector3 wallDir = (segmentEnd - segmentStart).normalized;

        float dirMul = AppHelper.IsClockwise(GetStartPosition(), GetEndPosition()) ? 1 : -1;
        Vector3 perpendicular = new Vector3(-wallDir.z, 0, wallDir.x) * dirMul;

        label.transform.position = center + perpendicular * 1f + Vector3.up * 0.5f;

        float angle = Mathf.Atan2(wallDir.z, wallDir.x) * Mathf.Rad2Deg;
        label.transform.rotation = Quaternion.Euler(90f, -angle, 0f);

        // --- Update Text and Width for the segment ---
        label.rectTransform.sizeDelta = new Vector2(segmentLength, 1);
        label.text = segmentLength.ToString("F2") + " ft";
    }

    public void DestroyLabel()
    {
        foreach (var label in _labelInstances)
        {
            if (label != null)
                Destroy(label.gameObject);
        }
        _labelInstances.Clear();
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

        _boxCollider.size = new Vector3(AppHelper._wallColliderThickness, 3f, length);
        _boxCollider.center = Vector3.zero;
    }

    private void EnsureColliderGO()
    {
        if (_colliderGO != null && _boxCollider != null) return;

        _colliderGO = new GameObject("WallCollider");
        _colliderGO.tag = "Wall";
        _colliderGO.transform.SetParent(transform, false);
        _boxCollider = _colliderGO.AddComponent<BoxCollider>();
        _boxCollider.size = new Vector3(AppHelper._wallColliderThickness, 3f, 1f);
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


    public void UpdateVisualsOnly()
    {
        if (_startWallPoint == null || _endWallPoint == null || _lineRenderer == null)
            return;

        Vector3 start = _startWallPoint._position;
        Vector3 end = _endWallPoint._position;

        start.y = 0.5f;
        end.y = 0.5f;

        _lineRenderer.SetPosition(0, start);
        _lineRenderer.SetPosition(1, end);
    }

    public void RemoveParentRoom(Room room)
    {
        if (_parentRooms.Contains(room))
        {
            _parentRooms.Remove(room);
        }
    }

    public int GetParentRoomCount()
    {
        return _parentRooms.Count;
    }

    public void ClearParentRooms()
    {
        _parentRooms.Clear();
    }
    public void Refresh()
    {
        UpdateFromPoints();
    }
}
