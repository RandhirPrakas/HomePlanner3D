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

    #region Public Properties
    public WallPoint StartWallPoint { get => _startWallPoint; set => _startWallPoint = value; }
    public WallPoint EndWallPoint { get => _endWallPoint; set => _endWallPoint = value; }
    #endregion

    #region Getter And Setters

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

        if (_canvasGO == null)
        {
            _canvasGO = GameObject.Find("WallLabelsCanvas");
            if (_canvasGO == null)
            {
                Debug.LogWarning("WallLabelsCanvas not found in scene!");
                return;
            }
        }

        // Load prefab containing the label with left/right arrows and TMP
        GameObject labelPrefab = Resources.Load<GameObject>("Prefabs/WallLabelPrefab");
        _labelGO = labelPrefab;
        labelPrefab.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        if (labelPrefab == null)
        {
            Debug.LogError("WallLabelPrefab not found in Resources.");
            return;
        }

        GameObject labelGO = Instantiate(labelPrefab, _canvasGO.transform);
        _labelText = labelGO.GetComponentInChildren<TMP_Text>();
        _labelRect = labelGO.GetComponent<RectTransform>();
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
        _labelText.text = _wallLength.ToString("F2") + " ft";
    }

    public Room GetCurrentRoom()
    {
        return _parentRoom;
    }

    public void DestroyLabel()
    {
        if (_labelGO != null)
        {
            GameObject.Destroy(_labelGO);
            _labelGO = null;
        }
    }
}
