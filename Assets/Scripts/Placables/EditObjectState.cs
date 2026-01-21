using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EditObjectState : ICameraSubState
{
    // Camera references remain the same
    private readonly OrthoCam _orthoCam;
    private readonly PerspCam _perspCam;

    private GameObject _selectedObject;
    private readonly PlaceableObject _placeableData;
    private readonly Collider _objectCollider;

    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private bool _isPlacementValid = false;

    private readonly Material _validPlacementMaterial;
    private readonly Material _invalidPlacementMaterial;
    private readonly Dictionary<Renderer, Material[]> _originalMaterials = new Dictionary<Renderer, Material[]>();

    // The new visualizer class handles all distance measurement logic
    private readonly MeasurementVisualizer _measurementVisualizer;
     // For world Canvas on Placed object.
    private WorldCanvasHandler _worldCanvasHandler;

    public GameObject SelectedObject
    {
        get => _selectedObject;
        set
        {
            if (_selectedObject == value) return;
            _selectedObject = value;
        }
    }

    public EditObjectState(OrthoCam orthoCam, PerspCam perspCam, GameObject objectToEdit)
    {
        _orthoCam = orthoCam;
        _perspCam = perspCam;

        SelectedObject = objectToEdit;
        _placeableData = SelectedObject.GetComponent<PlaceableObject>();
        _objectCollider = SelectedObject.GetComponent<Collider>();

        // Resource loading remains the same
        _validPlacementMaterial = Constants.DEFAULT_VALID_PLACAMENT_MATERIAL;
        _invalidPlacementMaterial = Constants.DEFAULT_INVALID_PLACAMENT_MATERIAL;

        // Instantiate the visualizer, which handles its own setup
        _measurementVisualizer = new MeasurementVisualizer();
    }

    public void Enter()
    {
        _originalPosition = SelectedObject.transform.position;
        _originalRotation = SelectedObject.transform.rotation;

        if (_objectCollider != null) _objectCollider.enabled = false;

        CacheOriginalMaterials();
        SetFeedbackMaterial(_invalidPlacementMaterial);

        // Initial update
        _measurementVisualizer.UpdateVisuals(_objectCollider);

        // Setting up world canvas for edit it.
        SetWorldCanvasUI();
        
        Debug.Log("Entered Edit Object State");
    }

    public void Exit()
    {
        if (_objectCollider != null) _objectCollider.enabled = true;
        RestoreOriginalMaterials();

        // The visualizer handles its own cleanup
        _measurementVisualizer.DestroyVisuals();
    }

    public void OnTouchStart(Vector3 worldPos, Vector2 screenPos) => UpdateObjectPosition(screenPos);
    public void OnTouchHold(Vector3 worldPos, Vector2 screenPos) => UpdateObjectPosition(screenPos);

    public void OnTouchEnd(Vector3 worldPos, Vector2 screenPos)
    {
        UpdateObjectPosition(screenPos);

        if (!_isPlacementValid)
        {
            SelectedObject.transform.SetPositionAndRotation(_originalPosition, _originalRotation);
        }

        // State transition logic remains the same
        if (_orthoCam != null)
        {
            GameManager.Instance.GetSubStateManager().SetSubState(new Ortho_IdleState(_orthoCam));
        }
        else if (_perspCam != null)
        {
            GameManager.Instance.GetSubStateManager().SetSubState(new Persp_IdleState(_perspCam));
        }
    }

    public void Update()
    {
        // Camera handling remains the same
        if (_orthoCam != null)
        {
            _orthoCam.Update();
        }
        else if (_perspCam != null)
        {
            _perspCam.UpdateCamera();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            ChangeSize(0.5f);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ChangeSize(-0.5f);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            RotateObject(-30f);
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            RotateObject(30f);
    }

    public void OnPinch(float delta)
    {
        // Camera handling remains the same
        if (_orthoCam != null)
        {
            _orthoCam.ZoomCamera(delta);
        }
        else if (_perspCam != null)
        {
            _perspCam.ZoomCamera(delta);
        }
    }

    private void UpdateObjectPosition(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        bool foundValidSurface = false;

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            if (_placeableData.Type == PlaceableObject.PlacementType.Ground && hit.collider.CompareTag("Room"))
            {
                SelectedObject.transform.position = hit.point + new Vector3(0, _placeableData.GroundOffset, 0);
                SelectedObject.transform.rotation = Quaternion.identity;
                foundValidSurface = true;
            }
            else if (_placeableData.Type == PlaceableObject.PlacementType.Wall && hit.collider.CompareTag("Wall"))
            {
                SelectedObject.transform.position = hit.point;
                SelectedObject.transform.rotation = Quaternion.LookRotation(-hit.normal);
                foundValidSurface = true;
            }
        }

        if (!foundValidSurface)
        {
            var plane = new Plane(Vector3.up, SelectedObject.transform.position);
            if (plane.Raycast(ray, out float enter))
            {
                SelectedObject.transform.position = ray.GetPoint(enter);
            }
        }

        if (foundValidSurface != _isPlacementValid)
        {
            _isPlacementValid = foundValidSurface;
            SetFeedbackMaterial(_isPlacementValid ? _validPlacementMaterial : _invalidPlacementMaterial);
        }

        // Just one call to update all distance visuals
        _measurementVisualizer.UpdateVisuals(_objectCollider);
    }

    #region Material Handling
    private void CacheOriginalMaterials()
    {
        _originalMaterials.Clear();
        foreach (var renderer in SelectedObject.GetComponentsInChildren<Renderer>(true))
        {
            _originalMaterials[renderer] = renderer.materials;
        }
    }

    private void SetFeedbackMaterial(Material mat)
    {
        if (mat == null) return;
        foreach (var renderer in _originalMaterials.Keys)
        {
            var newMaterials = new Material[renderer.materials.Length];
            for (int i = 0; i < newMaterials.Length; i++) { newMaterials[i] = mat; }
            renderer.materials = newMaterials;
        }
    }

    private void RestoreOriginalMaterials()
    {
        foreach (var pair in _originalMaterials)
        {
            if (pair.Key != null) { pair.Key.materials = pair.Value; }
        }
    }
    #endregion

    private void GroundObject()
    {
        if (SelectedObject == null) return;
        SelectedObject.transform.position = new Vector3(SelectedObject.transform.position.x, SelectedObject.transform.localScale.y / 2, SelectedObject.transform.position.z);
    }

    private void ChangeSize(float amount)
    {
        if (SelectedObject == null) return;
        SelectedObject.transform.localScale += SelectedObject.transform.localScale * amount;
        GroundObject();
    }


    private void RotateObject(float amount)
    {
        if (SelectedObject == null)
            return;

        Debug.Log("Rotate this object");
        SelectedObject.transform.Rotate(0, amount, 0);
        GroundObject();
    }

    public void Init(Vector3 worldPos, Vector2 screenPos) { }
    
    private void SetWorldCanvasUI()
    {
        if (_worldCanvasHandler == null)
        {
            // Instantiate and parent under the wall point
            _worldCanvasHandler = GameObject.Instantiate(
                GameManager.Instance._uiManager.worldCanvasHandlerPlacedObject,
                Vector3.zero,
                Quaternion.identity,
                null
            );
            _worldCanvasHandler.gameObject.name = "WorldCanvas";
        }

        if (_selectedObject != null)
            _worldCanvasHandler._selectedObject = _selectedObject;
    }
}