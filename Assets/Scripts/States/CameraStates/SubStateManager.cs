using UnityEngine;

[System.Serializable]
public class SubStateManager
{
    private ICameraSubState _currentSubState;

    private ICameraSubState _perspIdleState;
    private ICameraSubState _orthoIdleState;

    private OrthoCam _orthoCam;
    private PerspCam _perspcam;

    public OrthoCam OrthoCamera { get => _orthoCam; set => _orthoCam = value; }
    private PerspCam PerspectiveCam { get => _perspcam; set => _perspcam = value; }

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

        _perspIdleState = new Persp_IdleState(PerspectiveCam);
        _orthoIdleState = new Ortho_IdleState(_orthoCam);
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
            //ClearConsole();
#endif
            if (gameObject.CompareTag(Constants.TAG_DOOR))
            {
                SetSubState(new EditOpeningState<Door>(_orthoCam, gameObject.GetComponentInParent<Door>()));
                return;
            }
            else if(gameObject.CompareTag(Constants.TAG_WINDOW))
            {
                SetSubState(new EditOpeningState<Window>(_orthoCam, gameObject.GetComponentInParent<Window>()));
                return;
            }

            // To Edit Alread placed objects like Chairs/table etcs
            /*if (gameObject.CompareTag(Constants.TAG_PLACABLES))
            {
                SetSubState(new EditObjectState(OrthoCamera, null, gameObject));
                return;
            }*/

            // To Edit the position of the room
            /*if (gameObject.CompareTag(Constants.TAG_ROOM))
            {
                SetSubState(new MoveRoomState(gameObject.GetComponent<Room>(), OrthoCamera));
                return;
            }*/

            if (gameObject.CompareTag(Constants.TAG_GROUND) || gameObject.CompareTag(Constants.TAG_WALL))
            {
                foreach (WallPoint wp in WallPointManager.Instance._allWallPoints)
                {
                    if ((worldPos - wp._position).magnitude <= 5f)
                    {
                        SetSubState(new EditRoomPointsState(_orthoCam, wp));
                        return;
                    }
                }
            }

            if (gameObject.CompareTag(Constants.TAG_GROUND))
            {
                SetOrthoIdleState();
            }
            else if (gameObject.CompareTag(Constants.TAG_WALL))
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
#if UNITY_EDITOR
            ClearConsole();
#endif
            Debug.Log(gameObject.name);

            /*// To Edit Alread placed objects like Chairs/ table etcs
            if(gameObject.CompareTag(Constants.TAG_PLACABLES))
            {
                SetSubState(new EditObjectState(null, PerspectiveCam, gameObject));
            }*/

            /*if (gameObject.CompareTag(Constants.TAG_DOOR) || gameObject.CompareTag(Constants.TAG_WINDOW))
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
