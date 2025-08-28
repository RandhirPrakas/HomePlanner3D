using UnityEngine;

public class IdleState : ICameraSubState
{
    public IdleState()
    {
        
    }

    public void Enter()
    {
        Debug.Log("Entered IdleState");
        GameManager.Instance._uiManager.SetDrawButtonActive(true);
    }

    public void Exit()
    {
        Debug.Log("Exited IdleState");
    }

    public void Update()
    {
        // Nothing happens in idle state
    }

    public void Init(Vector3 worldPos, Vector2 screenPos)
    {
        // No initialization required for idle
    }

    public void OnTouchStart(Vector3 worldPos, Vector2 screenPos)
    {
        // Ignore touches in idle mode
    }

    public void OnTouchHold(Vector3 worldPos, Vector2 screenPos)
    {
        // Ignore touches in idle mode
    }

    public void OnTouchEnd(Vector3 worldPos, Vector2 screenPos)
    {
        // Ignore touches in idle mode
    }
}
