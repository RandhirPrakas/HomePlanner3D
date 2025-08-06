using UnityEngine;

public class OrthographicState : CameraState
{
    public override void Enter()
    {
        Camera.main.orthographic = true;
        Debug.Log("Switched to Orthographic Mode");
    }

    public override void Exit()
    {
        Debug.Log("Exiting Orthographic Mode");
    }
}
