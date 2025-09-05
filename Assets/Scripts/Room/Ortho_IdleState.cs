using UnityEngine;

public class Ortho_IdleState : ICameraSubState
{
    OrthoCam _orthoCam;

    public void Enter()
    {
        Debug.Log("Entered IdleState");
        GameManager.Instance._uiManager.SetDrawButtonActive(true);
        _orthoCam = GameManager.Instance.GetOrthoCamera();
    }

    public void Exit()
    {
        GameManager.Instance._uiManager.SetDrawButtonActive(false);
    }

    public void Init(Vector3 worldPos, Vector2 screenPos)
    {
        _orthoCam.SetInitialTouchPosition(screenPos);
    }

    public void OnTouchEnd(Vector3 worldPos, Vector2 screenPos)
    {
        throw new System.NotImplementedException();
    }

    public void OnTouchHold(Vector3 worldPos, Vector2 screenPos)
    {
        Vector3 distance = _orthoCam.GetDistance(screenPos);
        _orthoCam.MoveCameraByDistance(distance);
        _orthoCam.SetInitialTouchPosition(screenPos);
    }

    public void OnTouchStart(Vector3 worldPos, Vector2 screenPos)
    {
        _orthoCam.SetInitialTouchPosition(screenPos);
    }

    public void Update()
    {
        //throw new System.NotImplementedException();
    }
}
