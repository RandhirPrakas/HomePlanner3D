using System.Collections.Generic;
using UnityEngine;

public abstract class Opening : MonoBehaviour
{

    [SerializeField] private OpeningType _openingType = OpeningType.Door;
    [SerializeField] private Vector3 _openingPosition;
    [SerializeField] private float _normalizedPosition;

    [SerializeField] private float _width = 2f;
    [SerializeField] private float _height = 2f;

    [SerializeField] private GameObject _strandedOpenings;
    public OpeningVisualizer _openingVisualizer;

    public float NormalizedPosition => _normalizedPosition;
    public Transform StrandedOpening { get => _strandedOpenings.transform; }
    public OpeningVisualizer OpeningVisual { get => _openingVisualizer; set => _openingVisualizer = value; }

    public Wall _parentWall;
    public Wall _lastWall;

    #region Properties
    public float Width
    {
        get => _width;
        set
        {
            _width = value;
            if (_openingVisualizer != null)
            {
                _openingVisualizer.UpdateWidth(_width);
            }
        }
    }
    public float Height { get => _height; set => _height = value; }
    public Vector3 OpeningPosition { get => _openingPosition; set => _openingPosition = value; }
    public OpeningType OpeningType { get => _openingType; set => _openingType = value; }
    public Wall ParentWall => _parentWall;

    #endregion

    private void Awake()
    {
        _strandedOpenings = GameObject.Find("StrandedOpenings");
    }
    public abstract void Initialize(Wall wall, Vector3 worldPosition);

    public void Detach()
    {
        if (_parentWall != null)
        {
            _parentWall._allOpenings.Remove(this);
            _lastWall = _parentWall;
            _parentWall = null;
        }

        // Move into stranded container
        if (StrandedOpening != null)
        {
            transform.SetParent(StrandedOpening, true);
        }
    }

    protected void CalculateAndSetNormalizedPosition(Vector3 worldPosition)
    {
        if (_parentWall == null) return;

        Vector3 wallStart = _parentWall.GetStartPosition();
        Vector3 wallEnd = _parentWall.GetEndPosition();

        Vector3 wallVector = wallEnd - wallStart;
        Vector3 openingVector = worldPosition - wallStart;

        float distanceAlongWall = Vector3.Dot(openingVector, wallVector.normalized);

        if (wallVector.magnitude > 0.01f)
        {
            _normalizedPosition = distanceAlongWall / wallVector.magnitude;
        }
        else
        {
            _normalizedPosition = 0f;
        }
    }


    public void UpdatePositionAndRotation()
    {
        if (_parentWall == null) return;

        Vector3 wallStart = _parentWall.GetStartPosition();
        Vector3 wallEnd = _parentWall.GetEndPosition();
        Vector3 wallVector = wallEnd - wallStart;
        Vector3 newWorldPosition = wallStart + wallVector * _normalizedPosition;
        OpeningPosition = new Vector3(newWorldPosition.x, 3f, newWorldPosition.z);
        transform.position = OpeningPosition;

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
}