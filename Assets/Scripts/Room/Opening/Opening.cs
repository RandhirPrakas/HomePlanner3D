using System.Collections.Generic;
using UnityEngine;

public abstract class Opening : MonoBehaviour
{
    [SerializeField] private List<Wall> _connectedWalls = new List<Wall>();

    [SerializeField] private OpeningType _openingType = OpeningType.Door;
    [SerializeField] private Vector3 _openingPosition; // local position along the wall
    [SerializeField] private float _width = 2f;
    [SerializeField] private float _height = 2f;

    public Wall _parentWall;

    #region Properties
    public float Width { get => _width; set => _width = value; }
    public float Height { get => _height; set => _height = value; }
    public Vector3 OpeningPosition { get => _openingPosition; set => _openingPosition = value; }
    public OpeningType OpeningType { get => _openingType; set => _openingType = value; }
    public Wall ParentWall => _parentWall;

    public List<Wall> ConnectedWall { get => _connectedWalls; }
    #endregion

    /// <summary>
    /// Initialize the opening on a given wall at a position.
    /// </summary>
    public abstract void Initialize(Wall wall, Vector3 worldPosition);
}