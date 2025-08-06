using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Wall : MonoBehaviour
{
    public List<Opening> _allOpenings = new List<Opening>();

    [SerializeField] private WallPoint _startPosition;
    [SerializeField] private WallPoint _endPosition;

    [SerializeField] private float _wallLength;

    private LineRenderer _lineRenderer;
    private TMP_Text _labelText;
    private RectTransform _labelRect;

    public void SetStartAndEndPosition(WallPoint startPosition, WallPoint endPosition)
    {
        this._startPosition = startPosition;
        this._endPosition = endPosition;

        InitLineRenderer();
        //InitLabel();
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
        _lineRenderer.material = Resources.Load<Material>("ProceduralMaterials/QuadMaterial 1");
        _lineRenderer.SetPosition(0, _startPosition._position);
        _lineRenderer.SetPosition(1, _endPosition._position);
    }

    private void InitLabel()
    {
        if (_labelText != null)
            return;

        GameObject canvasGO = GameObject.Find("WallLabelsCanvas");
        if (canvasGO == null)
        {
            Debug.LogWarning("WallLabelsCanvas not found in scene!");
            return;
        }

        // Create new label under shared canvas
        GameObject labelGO = new GameObject("WallLengthLabel", typeof(RectTransform));
        labelGO.transform.SetParent(canvasGO.transform, false);

        _labelText = labelGO.AddComponent<TextMeshProUGUI>();
        _labelText.fontSize = 14;
        _labelText.color = Color.black;

        _labelRect = _labelText.GetComponent<RectTransform>();
        _labelRect.sizeDelta = new Vector2(50, 30);
    }

    public void UpdateFromPoints()
    {
        if (_startPosition == null || _endPosition == null || _lineRenderer == null)
            return;

        Vector3 start = _startPosition._position;
        Vector3 end = _endPosition._position + Vector3.up * 10;

        _lineRenderer.SetPosition(0, start);
        _lineRenderer.SetPosition(1, end);

        _wallLength = Vector3.Distance(start, end);
        UpdateLabel(start, end);
    }

    private void UpdateLabel(Vector3 start, Vector3 end)
    {
        if (_labelText == null || _labelRect == null)
            return;

        Vector3 center = (start + end) / 2f;
        _labelRect.position = center + Vector3.up * 0.1f;

        _labelText.text = _wallLength.ToString("F2") + "m";

        // Optionally face the camera
        _labelRect.forward = Camera.main.transform.forward;
    }
}
