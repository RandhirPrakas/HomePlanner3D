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
    
    [Tooltip("Handle the mobility of placed object (for example whether the object can be moved on not?)")]
    private bool isLock = false;
    public bool IsLock
    {
        get => isLock;
        set => isLock = value;
    }
}