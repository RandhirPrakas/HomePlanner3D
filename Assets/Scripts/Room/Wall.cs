using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Wall : MonoBehaviour
{
    public List<Opening> _allOpenings = new List<Opening>();

    [SerializeField] private WallPoint _startWallPoint;
    [SerializeField] private WallPoint _endWallPoint;

    [SerializeField] private float _wallLength;

    [SerializeField] private GameObject _canvasGO;
    [SerializeField] private Room _parentRoom;

    private LineRenderer _lineRenderer;

    private GameObject _labelGO;
    private TMP_Text _labelText;
    private RectTransform _labelRect;

    // Colliders
    [SerializeField] private GameObject _colliderGO;
    [SerializeField] private BoxCollider _boxCollider;

    #region Public Properties
    public WallPoint StartWallPoint { get => _startWallPoint; set => _startWallPoint = value; }
    public WallPoint EndWallPoint { get => _endWallPoint; set => _endWallPoint = value; }
    #endregion

    #region Getter And Setters

    public Room GetParentRoom()
    {
        return _parentRoom;
    }

    public void SetParentRoom(Room room)
    {
        _parentRoom = room;
    }

    public WallPoint GetStartWallPoint()
    {
        return _startWallPoint;
    }

    public void SetStartWallPoint(WallPoint newWallPoint)
    {
        _startWallPoint = newWallPoint;
    }

    public WallPoint GetEndWallPoint()
    {
        return _endWallPoint;
    }

    public void SetEndWallPoint(WallPoint wallPoint)
    {
        _endWallPoint = wallPoint;
    }

    public Vector3 GetStartPosition()
    {
        Vector3 pos = new Vector3(_startWallPoint._position.x, 0, _startWallPoint._position.z);
        return pos;
    }

    public Vector3 GetEndPosition()
    {
        Vector3 pos = new Vector3(_endWallPoint._position.x, 0, _endWallPoint._position.z);
        return pos;
    }

    public Room GetRoomParent()
    {
        return _parentRoom;
    }

    #endregion


    public void SetStartAndEndPosition(WallPoint startPosition, WallPoint endPosition, Room room)
    {
        this._startWallPoint = startPosition;
        this._endWallPoint = endPosition;
        this._parentRoom = room;

        InitLineRenderer();
        EnsureColliderGO();
        InitLabel();
        UpdateFromPoints();
    }

    private void InitLineRenderer()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        if (_lineRenderer == null)
        {
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        _lineRenderer.positionCount = 2;
        _lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _lineRenderer.material = Resources.Load<Material>("ProceduralMaterials/DefaultLRmaterial");
        _lineRenderer.startWidth = AppHelper._lrThickness;
        _lineRenderer.endWidth = AppHelper._lrThickness;
        _lineRenderer.SetPosition(0, _startWallPoint._position);
        _lineRenderer.SetPosition(1, _endWallPoint._position);
    }


    private void InitLabel()
    {
        if (_labelText != null)
            return;

        // Use the parent room's canvas instead of global Find
        if (_canvasGO == null && _parentRoom != null)
        {
            if (_parentRoom._roomCanvas == null)
            {
                _parentRoom.SpawnWallLabelCanvas();
            }
            _canvasGO = _parentRoom._roomCanvas.gameObject;
        }

        if (_canvasGO == null)
        {
            Debug.LogWarning("No room canvas found for this wall.");
            return;
        }

        GameObject labelPrefab = Resources.Load<GameObject>("Prefabs/WallLabelPrefab");
        if (labelPrefab == null)
        {
            Debug.LogError("WallLabelPrefab not found in Resources.");
            return;
        }

        // Instantiate prefab into this room's canvas
        _labelGO = Instantiate(labelPrefab, _canvasGO.transform);
        _labelGO.transform.localRotation = Quaternion.identity;

        _labelText = _labelGO.GetComponentInChildren<TMPro.TMP_Text>();
        _labelRect = _labelGO.GetComponent<RectTransform>();
    }




    public void UpdateFromPoints()
    {
        if (_startWallPoint == null || _endWallPoint == null || _lineRenderer == null)
            return;

        Vector3 start = _startWallPoint._position;
        Vector3 end = _endWallPoint._position;

        _lineRenderer.SetPosition(0, start);
        _lineRenderer.SetPosition(1, end);

        _wallLength = Vector3.Distance(start, end);
        UpdateLabel(start, end);
        UpdateCollider(start, end);

        _parentRoom?.UpdateFloorOnEditingPoints();
    }

    private void UpdateLabel(Vector3 start, Vector3 end)
    {
        if (_labelText == null || _labelRect == null)
            return;

        Vector3 center = (start + end) / 2f;
        Vector3 direction = (end - start).normalized;

        // Set position
        _labelRect.position = center + Vector3.up * 0.1f;

        // Set rotation
        float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;

        if (angle > 90 || angle < -90)
            angle += 180;

        _labelRect.rotation = Quaternion.Euler(90f, 0f, -angle);

        // Not working
        // SetSize (So World Space matchses with wall length) 
        float height = 0f; // fixed height in world units
        _labelRect.sizeDelta = new Vector2(_wallLength, height);

        // Set text
        _labelText.text = (_wallLength).ToString("F2") + " ft";
    }

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

        float lrWidth = _lineRenderer != null ? _lineRenderer.startWidth : AppHelper._lrThickness;
        float pickPadding = Mathf.Max(0.02f, lrWidth * 0.25f); 
        float colliderZ = Mathf.Max(0.01f, length);

        _boxCollider.size = new Vector3(2.5f, 3f, colliderZ);
        _boxCollider.center = Vector3.zero;

    }

    public Room GetCurrentRoom()
    {
        return _parentRoom;
    }

    public void DestroyLabel()
    {
        if (_labelGO != null)
        {
            Destroy(_labelGO);
            _labelGO = null;
        }

        _labelText = null;
        _labelRect = null;
    }

    // Colliders 

    private void EnsureColliderGO()
    {
        if (_colliderGO != null && _boxCollider != null) return;

        _colliderGO = new GameObject("WallCollider");
        _colliderGO.tag = "Wall";
        _colliderGO.transform.SetParent(transform, false); 
        _boxCollider = _colliderGO.AddComponent<BoxCollider>();
        _boxCollider.size = new Vector3(1, _boxCollider.size.y, _boxCollider.size.z);
    }
}
