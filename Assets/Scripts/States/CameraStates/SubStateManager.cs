using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class SubStateManager
{
    private ICameraSubState _currentSubState;

    private ICameraSubState _perspIdleState = new Persp_IdleState();
    private ICameraSubState _orthoIdleState = new Ortho_IdleState();
    private EditRoomPointsState _editPointState = new EditRoomPointsState();

    #region Getter And Setter


    #endregion

    public void SetSubState(ICameraSubState newState)
    {
        _currentSubState?.Exit();
        _currentSubState = newState;
        _currentSubState.Enter();
    }

    public ICameraSubState GetCurrentSubState()
    {
        return _currentSubState;
    }

    public void Start()
    {
        AppEventHandler.OnTouchEnd += SwitchSubstate;
    }

    public void Update()
    {
        _currentSubState?.Update();
    }

    public void SetOrthoIdleState()
    {
        if (_currentSubState == _orthoIdleState)
            return;
        SetSubState(_orthoIdleState);
    }

    public void SetPerspIdleState()
    {
        if (_currentSubState == _perspIdleState)
            return;
        SetSubState(_perspIdleState);
    }

    public void SetEditPointState()
    {
        if (_currentSubState == _editPointState)
            return;
        SetSubState(_editPointState);
    }

    public void SetDrawRoomState()
    {
        SetSubState(new DrawRoomState());
    }

    private void SwitchSubstate(GameObject gameObject, Vector3 worldPos, Vector2 screenPos)
    {
        if (GameManager.Instance.GetCameraStateManager().GetCurrentState() is OrthographicState)
        {
            ClearConsole();
            if (gameObject.CompareTag("Door"))
            {
                SetSubState(new EditDoorState());
                return;
            }

            if (gameObject.CompareTag("Ground") || gameObject.CompareTag("Wall"))
            {
                foreach (WallPoint wp in WallPointManager.Instance._allWallPoints)
                {
                    if ((worldPos - wp._position).magnitude <= 5f)
                    {
                        SetEditPointState();
                        return;
                    }
                }
            }

            if (gameObject.CompareTag("Ground"))
            {
                SetOrthoIdleState();
            }
            else if (gameObject.CompareTag("Wall"))
            {
                Wall wall = gameObject.GetComponentInParent<Wall>();
                var currentState = GetCurrentSubState();

                if (currentState is MoveWallState moveWallState)
                {
                    // already in MoveWallState → just change active wall
                    //moveWallState.SetActiveWall(wall);
                }
                else
                {
                    // not in MoveWallState → enter new state
                    SetSubState(new MoveWallState(wall));
                }
            }
        }

    }

    #region For Dev Purpose

    public static void ClearConsole()
    {
        var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
        var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        clearMethod.Invoke(null, null);
    }

    #endregion
}
