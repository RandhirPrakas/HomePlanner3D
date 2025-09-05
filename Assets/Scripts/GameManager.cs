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
    [SerializeField] private OrthoCam _orthoCam;
    [SerializeField] private PerspCam _perspCam;

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

        
    }

    private void Start()
    {
        _subStateManager.Start();
        Initialize();
    }

    private void Update()
    {
        _cameraStateManager.Update();
        _subStateManager.Update();
    }

    private void Initialize()
    {
        if (_touchManager == null)
            _touchManager = FindObjectOfType<TouchManager>();

        InitCameras();
        InitStates();
    }

    void InitStates()
    {
        _cameraStateManager.SetCameraState(new OrthographicState());
        // Start in the safe IdleState
        _subStateManager.SetOrthoIdleState();
    }

    void InitCameras()
    {
        _orthoCam = Camera.main.GetComponent<OrthoCam>();
        _orthoCam._mainCamera = Camera.main;
    }

    #region camera

    public OrthoCam GetOrthoCamera()
    {
        return _orthoCam;
    }

    public PerspCam GetPerspCam()
    {
        return _perspCam;
    }

    #endregion

}
