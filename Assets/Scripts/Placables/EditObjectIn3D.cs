using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class EditObjectIn3D : ICameraSubState
{
    private readonly OrthoCam _orthoCam;
    private readonly PerspCam _perspCam;

    private GameObject _selectedObject;
    private readonly PlaceableObject _placeableData;
    private readonly Collider _objectCollider;

    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private bool _isPlacementValid = false;
    private int _floorLayerMask; // This must be an int for the bitmask

    private readonly Material _validPlacementMaterial;
    private readonly Material _invalidPlacementMaterial;
    private readonly Dictionary<Renderer, Material[]> _originalMaterials = new Dictionary<Renderer, Material[]>();

    // The visualizer handles all distance measurement logic
    private readonly MeasurementVisualizer _measurementVisualizer;

    public GameObject SelectedObject
    {
        get => _selectedObject;
        set
        {
            if (_selectedObject == value) return;
            _selectedObject = value;
        }
    }

    public EditObjectIn3D(OrthoCam orthoCam, PerspCam perspCam, GameObject objectToEdit)
    {
        _orthoCam = orthoCam;
        _perspCam = perspCam;

        SelectedObject = objectToEdit;
        _placeableData = SelectedObject.GetComponent<PlaceableObject>();
        _objectCollider = SelectedObject.GetComponent<Collider>();

        _validPlacementMaterial = Constants.DEFAULT_VALID_PLACAMENT_MATERIAL;
        _invalidPlacementMaterial = Constants.DEFAULT_INVALID_PLACAMENT_MATERIAL;

    }

    public void Enter()
    {
        Debug.Log("Entered Edit Object State");
        _originalPosition = SelectedObject.transform.position;
        _originalRotation = SelectedObject.transform.rotation;

        if (_objectCollider != null) _objectCollider.enabled = false;

        CacheOriginalMaterials();
        SetFeedbackMaterial(_invalidPlacementMaterial);

        _floorLayerMask = 1 << LayerMask.NameToLayer(Constants.LAYER_FlOOR);
    }

    public void Exit()
    {
        if (_objectCollider != null) _objectCollider.enabled = true;
        RestoreOriginalMaterials();
        //_measurementVisualizer.DestroyVisuals();
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

        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            RotateObject(-30);
        }
        else if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            RotateObject(30);
        }
    }

    public void OnPinch(float delta)
    {
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
        bool foundValidSurface = TryPlaceOnValidSurface(ray);

        if (!foundValidSurface)
            PlaceOnFallbackPlane(ray);

        UpdatePlacementFeedback(foundValidSurface);

    }

    private bool TryPlaceOnValidSurface(Ray ray)
    {
        if (!Physics.Raycast(ray, out RaycastHit hit, 1000f, _floorLayerMask))
            return false;

        if (_placeableData.Type == PlaceableObject.PlacementType.Ground && hit.collider.CompareTag("Room"))
        {
            PlaceOnGround(hit);
            return true;
        }

        if (_placeableData.Type == PlaceableObject.PlacementType.Wall && hit.collider.CompareTag("Wall"))
        {
            PlaceOnWall(hit);
            return true;
        }

        return false;
    }

    private void PlaceOnGround(RaycastHit hit)
    {
        SelectedObject.transform.position = hit.point + new Vector3(0, _placeableData.GroundOffset, 0);
        SelectedObject.transform.rotation = Quaternion.identity;
    }

    private void PlaceOnWall(RaycastHit hit)
    {
        SelectedObject.transform.position = hit.point;
        SelectedObject.transform.rotation = Quaternion.LookRotation(-hit.normal);
    }

    private void PlaceOnFallbackPlane(Ray ray)
    {
        var plane = new Plane(Vector3.up, SelectedObject.transform.position);
        if (plane.Raycast(ray, out float enter))
            SelectedObject.transform.position = ray.GetPoint(enter);
    }

    private void UpdatePlacementFeedback(bool foundValidSurface)
    {
        if (foundValidSurface == _isPlacementValid) return;

        _isPlacementValid = foundValidSurface;
        SetFeedbackMaterial(_isPlacementValid ? _validPlacementMaterial : _invalidPlacementMaterial);
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

    public void Init(Vector3 worldPos, Vector2 screenPos) { }

    private void GroundObject()
    {
        if (SelectedObject == null) return;
        SelectedObject.transform.position = new Vector3(SelectedObject.transform.position.x, SelectedObject.transform.localScale.y / 2, SelectedObject.transform.position.z);
    }

    private void ChangeSize(float factor)
    {
        if (SelectedObject == null) return;

        Debug.Log("Changing Size");
        SelectedObject.transform.localScale += SelectedObject.transform.localScale * factor;
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
}