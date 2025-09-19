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

    /// <summary>
    /// Initializes the placement state with a specific object prefab.
    /// </summary>
    /// <param name="orthoCam">The camera controller.</param>
    /// <param name="prefab">The GameObject prefab to be placed.</param>
    public PlaceObjectState(OrthoCam orthoCam, GameObject prefab = null)
    {
        if (prefab == null)
        {
            prefab = Resources.Load<GameObject>("Test/Cube");
        }
        _orthoCam = orthoCam;
        _prefabToPlace = prefab;
        _placeableData = prefab.GetComponent<PlaceableObject>();

        // Load materials for visual feedback
        _validPlacementMaterial = Resources.Load<Material>("ProceduralMaterials/ValidPlacement");
        _invalidPlacementMaterial = Resources.Load<Material>("ProceduralMaterials/InvalidPlacement");
    }

    public void Enter()
    {
        Debug.Log("Entered PlaceObject State");
        if (_placeableData == null)
        {
            Debug.LogError($"The prefab '{_prefabToPlace.name}' is missing the PlaceableObject component!");
            // Optionally, transition back to idle state here
            return;
        }

        // Create a placeholder instance to move around
        _placeholderInstance = GameObject.Instantiate(_prefabToPlace);
        _placeholderInstance.name = "Placement_Placeholder";

        // Disable colliders on the placeholder to prevent raycast interference
        foreach (var col in _placeholderInstance.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        // Store renderers for changing material
        _placeholderRenderers = _placeholderInstance.GetComponentsInChildren<Renderer>();
        SetPlaceholderMaterial(_invalidPlacementMaterial);
        _isPlacementValid = false;
    }

    public void Exit()
    {
        Debug.Log("Exiting PlaceObject State");
        // Clean up the placeholder when the state exits
        if (_placeholderInstance != null)
        {
            GameObject.Destroy(_placeholderInstance);
        }
    }

    public void OnTouchStart(Vector3 worldPos, Vector2 screenPos)
    {
        // Immediately try to place on touch start
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
            GameObject.Instantiate(_prefabToPlace, _placeholderInstance.transform.position, _placeholderInstance.transform.rotation);
        }
        else
        {
            Debug.Log("Invalid placement position. Action cancelled.");
        }

        // NOTE: After placement, you will likely want to transition back to the idle state.
        // This logic should be handled by your main state machine controller.
        // For example: _orthoCam.ChangeState(new Ortho_IdleState(_orthoCam));
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

    // This method is not part of the core logic but is required by the interface.
    public void Init(Vector3 worldPos, Vector2 screenPos) { }
}