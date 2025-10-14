using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class Opening : MonoBehaviour
{
    [SerializeField] private OpeningType _openingType = OpeningType.Door;
    [SerializeField] private Vector3 _openingPosition;
    [SerializeField] private float _normalizedPosition;

    [SerializeField] private float _width = 2;
    [SerializeField] private float _height = 2;

    [SerializeField] private GameObject _strandedOpenings;
    public OpeningVisualizer _openingVisualizer;

    [SerializeField] private TMP_Text _widthLabel;

    [Header("Resizing")]
    [SerializeField] private GameObject _resizeHandlePrefab;
    private List<GameObject> _resizeHandleGOs = new List<GameObject>();
    public BoxCollider _boxCollider;

    private MeshRenderer _openingRenderer;
    public GameObject _openingRedererGo;

    public Wall ParentWall
    {
        get => _parentWall;
        set
        {
            if (_parentWall == value) return;
            _parentWall = value;
        }
    }
    public Vector3? _lastKnownWallStart = null;
    public Vector3? _lastKnownWallEnd = null;

    [SerializeField] private Vector3 _openingStart, _openingEnd;

    public Vector3 OpeningStart
    {
        get => _openingStart;
        set
        {
            if (_openingStart == value) return;
            _openingStart = value;
        }
    }

    public Vector3 OpeningEnd
    {
        get => _openingEnd;
        set
        {
            if (_openingEnd == value) return;
            _openingEnd = value;
        }
    }

    public float NormalizedPosition => _normalizedPosition;
    public Transform StrandedOpening { get => _strandedOpenings.transform; }
    public OpeningVisualizer OpeningVisual { get => _openingVisualizer; set => _openingVisualizer = value; }

    public Wall _parentWall;
    public Wall _lastWall;

    #region Properties

    public MeshRenderer OpeningRenderer { get => _openingRenderer; 
    set
        {
            if (_openingRenderer = value) return;
            _openingRedererGo = _openingRenderer.gameObject;
        }
    }
    public float Width
    {
        get => _width;
        set
        {
            _width = Mathf.Max(value, 0.5f); // Using your original value, just preventing zero/negative size
            if (_openingVisualizer != null)
            {
                _openingVisualizer.UpdateWidth(_width);
            }
            UpdateWidthLabel();
            RepositionHandles(); // --- MODIFIED: This line is required for real-time updates ---
            UpdateCollider();
        }
    }
    public float Height
    {
        get => _height;
        set
        {
            _height = Mathf.Max(value, 0.5f);
            RepositionHandles(); // --- MODIFIED: This line is required for real-time updates ---
            UpdateCollider();
        }
    }
    public Vector3 OpeningPosition { get => _openingPosition; set => _openingPosition = value; }
    public OpeningType OpeningType { get => _openingType; set => _openingType = value; }
    public Vector3 OpeningStartPoint
    {
        get
        {
            if (ParentWall == null) return transform.position;
            Vector3 wallDir = (ParentWall.GetEndPosition() - ParentWall.GetStartPosition()).normalized;
            return transform.position - wallDir * (Width / 2f);
        }
    }

    public Vector3 OpeningEndPoint
    {
        get
        {
            if (ParentWall == null) return transform.position;
            Vector3 wallDir = (ParentWall.GetEndPosition() - ParentWall.GetStartPosition()).normalized;
            return transform.position + wallDir * (Width / 2f);
        }
    }

    #endregion

    private void Awake()
    {
        _strandedOpenings = GameObject.Find("StrandedOpenings");
    }
    public virtual void Initialize(Wall wall, Vector3 worldPosition)
    {
        this.ParentWall = wall;
        this.transform.SetParent(wall.transform);

        if (Constants.DEFAULT_WALL_LENGTH_LABEL != null && _widthLabel == null)
        {
            GameObject labelGO = Instantiate(Constants.DEFAULT_WALL_LENGTH_LABEL, transform).gameObject;
            _widthLabel = labelGO.GetComponent<TMP_Text>();
        }
    }

    public void Detach(Vector3 lastStart, Vector3 lastEnd)
    {
        if (ParentWall != null)
        {
            ParentWall._allOpenings.Remove(this);
            _lastWall = ParentWall;
            ParentWall = null;
            _lastKnownWallStart = lastStart;
            _lastKnownWallEnd = lastEnd;
        }

        if (StrandedOpening != null)
        {
            transform.SetParent(StrandedOpening, true);
        }
    }

    public void CalculateAndSetNormalizedPosition(Vector3 worldPosition)
    {
        if (ParentWall == null) return;
        Vector3 wallStart = ParentWall.GetStartPosition();
        Vector3 wallEnd = ParentWall.GetEndPosition();
        Vector3 wallVector = wallEnd - wallStart;
        Vector3 openingVector = worldPosition - wallStart;
        float distanceAlongWall = Vector3.Dot(openingVector, wallVector.normalized);
        if (wallVector.magnitude > 0.01f)
            _normalizedPosition = distanceAlongWall / wallVector.magnitude;
        else
            _normalizedPosition = 0f;
    }

    public void UpdatePositionAndRotation()
    {
        if (ParentWall == null) return;
        Vector3 wallStart = ParentWall.GetStartPosition();
        Vector3 wallEnd = _parentWall.GetEndPosition();
        Vector3 wallVector = wallEnd - wallStart;
        Vector3 newWorldPosition = wallStart + wallVector * _normalizedPosition;
        OpeningPosition = new Vector3(newWorldPosition.x, 3f, newWorldPosition.z);

        Vector3 wallDirection = (wallEnd - wallStart).normalized;
        Vector3 perpendicular = Vector3.Cross(wallDirection, Vector3.up).normalized;
        if (OpeningVisual != null)
        {
            OpeningVisual.transform.rotation = Quaternion.LookRotation(perpendicular, Vector3.up) * Quaternion.Euler(90f, 0f, 0f);
        }
        if (wallDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(wallDirection);
        }
    }

    public void UpdateWidthLabel()
    {
        if (_widthLabel == null) return;
        _widthLabel.text = _width.ToString("F2") + " ft";
        _widthLabel.rectTransform.sizeDelta = new Vector2(_width, 1);
        // The rest of your label logic...
    }

    // --- ADDED: Handle Management Region ---
    #region Handle Management
    public void ShowResizeHandles()
    {
        HideResizeHandles();
        if (_resizeHandlePrefab == null)
            _resizeHandlePrefab = Resources.Load<GameObject>("Prefabs/ResizeHandlePrefab");
        if (_resizeHandlePrefab == null) return;

        CreateHandle(CornerType.TopLeft);
        CreateHandle(CornerType.TopRight);
        CreateHandle(CornerType.BottomLeft);
        CreateHandle(CornerType.BottomRight);
    }

    public void HideResizeHandles()
    {
        foreach (GameObject handleGO in _resizeHandleGOs)
        {
            if (handleGO != null) Destroy(handleGO);
        }
        _resizeHandleGOs.Clear();
    }

    private void CreateHandle(CornerType cornerType)
    {
        GameObject handleGO = Instantiate(_resizeHandlePrefab, transform);
        handleGO.name = $"{cornerType.ToString()}";
        ResizeHandle handleComp = handleGO.GetComponent<ResizeHandle>();
        handleComp.ownerOpening = this;
        handleComp.corner = cornerType;
        _resizeHandleGOs.Add(handleGO);
    }

    public void RepositionHandles()
    {
        if (_resizeHandleGOs.Count == 0) return;
        const float visibilityOffset = 0.1f;
        foreach (GameObject handleGO in _resizeHandleGOs)
        {
            ResizeHandle handle = handleGO.GetComponent<ResizeHandle>();
            float y = 0, z = 0;
            switch (handle.corner)
            {
                case CornerType.TopRight: z = Width / 2f; y = Height; break;
                case CornerType.TopLeft: z = -Width / 2f; y = Height; break;
                case CornerType.BottomRight: z = Width / 2f; y = 0; break;
                case CornerType.BottomLeft: z = -Width / 2f; y = 0; break;
            }
            handle.transform.localPosition = new Vector3(visibilityOffset, y, z);
        }
    }

    public void UpdateCollider()
    {
        if (_boxCollider == null)
        {
            Debug.LogError("BoxCollider is missing on this Opening!", this);
            return;
        }

        if (this is Door)
            _boxCollider.center = new Vector3(0, (_height / 2f) - 3f, 0);
        else if (this is Window)
            _boxCollider.center = Vector3.zero;

        _boxCollider.size = new Vector3(0.2f, _height, _width);
    }
    #endregion
}