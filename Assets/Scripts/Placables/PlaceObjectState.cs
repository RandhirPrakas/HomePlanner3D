using Unity.VisualScripting;
using UnityEngine;

public class PlaceObjectState : ICameraSubState
{
    private readonly OrthoCam _orthoCam;
    private readonly GameObject _prefabToPlace;
    private readonly PlaceableObject _placeableData;
  

    // --- Preview Visuals ---
    private GameObject _placeholderInstance;
    private Material _validPlacementMaterial;
    private Material _invalidPlacementMaterial;
    private Renderer[] _placeholderRenderers;
    private bool _isPlacementValid = false;
  
    // --- Separate Variables
  //  private WorldCanvasHandler _worldCanvasHandler = null;
    private Vector3 clonedPosition = Vector3.zero;
    private Quaternion clonedRotation = Quaternion.identity;
    private Vector3 clonedScale = Vector3.one;
  
    
    /// <summary>
    /// Initializes the placement state with a specific object prefab.
    /// </summary>
    /// <param name="orthoCam">The camera controller.</param>
    /// <param name="prefab">The GameObject prefab to be placed.</param>
    public PlaceObjectState(OrthoCam orthoCam, GameObject prefab = null)
    {
        if (prefab == null)
        {
            GameManager.Instance.GetSubStateManager().SetOrthoIdleState();
            return;
        }

        _orthoCam = orthoCam;
        _prefabToPlace = prefab;
        _placeableData = prefab.GetComponent<PlaceableObject>();

        // Load materials for visual feedback
        _validPlacementMaterial = Resources.Load<Material>("ProceduralMaterials/ValidPlacement");
        _invalidPlacementMaterial = Resources.Load<Material>("ProceduralMaterials/InvalidPlacement");

        // Added World Canvas to Furniture
       // ANAND WROTE
        SetWorldCanvasUI();
    }
    
    /// <summary>
    /// Initializes the placement state with a specific object prefab.
    /// </summary>
    /// <param name="orthoCam">The camera controller.</param>
    /// <param name="prefab">The GameObject prefab to be placed.</param>
    public PlaceObjectState(OrthoCam orthoCam, GameObject prefab , Vector3 position, Quaternion rotation, Vector3 scale)
    {
        if (prefab == null)
        {
            GameManager.Instance.GetSubStateManager().SetOrthoIdleState();
            return;
        }

        _orthoCam = orthoCam;
        _prefabToPlace = prefab;
        _placeableData = prefab.GetComponent<PlaceableObject>();
        clonedPosition = position;
        clonedRotation = rotation;
        clonedScale = scale;
        
        // Load materials for visual feedback
        _validPlacementMaterial = Resources.Load<Material>("ProceduralMaterials/ValidPlacement");
        _invalidPlacementMaterial = Resources.Load<Material>("ProceduralMaterials/InvalidPlacement");

        // Added World Canvas to Furniture
        // ANAND WROTE
        SetWorldCanvasUI();
    }


    public void Enter()
    {
        Debug.Log("Entered PlaceObject State");
        if (_placeableData == null)
        {
            Debug.LogError($"The prefab '{_prefabToPlace.name}' is missing the PlaceableObject component!");
            return;
        }

        _placeholderInstance = GameObject.Instantiate(_prefabToPlace);
        _placeholderInstance.name = $"{_prefabToPlace.name}";
        _placeholderInstance.tag = Constants.TAG_PLACABLES;

        // Setting Transform Position, Rotation & Scale
        // -- Position
        if (clonedPosition != Vector3.zero)
            _placeholderInstance.transform.position = clonedPosition;
        // -- Rotation
        if (clonedRotation != Quaternion.identity)
            _placeholderInstance.transform.rotation = clonedRotation;
        // -- Scale
        if (clonedScale != Vector3.one)
            _placeholderInstance.transform.localScale = clonedScale;
       
        foreach (var col in _placeholderInstance.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        _placeholderRenderers = _placeholderInstance.GetComponentsInChildren<Renderer>();
        SetPlaceholderMaterial(_invalidPlacementMaterial);
        _isPlacementValid = false;

        // Attaching the UI with instantiated Furniture
       // _worldCanvasHandler._selectedObject = _placeholderInstance;
    }

    public void Exit()
    {
        Debug.Log("Exiting PlaceObject State");
        if (_placeholderInstance != null)
        {
            GameObject.Destroy(_placeholderInstance);
            //GameObject.Destroy(_worldCanvasHandler.gameObject);
        }
    }

    public void OnTouchStart(Vector3 worldPos, Vector2 screenPos)
    {
        // if (_worldCanvasHandler == null)
        // {
        //     SetWorldCanvasUI();
        // }

        UpdatePlaceholderPosition(screenPos);
    }

    public void OnTouchHold(Vector3 worldPos, Vector2 screenPos)
    {
        UpdatePlaceholderPosition(screenPos);
    }

    public void OnTouchEnd(Vector3 worldPos, Vector2 screenPos)
    {
        UpdatePlaceholderPosition(screenPos);

        if (_isPlacementValid)
        {
            Debug.Log("Placing object at final position.");
            GameObject go = GameObject.Instantiate(_prefabToPlace, _placeholderInstance.transform.position, _placeholderInstance.transform.rotation);
            go.tag = Constants.TAG_PLACABLES;
        }
        else
        {
            Debug.Log("Invalid placement position. Action cancelled.");
        }

        if (GameManager.Instance.GetCameraStateManager().GetCurrentState() is OrthographicState)
            GameManager.Instance.GetSubStateManager().SetOrthoIdleState();
        else
            GameManager.Instance.GetSubStateManager().SetPerspIdleState();
    }

    public void Update()
    {
        _orthoCam.Update();
    }

    public void OnPinch(float delta)
    {
        _orthoCam.ZoomCamera(delta);
    }

    private void UpdatePlaceholderPosition(Vector2 screenPos)
    {
        if(_placeableData.IsLock)
            return;
        
        // Hiding The WorldCanvasUI as soon as movement Start
      //  _worldCanvasHandler?.Hide();
        
        if (_placeholderInstance == null) return;
        
        Ray ray = Camera.main.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            bool wasPlacementValid = false;

            // Check placement based on the object's defined type
            if (_placeableData.Type == PlaceableObject.PlacementType.Ground && hit.collider.CompareTag("Room"))
            {
                _placeholderInstance.transform.position = hit.point + new Vector3(0, _placeableData.GroundOffset, 0);
                // Keep ground objects upright
                _placeholderInstance.transform.rotation = Quaternion.identity;
                wasPlacementValid = true;
            }
            else if (_placeableData.Type == PlaceableObject.PlacementType.Wall && hit.collider.CompareTag("Wall"))
            {
                _placeholderInstance.transform.position = hit.point;
                // Align the object to the wall surface
                _placeholderInstance.transform.rotation = Quaternion.LookRotation(-hit.normal);
                wasPlacementValid = true;
            }

            // Update visual feedback
            if (wasPlacementValid && !_isPlacementValid)
            {
                SetPlaceholderMaterial(_validPlacementMaterial);
                _isPlacementValid = true;
            }
            else if (!wasPlacementValid && _isPlacementValid)
            {
                SetPlaceholderMaterial(_invalidPlacementMaterial);
                _isPlacementValid = false;
            }
            _placeholderInstance.SetActive(wasPlacementValid);
        }
        else
        {
            // If raycast hits nothing, it's an invalid position
            if (_isPlacementValid)
            {
                SetPlaceholderMaterial(_invalidPlacementMaterial);
                _isPlacementValid = false;
            }
            _placeholderInstance.SetActive(false);
        }
    }

    private void SetPlaceholderMaterial(Material mat)
    {
        if (mat == null) return;
        foreach (var renderer in _placeholderRenderers)
        {
            renderer.material = mat;
        }
    }
    /// <summary>
    /// This method to initialze World Canvas for placed object. Called from PlacedObjectState.
    /// </summary>
    private void SetWorldCanvasUI()
    {
        // if (_worldCanvasHandler == null)
        // {
        //     // Instantiate and parent under the wall point
        //     _worldCanvasHandler = GameObject.Instantiate(
        //         GameManager.Instance._uiManager.worldCanvasHandlerPlacedObject,
        //         Vector3.zero,
        //         Quaternion.identity,
        //         null
        //     );
        //     _worldCanvasHandler.gameObject.name = "WorldCanvas";
        // }
        //
        // if (_placeholderInstance != null)
        // {
        //     _worldCanvasHandler._selectedObject = _placeholderInstance;
        // }
    }

    // This method is not part of the core logic but is required by the interface.
    public void Init(Vector3 worldPos, Vector2 screenPos) { }
}