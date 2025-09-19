using UnityEngine;

public class Persp_IdleState : ICameraSubState
{
    PerspCam _perspCam;

    public Persp_IdleState(PerspCam perspCam)
    {
        _perspCam = perspCam;
    }

    /*public void Enter()
    {
        _perspCam = GameManager.Instance.GetPerspCam();
    }

    public void Exit()
    {

    }

    public void Init(Vector3 worldPos, Vector2 screenPos)
    {
        _perspCam.SetInitialTouchPosition(screenPos);
    }

    public void OnTouchStart(Vector3 worldPos, Vector2 screenPos)
    {
        _perspCam.SetInitialTouchPosition(screenPos);
    }

    public void OnTouchHold(Vector3 worldPos, Vector2 screenPos)
    {
        _perspCam.RotateCamera(screenPos);
    }

    public void OnTouchEnd(Vector3 worldPos, Vector2 screenPos) { }

    public void OnPinch(float delta)
    {
        _perspCam.ZoomCamera(delta);
    }

    public void Update()
    {
        _perspCam.UpdateCamera();
    }*/
    public void Enter()
    {
        //throw new System.NotImplementedException();
    }

    public void Exit()
    {
        //throw new System.NotImplementedException();
    }

    public void Init(Vector3 worldPos, Vector2 screenPos)
    {
        //throw new System.NotImplementedException();
    }

    public void OnPinch(float delta)
    {
        //throw new System.NotImplementedException();
    }

    public void OnTouchEnd(Vector3 worldPos, Vector2 screenPos)
    {
        //throw new System.NotImplementedException();
    }

    public void OnTouchHold(Vector3 worldPos, Vector2 screenPos)
    {
        //throw new System.NotImplementedException();
    }

    public void OnTouchStart(Vector3 worldPos, Vector2 screenPos)
    {
        //throw new System.NotImplementedException();
    }

    public void Update()
    {
        //throw new System.NotImplementedException();
    }
}
