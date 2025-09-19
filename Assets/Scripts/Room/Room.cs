using System.Collections.Generic;
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

        PopulateRoomWalls();
        SetWallPointPositions();
        AddMeshComponent();
        GenerateFloor();

        UpdateAreaLabel();
    }

    public void UpdateFloor()
    {
        SetWallPointPositions();
        if (_meshFilter != null && _quadGenerator != null)
        {
            _meshFilter.mesh = _quadGenerator.GenerateFloor(_wallPointsPositions);
        }

        UpdateAreaLabel();
    }

    public void GenerateFloor()
    {
        _quadGenerator = GetComponent<QuadGenerator>() ?? gameObject.AddComponent<QuadGenerator>();

        // Store the generated mesh in a temporary variable.
        Mesh generatedMesh = _quadGenerator.GenerateFloor(_wallPointsPositions);
        _meshFilter.mesh = generatedMesh;
        _meshRenderer.material = AppHelper._defaultFloorMaterial;

        if (GetComponent<MeshCollider>() != null)
        {
            Destroy(GetComponent<MeshCollider>());
        }

        if (generatedMesh != null)
        {
            this.gameObject.AddComponent<MeshCollider>();
        }
    }

    private void AddMeshComponent()
    {
        _meshFilter = gameObject.AddComponent<MeshFilter>();
        _meshRenderer = gameObject.AddComponent<MeshRenderer>();
    }

    /*private void SetWallPointPositions()
    {
        _wallPointsPositions.Clear();
        foreach (var wp in _roomWallPoints)
        {
            _wallPointsPositions.Add(wp._position);
            wp.AddConnectedRoom(this);
        }
    }*/

    // In Room.cs

    private void SetWallPointPositions()
    {
        // We must have at least 3 points to form a room.
        if (_roomWallPoints == null || _roomWallPoints.Count < 3)
        {
            _wallPointsPositions.Clear();
            return;
        }

        // This new list will hold the points in their correct connected order.
        List<WallPoint> sortedPoints = new List<WallPoint>();

        // Start with the first point in the list.
        WallPoint currentPoint = _roomWallPoints[0];
        WallPoint lastPoint = null; // Used to prevent immediately going backward.

        // Loop through all points by following their connections.
        for (int i = 0; i < _roomWallPoints.Count; i++)
        {
            sortedPoints.Add(currentPoint);

            WallPoint nextPoint = null;
            // Find the next point in the chain that is part of this room.
            foreach (WallPoint neighbor in currentPoint.GetConnectedWallPoints())
            {
                // Make sure the neighbor is part of this room and is not the point we just came from.
                if (neighbor != lastPoint && _roomWallPoints.Contains(neighbor))
                {
                    // This is our next point in the perimeter.
                    nextPoint = neighbor;
                    break;
                }
            }

            // If we found a next point, continue the chain.
            if (nextPoint != null)
            {
                lastPoint = currentPoint;
                currentPoint = nextPoint;
            }
            else
            {
                Debug.LogWarning("Room perimeter is not a closed loop. Floor generation halted.");
                _wallPointsPositions.Clear();
                return;
            }
        }

        // Update the main list with the correctly ordered points.
        _roomWallPoints = sortedPoints;

        // Finally, create the list of vector positions for the mesh generator.
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

    private void PopulateRoomWalls()
    {
        _roomWalls.Clear();
        for (int i = 0; i < _roomWallPoints.Count; i++)
        {
            WallPoint p1 = _roomWallPoints[i];
            // The modulo operator ensures we loop back to the first point at the end
            WallPoint p2 = _roomWallPoints[(i + 1) % _roomWallPoints.Count];

            // Find the wall that connects p1 and p2
            foreach (Wall wall in p1.GetConnectedWalls())
            {
                // Check if this wall connects to p2
                if (wall.StartWallPoint == p2 || wall.EndWallPoint == p2)
                {
                    if (!_roomWalls.Contains(wall))
                    {
                        _roomWalls.Add(wall);
                        wall.AddParentRoom(this);
                    }
                    break;
                }
            }
        }
    }
}
