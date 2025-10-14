using UnityEngine;

public class ResizeHandle : MonoBehaviour
{
    public Opening ownerOpening;
    public CornerType corner;
}

public enum CornerType
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
    midLeft,
    midRight
}