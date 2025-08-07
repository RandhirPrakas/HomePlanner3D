using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Room : MonoBehaviour
{
    public List<Wall> _allRoomWalls = new List<Wall>();
    public Canvas _roomCanvas;
    public HashSet<Vector3> _wallCorners = new HashSet<Vector3>();
    private List<Vector3> _flattenedList = new List<Vector3>();
    private void Awake()
    {
        AppHelper.OnWallCreation += OnWallCreation;
    }

    public void SpawnWallLabelCanvas()
    {
        GameObject canvasGO = new GameObject("WallLabelsCanvas");
        canvasGO.transform.SetParent(transform);

        _roomCanvas = canvasGO.AddComponent<Canvas>();
        _roomCanvas.renderMode = RenderMode.WorldSpace;
        _roomCanvas.worldCamera = Camera.main;

        CanvasScaler cs = canvasGO.AddComponent<CanvasScaler>();
        cs.dynamicPixelsPerUnit = 10;

        canvasGO.AddComponent<GraphicRaycaster>();

        RectTransform rt = _roomCanvas.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(150, 150);
        canvasGO.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
    }

    private void OnWallCreation()
    {
        Debug.Log(_wallCorners.Count);

        if(_wallCorners.Count < 4)
        {
            Debug.Log("Cannot Create Room");
            return;
        }

        _flattenedList = _wallCorners.Select(p => new Vector3(p.x, 0.1f, p.z)).ToList();
        _flattenedList = SortCounterClockwiseXZ(_flattenedList);
        //GenerateFloor

        GenerateFloor();
        
    }

    private void GenerateFloor()
    {
        GameObject floor = transform.Find("Floor")?.gameObject;
        if (floor == null)
        {
            floor = new GameObject("Floor");
            floor.transform.parent = this.transform;
            floor.transform.localPosition = Vector3.zero;
            floor.transform.localRotation = Quaternion.identity;
        }

        // Ensure MeshFilter and MeshRenderer exist
        var meshFilter = floor.GetComponent<MeshFilter>();
        if (meshFilter == null) meshFilter = floor.AddComponent<MeshFilter>();

        var meshRenderer = floor.GetComponent<MeshRenderer>();
        if (meshRenderer == null) meshRenderer = floor.AddComponent<MeshRenderer>();

        // Clear mesh if it exists
        if (meshFilter.sharedMesh != null)
        {
            DestroyImmediate(meshFilter.sharedMesh);
        }

        // Handle too few points
        if (_wallCorners == null || _wallCorners.Count < 3)
        {
            meshRenderer.enabled = false;
            return;
        }

        // Generate new mesh
        var floorGenerator = floor.GetComponent<QuadGenerator>();
        if (floorGenerator == null)
        {
            floorGenerator = floor.AddComponent<QuadGenerator>();
        }
        

        Mesh newMesh = floorGenerator.GenerateFloor(_flattenedList);
        meshFilter.mesh = newMesh;


        // Enable/disable renderer based on point count
        meshRenderer.enabled = _wallCorners.Count >= 3;
    }

    private List<Vector3> SortClockwiseXZ(List<Vector3> points)
    {
        if (points == null || points.Count < 3)
            return points;

        Vector3 center = Vector3.zero;
        foreach (var p in points)
            center += p;
        center /= points.Count;

        points.Sort((a, b) =>
        {
            float angleA = Mathf.Atan2(a.z - center.z, a.x - center.x);
            float angleB = Mathf.Atan2(b.z - center.z, b.x - center.x);
            return angleA.CompareTo(angleB);
        });

        return points;
    }

    private List<Vector3> SortCounterClockwiseXZ(List<Vector3> points)
    {
        if (points == null || points.Count < 3)
            return points;

        Vector3 center = Vector3.zero;
        foreach (var p in points)
            center += p;
        center /= points.Count;

        points.Sort((a, b) =>
        {
            float angleA = Mathf.Atan2(a.z - center.z, a.x - center.x);
            float angleB = Mathf.Atan2(b.z - center.z, b.x - center.x);
            return angleB.CompareTo(angleA); // Flip comparison for CCW
        });

        return points;
    }


    public void UpdateFloorOnEditingPoints()
    {
        HashSet<Vector3> uniqueCorners = new HashSet<Vector3>();

        foreach (Wall wall in _allRoomWalls)
        {
            uniqueCorners.Add(wall.GetStartPosition());
            uniqueCorners.Add(wall.GetEndPosition());
        }

        _flattenedList = uniqueCorners.Select(p => new Vector3(p.x, 0.1f, p.z)).ToList();
        _flattenedList = SortCounterClockwiseXZ(_flattenedList);

        GenerateFloor();
    }

}
