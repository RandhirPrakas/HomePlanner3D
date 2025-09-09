using UnityEngine;

public class Ortho_IdleState : ICameraSubState
{
    OrthoCam _orthoCam;

    public Ortho_IdleState(OrthoCam orthoCam)
    {
        _orthoCam = orthoCam;
    }

    public void Enter()
    {
        Debug.Log("Entered IdleState");
        GameManager.Instance._uiManager.SetDrawButtonActive(true);
        _orthoCam = GameManager.Instance.GetOrthoCamera();
    }

    public void Exit()
    {
        
    }

    public void Init(Vector3 worldPos, Vector2 screenPos)
    {
        _orthoCam.SetInitialTouchPosition(screenPos);
    }

    public void OnTouchEnd(Vector3 worldPos, Vector2 screenPos)
    {

    }

    public void OnTouchHold(Vector3 worldPos, Vector2 screenPos)
    {
        Vector3 distance = _orthoCam.GetDistance(screenPos);
        _orthoCam.MoveCameraByDistance(distance, screenPos);
        _orthoCam.SetInitialTouchPosition(screenPos);
    }

    public void OnTouchStart(Vector3 worldPos, Vector2 screenPos)
    {
        _orthoCam.SetInitialTouchPosition(screenPos);
    }

    public void OnPinch(float delta)
    {
        _orthoCam.ZoomCamera(delta);
    }

    public void Update()
    {
        _orthoCam.Update();
    }
}
