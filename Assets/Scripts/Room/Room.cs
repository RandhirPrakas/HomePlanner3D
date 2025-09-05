using System.Collections.Generic;
using System.Data;
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
    public void Initialize(List<WallPoint> points)
    {
        _roomWallPoints = points;

        Debug.Log($"Room created with {_roomWallPoints.Count} points");

        SetWallPointPositions();

        AddMeshComponent();
        GenerateFloor();
    }

    public void UpdateFloor()
    {
        SetWallPointPositions();
        _meshFilter.mesh = _quadGenerator.GenerateFloor(_wallPointsPositions);
    }



    public void GenerateFloor()
    {
        _quadGenerator = this.gameObject.GetComponent<QuadGenerator>();
        if (_quadGenerator == null)
            _quadGenerator = this.gameObject.AddComponent<QuadGenerator>();

        _meshFilter.mesh = _quadGenerator.GenerateFloor(_wallPointsPositions);
        _meshRenderer.material = AppHelper._defaultFloorMaterial;
    }

    private void AddMeshComponent()
    {
        _meshFilter = this.AddComponent<MeshFilter>();
        _meshRenderer = this.AddComponent<MeshRenderer>();
    }

    public void RemoveRoom()
    {
        // remove connected room reference fromt he wallpoint of room
        foreach(WallPoint wp in _roomWallPoints)
        {
            wp.RemoveConnectedRoom(this);
        }


        // Delete the gameobjet reference
        Destroy(this.gameObject);
    }

    private void SetWallPointPositions()
    {
        _wallPointsPositions.Clear();
        foreach (WallPoint wp in _roomWallPoints)
        {
            _wallPointsPositions.Add(wp._position);
            wp.AddConnectedRoom(this);
        }
    }
}
