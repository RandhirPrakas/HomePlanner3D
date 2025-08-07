using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public TouchManager _touchManager;

    [SerializeField] private CameraStateManager _cameraStateManager;
    [SerializeField] private SubStateManager _subStateManager;


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

    private void Update()
    {
        _cameraStateManager.Update();
        _subStateManager.Update();

        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(_subStateManager.GetCurrentSubState()?.GetType() == typeof(DrawRoomState))
            {
                _subStateManager.SetSubState(new EditRoomPointsState());
            }
            else if(_subStateManager.GetCurrentSubState()?.GetType() == typeof(EditRoomPointsState))
            {
                _subStateManager.SetSubState(new DrawRoomState());
            }
        }
        else if(Input.GetKeyDown(KeyCode.KeypadEnter))
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
        _subStateManager.SetSubState(new DrawRoomState());

        /*_cameraStateManager = new CameraStateManager();
        _subStateManager = new SubStateManager();*/
    }

    // just for testing 
    private ProceduarlwallGenerator _wallGenerator;
    private void GenerateWalls()
    {
        if(_wallGenerator == null)
        {
            _wallGenerator = new ProceduarlwallGenerator();
        }

        foreach(Room room in RoomManager.Instance._allRooms)
        {
            for(int i=0;i<room._allRoomWalls.Count;i++)
            {
                Wall wall = room._allRoomWalls[i];
                _wallGenerator.MapAllRequiredPoints(wall.GetStartPosition(), wall.GetEndPosition(),wall.gameObject.transform);
            }
        }
    }
}
