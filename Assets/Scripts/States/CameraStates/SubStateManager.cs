using UnityEngine;

[System.Serializable]
public class SubStateManager
{
    private ICameraSubState _currentSubState;

    private ICameraSubState _perspIdleState;
    private ICameraSubState _orthoIdleState;
    private EditRoomPointsState _editPointState;

    private OrthoCam _orthoCam;
    public OrthoCam OrthoCamera { get => _orthoCam; set => _orthoCam = value; }

    private float _inputBlockTime = 0f;
    public void SetSubState(ICameraSubState newState)
    {
        _currentSubState?.Exit();
        _currentSubState = newState;
        _currentSubState.Enter();

        _inputBlockTime = Time.time + 0.2f;
    }

    public ICameraSubState GetCurrentSubState() => _currentSubState;

    public void Start()
    {
        AppEventHandler.OnTouchEnd += SwitchSubstate;

        OrthoCamera = GameManager.Instance.GetOrthoCamera();

        _perspIdleState = new Persp_IdleState();
        _orthoIdleState = new Ortho_IdleState(_orthoCam);
        _editPointState = new EditRoomPointsState(_orthoCam);
    }

    public void Update()
    {
        if (Time.time < _inputBlockTime) return;
        _currentSubState?.Update();
    }

    public void SetOrthoIdleState()
    {
        if (_currentSubState == _orthoIdleState) return;
        SetSubState(_orthoIdleState);
    }

    public void SetPerspIdleState()
    {
        if (_currentSubState == _perspIdleState) return;
        SetSubState(_perspIdleState);
    }

    public void SetEditPointState()
    {
        if (_currentSubState == _editPointState) return;
        SetSubState(_editPointState);
    }

    public void SetDrawRoomState()
    {
        SetSubState(new DrawRoomState(OrthoCamera));
    }

    private void SwitchSubstate(GameObject gameObject, Vector3 worldPos, Vector2 screenPos)
    {
        if (Time.time < _inputBlockTime) return;
        if (GameManager.Instance.GetCameraStateManager().GetCurrentState() is OrthographicState)
        {
#if UNITY_EDITOR
            ClearConsole();
#endif
            if (gameObject.CompareTag("Door"))
            {
                SetSubState(new EditDoorState(_orthoCam));
                return;
            }
            else if(gameObject.CompareTag("Window"))
            {
                SetSubState(new EditWindowState(_orthoCam));
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
                    moveWallState.SetActiveWall(wall);
                }
                else
                {
                    // not in MoveWallState → enter new state
                    SetSubState(new MoveWallState(wall, _orthoCam));
                }
            }
        }
        else if(GameManager.Instance.GetCameraStateManager().GetCurrentState() is PerspectiveState)
        {
/*#if UNITY_EDITOR
            ClearConsole();
#endif
            Debug.Log(gameObject.name);

            if (gameObject.CompareTag("Door") || gameObject.CompareTag("Window"))
            {
                SetSubState(new EditOpeningIn3DState(Camera.main));
            }*/
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
