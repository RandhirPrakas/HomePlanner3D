using UnityEngine;

public class OrthographicState : CameraState
{
    public override void Enter()
    {
        Debug.Log("Switched to Orthographic Mode");
        Camera.main.orthographic = true;
    }

    public override void Exit()
    {
        Debug.Log("Exiting Orthographic Mode");
    }
}
