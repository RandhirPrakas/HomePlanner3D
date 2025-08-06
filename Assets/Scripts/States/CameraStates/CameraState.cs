public abstract class CameraState : ICameraState
{
    protected ICameraSubState currentSubState;

    public abstract void Enter();
    public abstract void Exit();

    public virtual void Update()
    {
        currentSubState?.Update();
    }

    public void SetSubState(ICameraSubState subState)
    {
        currentSubState?.Exit();
        currentSubState = subState;
        currentSubState.Enter();
    }
}
