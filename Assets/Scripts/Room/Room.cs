using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Room : MonoBehaviour
{
    public List<WallPoint> _roomWallPoints = new List<WallPoint>();
    public List<Wall> _roomWalls = new List<Wall>();

    private List<Vector3> _wallPointsPositions = new List<Vector3>();
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private QuadGenerator _quadGenerator;

    [SerializeField] private float _area;
    [SerializeField] private Vector3 _centroid;

    public void Initialize(List<WallPoint> points)
    {
        _roomWallPoints = points;
        this.tag = "Room";

        SetWallPointPositions();
        AddMeshComponent();
        GenerateFloor();

        UpdateAreaLabel();
    }

    public void UpdateFloor()
    {
        SetWallPointPositions();
        if (_meshFilter != null && _quadGenerator != null)
            _meshFilter.mesh = _quadGenerator.GenerateFloor(_wallPointsPositions);

        UpdateAreaLabel();
    }

    public void GenerateFloor()
    {
        _quadGenerator = GetComponent<QuadGenerator>() ?? gameObject.AddComponent<QuadGenerator>();
        _meshFilter.mesh = _quadGenerator.GenerateFloor(_wallPointsPositions);
        _meshRenderer.material = AppHelper._defaultFloorMaterial;
        this.gameObject.AddComponent<MeshCollider>();
    }

    private void AddMeshComponent()
    {
        _meshFilter = gameObject.AddComponent<MeshFilter>();
        _meshRenderer = gameObject.AddComponent<MeshRenderer>();
    }

    private void SetWallPointPositions()
    {
        _wallPointsPositions.Clear();
        foreach (var wp in _roomWallPoints)
        {
            _wallPointsPositions.Add(wp._position);
            wp.AddConnectedRoom(this);
        }
    }

    #region Area Label
    private void UpdateAreaLabel()
    {
        if (_roomWallPoints.Count < 3) return;

        // Calculate area
        _area = AppHelper.CalculatePolygonArea(_roomWallPoints.ConvertAll(wp => new Vector3(wp._position.x, 0, wp._position.z))
        );

        // Calculate centroid
        _centroid = AppHelper.CalculateCentroid(_wallPointsPositions);

        // Spawn label only once
        if (LabelManager.Instance != null)
            LabelManager.Instance.RequestRoomLabel(this, _centroid, _area);
    }
    #endregion

    public void RemoveRoom()
    {
        foreach (var wp in _roomWallPoints)
            wp.RemoveConnectedRoom(this);

        LabelManager.Instance.RemoveRoomLabel(this);

        Destroy(gameObject);
    }
}
