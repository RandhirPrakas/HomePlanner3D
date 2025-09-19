using System.Collections.Generic;
using UnityEngine;

public class EditObjectState : ICameraSubState
{
    // It will hold a reference to one of these, but not both.
    private readonly OrthoCam _orthoCam;
    private readonly PerspCam _perspCam;

    private readonly GameObject _selectedObject;
    private readonly PlaceableObject _placeableData;
    private readonly Collider _objectCollider;

    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private bool _isPlacementValid = false;

    private readonly Material _validPlacementMaterial;
    private readonly Material _invalidPlacementMaterial;
    private readonly Dictionary<Renderer, Material[]> _originalMaterials = new Dictionary<Renderer, Material[]>();

    /// <summary>
    /// A single constructor that can accept either camera type.
    /// Pass 'null' for the camera you are not using.
    /// </summary>
    public EditObjectState(OrthoCam orthoCam, PerspCam perspCam, GameObject objectToEdit)
    {
        _orthoCam = orthoCam;
        _perspCam = perspCam;

        _selectedObject = objectToEdit;
        _placeableData = _selectedObject.GetComponent<PlaceableObject>();
        _objectCollider = _selectedObject.GetComponent<Collider>();

        _validPlacementMaterial = Resources.Load<Material>("ProceduralMaterials/ValidPlacement");
        _invalidPlacementMaterial = Resources.Load<Material>("ProceduralMaterials/InvalidPlacement");
    }

    public void Enter()
    {
        _originalPosition = _selectedObject.transform.position;
        _originalRotation = _selectedObject.transform.rotation;

        if (_objectCollider != null) _objectCollider.enabled = false;

        CacheOriginalMaterials();
        SetFeedbackMaterial(_invalidPlacementMaterial);
    }

    public void Exit()
    {
        if (_objectCollider != null) _objectCollider.enabled = true;
        RestoreOriginalMaterials();
    }

    public void OnTouchStart(Vector3 worldPos, Vector2 screenPos)
    {
        UpdateObjectPosition(screenPos);
    }

    public void OnTouchHold(Vector3 worldPos, Vector2 screenPos)
    {
        UpdateObjectPosition(screenPos);
    }

    public void OnTouchEnd(Vector3 worldPos, Vector2 screenPos)
    {
        UpdateObjectPosition(screenPos);

        if (!_isPlacementValid)
        {
            _selectedObject.transform.SetPositionAndRotation(_originalPosition, _originalRotation);
        }

        // --- Return to the correct idle state ---
        // We check which camera is active to decide which idle state to create.
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
        // Call the update method of the active camera
        if (_orthoCam != null)
        {
            _orthoCam.Update();
        }
        else if (_perspCam != null)
        {
            _perspCam.UpdateCamera();
        }
    }

    public void OnPinch(float delta)
    {
        // Call the zoom method of the active camera
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
                _selectedObject.transform.position = hit.point + new Vector3(0, _placeableData.GroundOffset, 0);
                _selectedObject.transform.rotation = Quaternion.identity;
                foundValidSurface = true;
            }
            else if (_placeableData.Type == PlaceableObject.PlacementType.Wall && hit.collider.CompareTag("Wall"))
            {
                _selectedObject.transform.position = hit.point;
                _selectedObject.transform.rotation = Quaternion.LookRotation(-hit.normal);
                foundValidSurface = true;
            }
        }

        if (!foundValidSurface)
        {
            var plane = new Plane(Vector3.up, _selectedObject.transform.position);
            if (plane.Raycast(ray, out float enter))
            {
                _selectedObject.transform.position = ray.GetPoint(enter);
            }
        }

        if (foundValidSurface && !_isPlacementValid)
        {
            _isPlacementValid = true;
            SetFeedbackMaterial(_validPlacementMaterial);
        }
        else if (!foundValidSurface && _isPlacementValid)
        {
            _isPlacementValid = false;
            SetFeedbackMaterial(_invalidPlacementMaterial);
        }
    }

    #region Material Handling
    private void CacheOriginalMaterials()
    {
        _originalMaterials.Clear();
        foreach (var renderer in _selectedObject.GetComponentsInChildren<Renderer>(true))
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

    public void Init(Vector3 worldPos, Vector2 screenPos) { }
}