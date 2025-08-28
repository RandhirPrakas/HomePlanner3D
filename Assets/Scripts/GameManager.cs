using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public UIManager _uiManager;
    public TouchManager _touchManager;

    [SerializeField] private CameraStateManager _cameraStateManager;
    [SerializeField] private SubStateManager _subStateManager;

    public Wall _activeWall;
    #region Getter and Setter

    public void SetSubState(ICameraSubState sub)
    {
        _subStateManager.SetSubState(sub);
    }

    public ICameraSubState GetSubState()
    {
        return _subStateManager.GetCurrentSubState();
    }

    public CameraState GetCameraState()
    {
        return _cameraStateManager.GetCurrentState();
    }

    public SubStateManager GetSubStateManager()
    {
        return _subStateManager;
    }

    #endregion

    private void Awake()
    {
        // Creating Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        Initialize();
    }

    private void Start()
    {
        _subStateManager.Start();
    }

    private void Update()
    {
        _cameraStateManager.Update();
        _subStateManager.Update();

        // --- New Hotkey System ---

        // ESCAPE KEY: Universal exit to IdleState
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GetSubState()?.GetType() != typeof(IdleState))
            {
                SetSubState(new IdleState());
            }
        }

        // D KEY: Enter DrawRoomState from Idle
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (GetSubState()?.GetType() == typeof(IdleState))
            {
                // Start a new room or get the most recent one
                Room room = (RoomManager.Instance._allRooms == null || RoomManager.Instance._allRooms.Count == 0) ? null : RoomManager.Instance._activeRoom;
                SetSubState(new DrawRoomState(room));
            }
        }

        // E KEY: Enter EditRoomPointsState from Idle
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (GetSubState()?.GetType() == typeof(IdleState))
            {
                SetSubState(new EditRoomPointsState());
            }
        }

        // A KEY: Enter AddDoorState from Idle
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (GetSubState()?.GetType() == typeof(IdleState))
            {
                // Note: This requires a better system for selecting a wall.
                // For now, it just grabs the first available wall for testing.
                if (RoomManager.Instance._allRooms.Count > 0 && RoomManager.Instance._allRooms[0]._allRoomWalls.Count > 0)
                {
                    Wall wall = RoomManager.Instance._allRooms[0]._allRoomWalls[0];
                    SetSubState(new AddDoorState(wall));
                }
                else
                {
                    Debug.LogWarning("Cannot enter AddDoorState. No walls exist to add a door to!");
                }
            }
        }

        // O KEY: Toggle Camera State (Orthographic/Perspective)
        if (Input.GetKeyDown(KeyCode.O))
        {
            if (GetCameraState()?.GetType() == typeof(OrthographicState))
            {
                _cameraStateManager.SetCameraState(new PerspectiveState());
            }
            else if (GetCameraState()?.GetType() == typeof(PerspectiveState))
            {
                _cameraStateManager.SetCameraState(new OrthographicState());
            }
        }

        // ENTER KEY: Generate the final 3D walls
        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
        {
            GenerateWalls();
        }
    }

    private void Initialize()
    {
        if (_touchManager == null)
            _touchManager = FindObjectOfType<TouchManager>();

        InitStates();
    }

    void InitStates()
    {
        _cameraStateManager.SetCameraState(new OrthographicState());
        // Start in the safe IdleState
        _subStateManager.SetSubState(new IdleState());
    }

    private ProceduarlwallGenerator _wallGenerator;
    public void GenerateWalls()
    {
        if (_wallGenerator == null)
        {
            _wallGenerator = new ProceduarlwallGenerator();
        }

        foreach (Room room in RoomManager.Instance._allRooms)
        {
            for (int i = 0; i < room._allRoomWalls.Count; i++)
            {
                Wall wall = room._allRoomWalls[i];
                _wallGenerator.MapAllRequiredPoints(wall.GetStartPosition(), wall.GetEndPosition(), wall.gameObject.transform);
            }
        }
    }
}
