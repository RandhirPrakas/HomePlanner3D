using UnityEngine;

public class Ortho_IdleState : ICameraSubState
{

    public void Enter()
    {
        Debug.Log("Entered IdleState");
        GameManager.Instance._uiManager.SetDrawButtonActive(true);
    }

    public void Exit()
    {
        GameManager.Instance._uiManager.SetDrawButtonActive(false);
    }

    public void Init(Vector3 worldPos, Vector2 screenPos)
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
