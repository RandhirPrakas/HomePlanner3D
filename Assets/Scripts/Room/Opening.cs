using UnityEngine;

public class Opening : MonoBehaviour
{
    [SerializeField] private OpeningType _openingType = OpeningType.Door;
    [SerializeField] private Vector3 _openingPosition; // local position along the wall
    [SerializeField] private float _width = 1f;
    [SerializeField] private float _height = 2f;

    private Wall _parentWall;

    #region Properties
    public float Width { get => _width; set => _width = value; }
    public float Height { get => _height; set => _height = value; }
    public Vector3 OpeningPosition { get => _openingPosition; set => _openingPosition = value; }
    public OpeningType OpeningType { get => _openingType; set => _openingType = value; }
    public Wall ParentWall => _parentWall;
    #endregion

    /// <summary>
    /// Initialize the opening on a given wall at a position.
    /// </summary>
    public void Initialize(Wall wall, Vector3 worldPosition, OpeningType type = OpeningType.Door)
    {
        _parentWall = wall;
        _openingType = type;

        // Convert world position into local space of the wall
        _openingPosition = wall.transform.InverseTransformPoint(worldPosition);

        // Attach opening to wall
        transform.SetParent(wall.transform, worldPositionStays: true);

        // Add to wall's list if not already there
        if (!wall._allOpenings.Contains(this))
            wall._allOpenings.Add(this);
    }
}