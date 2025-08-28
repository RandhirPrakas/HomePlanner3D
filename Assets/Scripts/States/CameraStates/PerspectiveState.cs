using UnityEngine;

public class PerspectiveState : CameraState
{
    public override void Enter()
    {
        Camera.main.orthographic = false;
    }

    public override void Exit()
    {
        Debug.Log("Exiting Perspective Mode");
    }
}