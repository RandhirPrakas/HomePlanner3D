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
    [SerializeField] private CatalogManager _catalogManager;

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

    public CatalogManager GetCatalogManager()
    {
        return _catalogManager;
    }

    #endregion

    #region For Time Being

    public bool _perspCamActive = false;
    public bool _placeObjects = false;
    public bool _roomMovement = false;
    public bool _window = false;
    public bool _editOpening = false;

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
        LoadEssentialAssets();
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

        _perspCam = Camera.main.GetComponent<PerspCam>();
        _perspCam._mainCamera = Camera.main;
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


    // Load essential assets from the addressebles
    
    public void LoadEssentialAssets()
    {
        // Door Visualizer
        AddressableLoader.LoadAndAssign<GameObject>(Constants.PATH_DOOR_VISUALIZER, go => Constants.DOOR_VISUALIZER = go);
        
        // Window Visualizer
        AddressableLoader.LoadAndAssign<GameObject>(Constants.PATH_WINDOW_VISUALIZER, go => Constants.WINDOW_VISUALIZER = go);

        // Object Distance Label Prefab
        AddressableLoader.LoadAndAssign<GameObject>(Constants.PATH_OBJECT_DISTANCE_LABEL_PREFAB, go => Constants.OBJECT_DISTANCE_LABEL_PREFAB = go);


        // Default Wall Length Label
        AddressableLoader.LoadAndAssign<GameObject>(
            Constants.PATH_WALL_LENGTH_LABEL,
            go =>
            {
                Constants.DEFAULT_WALL_LENGTH_LABEL = go.GetComponentInChildren<TMP_Text>();
            });

        // Default LR Material
        AddressableLoader.LoadAndAssign<Material>(Constants.PATH_DEFAULT_LR_MATERIAL, mat =>
        {
            Constants.DEFAULT_LINERENDERER_MATERIAL = mat;
        });

        // Default Floor Material
        AddressableLoader.LoadAndAssign<Material>(Constants.PATH_DEFAULT_FLOOR_MATERIAL, mat => {
            Constants.DEFAULT_FLOOR_MATERIAL = mat;
        });

        // Default Quad Material
        AddressableLoader.LoadAndAssign<Material>(Constants.PATH_DEFAULT_QUAD_MATERIAL, mat => {
            Constants.DEFAULT_QUAD_MATERIAL = mat;
        });

        // default hightlighted wall
        AddressableLoader.LoadAndAssign<Material>(Constants.PATH_HIGHLIGHTED_WALL_MATERIAL, mat => {
            Constants.DEFAULT_HIGHLIGHTED_WALL_MATERIAL= mat;
        });

        // Invalid Placement
        AddressableLoader.LoadAndAssign<Material>(Constants.PATH_INVALID_PLACAMENT_MATERIAL, mat => {
            Constants.DEFAULT_INVALID_PLACAMENT_MATERIAL = mat;
        });

        // valid Placement
        AddressableLoader.LoadAndAssign<Material>(Constants.PATH_VALID_PLACAMENT_MATERIAL, mat => {
            Constants.DEFAULT_VALID_PLACAMENT_MATERIAL = mat;
        });

        // Object Distance
        AddressableLoader.LoadAndAssign<Material>(Constants.PATH_OBJECT_DISTANCE_LR_MATERIAL, mat => {
            Constants.DEFAULT_OBJECT_DISTANCE_MATERIAL = mat;
        });
    }
}
