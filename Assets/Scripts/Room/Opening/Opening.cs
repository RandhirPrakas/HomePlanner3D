using System.Collections.Generic;
using UnityEngine;

public abstract class Opening : MonoBehaviour
{

    [SerializeField] private OpeningType _openingType = OpeningType.Door;
    [SerializeField] private Vector3 _openingPosition;
    [SerializeField] private float _width = 2f;
    [SerializeField] private float _height = 2f;

    [SerializeField] private GameObject _strandedOpenings;
    public OpeningVisualizer _openingVisualizer;


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
}