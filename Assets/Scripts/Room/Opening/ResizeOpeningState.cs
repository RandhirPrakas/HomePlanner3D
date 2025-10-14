using UnityEngine;

public class ResizeOpeningState<T> : ICameraSubState where T : Opening
{
    private readonly T _opening;
    private readonly ResizeHandle _handle;
    private readonly Camera _camera;
    private readonly Plane _resizePlane;
    private Vector3 _lastFrameDragPosition;

    public ResizeOpeningState(T opening, ResizeHandle handle, Camera camera, Vector2 screenPos)
    {

    }

    public void OnTouchHold(Vector3 worldPos, Vector2 screenPos)
    {
       
    }

    public void OnTouchEnd(Vector3 worldPos, Vector2 screenPos)
    {
        
    }


    // Other ICameraSubState methods (can be left empty)
    public void Enter() { }
    public void Exit() { }
    public void Init(Vector3 worldPos, Vector2 screenPos) { }
    public void Update() { }
    public void OnPinch(float delta) { }

    public void OnTouchStart(Vector3 worldPos, Vector2 screenPos)
    {
        
    }
}