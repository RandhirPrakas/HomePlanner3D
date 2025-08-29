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

    private GameObject _floor;
    private Material _floorMaterial;
    private void Awake()
    {
        AppHelper.OnWallCreation += OnWallCreation;
    }

    private void OnDestroy()
    {
        AppHelper.OnWallCreation -= OnWallCreation;
    }

    private void Start()
    {
        _floorMaterial = Resources.Load<Material>("ProceduralMaterials/DefaultFloorMaterial");
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

        RectTransform rt = _roomCanvas.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(150, 150);
        canvasGO.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
    }

    public void OnWallCreation()
    {
        Debug.Log(_wallCorners.Count);

        if (_wallCorners.Count < 4)
        {
            Debug.Log("Cannot Create Room");
            return;
        }

        _flattenedList = _wallCorners.Select(p => new Vector3(p.x, 0.1f, p.z)).ToList();
        //_flattenedList = SortCounterClockwiseXZ(_flattenedList);
        Debug.Log("Flattenend List Count" + _flattenedList.Count);
        //GenerateFloor

        //_flattenedList.Clear();
        /*foreach (var wall in _allRoomWalls)
        {
            if (!_flattenedList.Any(p => Vector3.Distance(p, wall.GetStartPosition()) < 0.001f))
                _flattenedList.Add(wall.GetStartPosition());
            if (!_flattenedList.Any(p => Vector3.Distance(p, wall.GetEndPosition()) < 0.001f))
                _flattenedList.Add(wall.GetEndPosition());
        }*/

        GenerateFloor();

    }

    private void GenerateFloor()
    {
        _floor = transform.Find("Floor")?.gameObject;
        if (_floor == null)
        {
            _floor = new GameObject("Floor");
            _floor.transform.parent = this.transform;
            _floor.transform.localPosition = Vector3.zero;
            _floor.transform.localRotation = Quaternion.identity;
        }

        // Ensure MeshFilter and MeshRenderer exist
        var meshFilter = _floor.GetComponent<MeshFilter>();
        if (meshFilter == null) meshFilter = _floor.AddComponent<MeshFilter>();  

        var meshRenderer = _floor.GetComponent<MeshRenderer>();
        if (meshRenderer == null) meshRenderer = _floor.AddComponent<MeshRenderer>();

        meshRenderer.material = _floorMaterial;
        // Clear mesh if it exists
        if (meshFilter.sharedMesh != null)
        {
            DestroyImmediate(meshFilter.sharedMesh);
        }

        // Handle too few points
        if (_flattenedList == null || _flattenedList.Count < 3)
        {
            meshRenderer.enabled = false;
            return;
        }

        // Generate new mesh
        var floorGenerator = _floor.GetComponent<QuadGenerator>();
        if (floorGenerator == null)
        {
            floorGenerator = _floor.AddComponent<QuadGenerator>();
        }


        Mesh newMesh = floorGenerator.GenerateFloor(_flattenedList);
        meshFilter.mesh = newMesh;



        // Enable/disable renderer based on point count
        meshRenderer.enabled = _flattenedList.Count >= 3;
    }

    private List<Vector3> BuildOrderedPolygonFromWalls(List<Wall> walls)
    {
        if (walls == null || walls.Count == 0) return new List<Vector3>();

        // Start with first wall
        List<Vector3> ordered = new List<Vector3>();
        Wall current = walls[0];
        Vector3 currentPoint = current.GetStartPosition();
        ordered.Add(currentPoint);
        Vector3 nextPoint = current.GetEndPosition();

        while (ordered.Count < walls.Count)
        {
            ordered.Add(nextPoint);

            // Find the next wall that starts where we ended
            Wall nextWall = walls.FirstOrDefault(w =>
                Vector3.Distance(w.GetStartPosition(), nextPoint) < 0.001f &&
                !ordered.Contains(w.GetEndPosition()));

            if (nextWall == null)
                nextWall = walls.FirstOrDefault(w =>
                    Vector3.Distance(w.GetEndPosition(), nextPoint) < 0.001f &&
                    !ordered.Contains(w.GetStartPosition()));

            if (nextWall == null)
                break; // broken wall loop

            nextPoint = (Vector3.Distance(nextWall.GetStartPosition(), nextPoint) < 0.001f)
                ? nextWall.GetEndPosition()
                : nextWall.GetStartPosition();
        }

        return SortCounterClockwiseXZ(ordered); // final cleanup
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
            return angleB.CompareTo(angleA);
        });

        return points;
    }


    /*public void UpdateFloorOnEditingPoints()
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
    }*/

    public void UpdateFloorOnEditingPoints()
    {
        List<Vector3> corners = new List<Vector3>();

        foreach (Wall wall in _allRoomWalls)
        {
            corners.Add(wall.GetStartPosition());
            corners.Add(wall.GetEndPosition());
        }

        _flattenedList = MergeClosePoints(corners, 0.01f);

        _flattenedList = _flattenedList.Select(p => new Vector3(p.x, 0.1f, p.z)).ToList();

        _flattenedList = SortCounterClockwiseXZ(_flattenedList);

        //_flattenedList.Clear();
        foreach (var wall in _allRoomWalls)
        {
            if (!_flattenedList.Any(p => Vector3.Distance(p, wall.GetStartPosition()) < 0.001f))
                _flattenedList.Add(wall.GetStartPosition());
            if (!_flattenedList.Any(p => Vector3.Distance(p, wall.GetEndPosition()) < 0.001f))
                _flattenedList.Add(wall.GetEndPosition());
        }
        GenerateFloor();
    }

    private List<Vector3> MergeClosePoints(List<Vector3> points, float tolerance)
    {
        List<Vector3> result = new List<Vector3>();
        foreach (var p in points)
        {
            if (!result.Any(r => Vector3.Distance(r, p) < tolerance))
            {
                result.Add(p);
            }
        }
        return result;
    }


    public void CleanUpNullWalls()
    {
        _allRoomWalls.RemoveAll(wall => wall == null);
    }

    public bool HasCornerNear(Vector3 point, float eps = 0.001f)
    {
        foreach (var c in _wallCorners)
        {
            if ((c - point).sqrMagnitude <= eps * eps)
                return true;
        }
        return false;
    }

    public bool HasPoint(Vector3 pos, float tolerance = 0.01f)
    {
        foreach (var p in _wallCorners)
        {
            if (Vector3.Distance(p, pos) <= tolerance)
                return true;
        }
        return false;
    }
}
