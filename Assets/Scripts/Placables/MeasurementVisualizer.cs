// MeasurementVisualizer.cs
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages the visual feedback for measuring distances from an object to its surroundings.
/// </summary>
public class MeasurementVisualizer
{
    private class MeasurementLine
    {
        public GameObject Instance { get; }
        public LineRenderer Line { get; }
        public TMP_Text Text { get; }

        public MeasurementLine(GameObject instance)
        {
            Instance = instance;
            Line = instance.GetComponent<LineRenderer>();
            Text = instance.GetComponentInChildren<TMP_Text>();
        }

        public void SetActive(bool isActive) => Instance.SetActive(isActive);
    }

    private readonly GameObject _measurementPrefab;
    private readonly List<MeasurementLine> _measurementLines = new List<MeasurementLine>();
    private readonly Vector3[] _directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };

    private const float LINE_Y_POSITION = 1.5f;

    public MeasurementVisualizer()
    {
        _measurementPrefab = Constants.OBJECT_DISTANCE_LABEL_PREFAB;
        if (_measurementPrefab == null)
        {
            Debug.LogError("MeasurementLine_Prefab could not be loaded!");
            return;
        }
        Initialize();
    }

    private void Initialize()
    {
        foreach (var direction in _directions)
        {
            var instance = Object.Instantiate(_measurementPrefab);
            instance.name = $"MeasurementVisualizer";
            instance.SetActive(false);
            _measurementLines.Add(new MeasurementLine(instance));
        }
    }

    public void UpdateVisuals(Collider objectCollider)
    {
        if (objectCollider == null || _measurementLines.Count == 0) return;

        Bounds objectBounds = objectCollider.bounds;
        for (int i = 0; i < _directions.Length; i++)
        {
            UpdateLine(objectBounds, _directions[i], _measurementLines[i]);
        }
    }

    private void UpdateLine(Bounds bounds, Vector3 direction, MeasurementLine measurement)
    {
        Vector3 rayOrigin = new Vector3(bounds.center.x, LINE_Y_POSITION, bounds.center.z);

        if (Physics.Raycast(rayOrigin, direction, out RaycastHit hit, 100f))
        {
            measurement.SetActive(true);

            Vector3 objectEdge = bounds.ClosestPoint(hit.point);

            Vector3 lineStart = objectEdge /*+ (direction * LINE_START_OFFSET)*/;

            Vector3 lineEnd = hit.point;
            /*if (hit.collider.CompareTag(Constants.TAG_WALL))
                lineEnd += direction * (AppHelper._wallColliderThickness - AppHelper._wallThickness + 0.1f) / 2;*/

            lineStart.y = LINE_Y_POSITION;
            lineEnd.y = LINE_Y_POSITION;

            measurement.Line.SetPosition(0, lineStart);
            measurement.Line.SetPosition(1, lineEnd);
            measurement.Line.useWorldSpace = true;

            measurement.Text.text = hit.distance.ToString("F2") + " ft";
            measurement.Text.transform.position = (lineStart + hit.point) / 2;

            Vector3 textDirection = (lineEnd - lineStart).normalized;
            float angle = Mathf.Atan2(textDirection.z, textDirection.x) * Mathf.Rad2Deg;
            float yRotation = -angle;

            if (direction == Vector3.left)
            {
                yRotation += 180f;
            }

            measurement.Text.transform.rotation = Quaternion.Euler(90f, yRotation, 0f);
        }
        else
        {
            measurement.SetActive(false);
        }
    }

    public void DestroyVisuals()
    {
        foreach (var visual in _measurementLines)
        {
            if (visual.Instance != null)
            {
                Object.Destroy(visual.Instance);
            }
        }
        _measurementLines.Clear();
    }
}