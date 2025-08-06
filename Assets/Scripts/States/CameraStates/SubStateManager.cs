
[System.Serializable]
public class SubStateManager
{
    private ICameraSubState currentSubState;

    public void SetSubState(ICameraSubState newState)
    {
        currentSubState?.Exit();
        currentSubState = newState;
        currentSubState.Enter();
    }

    public ICameraSubState GetCurrentSubState()
    {
        return currentSubState;
    }

    public void Update()
    {
        currentSubState?.Update();
    }
}
