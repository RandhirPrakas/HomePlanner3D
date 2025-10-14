// Create a new script named WallVisualizer.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WallVisualizer : MonoBehaviour
{
    [SerializeField] private Wall _wallData;
    [SerializeField] private GameObject _segmentPrefab; // A simple prefab with a LineRenderer

    private List<GameObject> _currentSegments = new List<GameObject>();

    public void Initialize(Wall wallData, GameObject segmentPrefab)
    {
        _wallData = wallData;
        _segmentPrefab = segmentPrefab;
        Rebuild();
    }

    public void Rebuild()
    {
        // Clear any old segments
        foreach (var segment in _currentSegments)
        {
            Destroy(segment);
        }
        _currentSegments.Clear();

        if (_wallData == null || _segmentPrefab == null) return;
         
        // Get the defining points of the wall
        Vector3 wallStart = _wallData.StartWallPoint._position;
        Vector3 wallEnd = _wallData.EndWallPoint._position;
        float wallLength = Vector3.Distance(wallStart, wallEnd);
        if (wallLength < 0.01f) return;

        // Create a sorted list of "events" along the wall
        var eventPoints = new List<(float normalizedPos, bool isOpening)>();
        eventPoints.Add((0, false));

        // Sort openings by their position along the wall
        var sortedOpenings = _wallData._allOpenings.OrderBy(o => o.NormalizedPosition);

        foreach (var opening in sortedOpenings)
        {
            float halfWidthNormalized = (opening.Width / 2f) / wallLength;
            eventPoints.Add((opening.NormalizedPosition - halfWidthNormalized, true));   
            eventPoints.Add((opening.NormalizedPosition + halfWidthNormalized, false));
        }

        eventPoints.Add((1, false)); // Wall end

        // Create segments between the event points
        for (int i = 0; i < eventPoints.Count - 1; i++)
        {
            var startEvent = eventPoints[i];
            var endEvent = eventPoints[i + 1];

            // If the space between two events is an opening, skip it (or place a door frame, etc.)
            if (startEvent.isOpening)
            {
                // Here, you could instantiate a door/window frame visualizer if desired
                continue;
            }

            Vector3 segmentStart = Vector3.Lerp(wallStart, wallEnd, startEvent.normalizedPos);
            Vector3 segmentEnd = Vector3.Lerp(wallStart, wallEnd, endEvent.normalizedPos);

            CreateSegment(segmentStart, segmentEnd);
        }
    }

    private void CreateSegment(Vector3 start, Vector3 end)
    {
        if (Vector3.Distance(start, end) < 0.1f) return;

        GameObject segmentGO = Instantiate(_segmentPrefab, transform);
        segmentGO.name = "WallSegment";
        _currentSegments.Add(segmentGO);

        // --- Configure Line Renderer ---
        LineRenderer lr = segmentGO.GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, new Vector3(start.x, 0.5f, start.z));
        lr.SetPosition(1, new Vector3(end.x, 0.5f, end.z));

        // --- Configure Box Collider ---
        BoxCollider bc = segmentGO.GetComponent<BoxCollider>();
        Vector3 midPoint = (start + end) / 2f;
        segmentGO.transform.position = midPoint;
        segmentGO.transform.LookAt(end);

        float length = Vector3.Distance(start, end);
        bc.size = new Vector3(AppHelper._wallColliderThickness, AppHelper._wallHeight, length);
    }
}