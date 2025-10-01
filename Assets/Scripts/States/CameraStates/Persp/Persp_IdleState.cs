using UnityEngine;

public class Persp_IdleState : ICameraSubState
{
    PerspCam _perspCam;

    public Persp_IdleState(PerspCam perspCam)
    {
        if(GameManager.Instance._perspCamActive)
            _perspCam = perspCam;
    }
    public void Enter()
    {
        Debug.Log("Entered Persp_Idle State");

        if (GameManager.Instance._perspCamActive)
            _perspCam = GameManager.Instance.GetPerspCam();
    }

    public void Exit()
    {
        Debug.Log("Exiting Persp_Idle State");
    }

    public void Init(Vector3 worldPos, Vector2 screenPos)
    {
        if (GameManager.Instance._perspCamActive)
            _perspCam.SetInitialTouchPosition(screenPos);
    }

    public void OnTouchStart(Vector3 worldPos, Vector2 screenPos)
    {
        if (GameManager.Instance._perspCamActive)
            _perspCam.SetInitialTouchPosition(screenPos);
    }

    public void OnTouchHold(Vector3 worldPos, Vector2 screenPos)
    {
        if (GameManager.Instance._perspCamActive)
            _perspCam.RotateCamera(screenPos);
    }

    public void OnTouchEnd(Vector3 worldPos, Vector2 screenPos) { }

    public void OnPinch(float delta)
    {
        if (GameManager.Instance._perspCamActive)
            _perspCam.ZoomCamera(delta);
    }

    public void Update()
    {
        if (GameManager.Instance._perspCamActive)
            _perspCam.UpdateCamera();
    }

    public void OnPan(Vector2 delta)
    {
        if (GameManager.Instance._perspCamActive)
            _perspCam.PanCameraByDelta(delta);
    }

}
