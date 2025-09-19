using UnityEngine;


public class PlaceableObject : MonoBehaviour, IPlacables
{
    public enum PlacementType
    {
        Ground,
        Wall
    }

    [Tooltip("Determines if the object should stick to the ground or to walls.")]
    public PlacementType Type = PlacementType.Ground;

    [Tooltip("The vertical offset from the floor (e.g., for objects whose pivot isn't at the very bottom).")]
    public float GroundOffset = 0f;
}