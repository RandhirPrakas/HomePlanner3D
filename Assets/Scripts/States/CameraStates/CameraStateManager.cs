
using System;

[System.Serializable]
public class CameraStateManager
{
    private CameraState currentCameraState;

    public void SetCameraState(CameraState newState)
    {
        currentCameraState?.Exit();
        currentCameraState = newState;
        currentCameraState.Enter();
    }

    public CameraState GetCurrentState()
    {
        return currentCameraState;
    }

    public void Update()
    {
        currentCameraState?.Update();
    }

    public void ToggleCamera()
    {
        if (currentCameraState is OrthographicState)
            SetCameraState(new PerspectiveState());
        else
            SetCameraState(new OrthographicState());
    }
}
