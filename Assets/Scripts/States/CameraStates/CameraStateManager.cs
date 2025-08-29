
using System;

[System.Serializable]
public class CameraStateManager
{
    private CameraState _currentCameraState;

    private CameraState _perspectiveCamState = new PerspectiveState();
    private CameraState _orthographicCamState = new OrthographicState();

    public void SetCameraState(CameraState newState)
    {
        _currentCameraState?.Exit();
        _currentCameraState = newState;
        _currentCameraState.Enter();
    }

    public CameraState GetCurrentState()
    {
        return _currentCameraState;
    }

    public void Update()
    {
        _currentCameraState?.Update();
    }

    public void SetPerspectiveState()
    {
        if (_currentCameraState == _perspectiveCamState)
            return;
        SetCameraState(_perspectiveCamState);
    }

    public void SetOrthographicState()
    {
        if (_currentCameraState == _orthographicCamState)
            return;
        SetCameraState(_orthographicCamState);
    }

    public void ToggleCamera()
    {
        if (_currentCameraState is OrthographicState)
            SetPerspectiveState();
        else
            SetOrthographicState();
    }

    
}
