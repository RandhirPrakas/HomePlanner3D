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


    public SubStateManager GetSubStateManager()
    {
        return _subStateManager;
    }

    public CameraStateManager GetCameraStateManager()
    {
        return _cameraStateManager;
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
            if (GetSubState()?.GetType() != typeof(Ortho_IdleState))
            {
                SetSubState(new Ortho_IdleState());
            }
        }

       
        // E KEY: Enter EditRoomPointsState from Idle
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (GetSubState()?.GetType() == typeof(Ortho_IdleState))
            {
                SetSubState(new EditRoomPointsState());
            }
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
        _subStateManager.SetOrthoIdleState();
    }

}
