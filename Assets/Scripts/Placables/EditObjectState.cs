using System.Collections.Generic;
using System.Threading.Tasks.Sources;
using UnityEngine;

public class EditObjectState : ICameraSubState
{
    // It will hold a reference to one of these, but not both.
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

    public GameObject SelectedObject { get => _selectedObject;
        set{
            if (SelectedObject == value) return;
            _selectedObject = value;
        }
    }

    /// <summary>
    /// A single constructor that can accept either camera type.
    /// Pass 'null' for the camera you are not using.
    /// </summary>
    public EditObjectState(OrthoCam orthoCam, PerspCam perspCam, GameObject objectToEdit)
    {
        _orthoCam = orthoCam;
        _perspCam = perspCam;

        SelectedObject= objectToEdit;
        _placeableData = SelectedObject.GetComponent<PlaceableObject>();
        _objectCollider = SelectedObject.GetComponent<Collider>();

        _validPlacementMaterial = Resources.Load<Material>("ProceduralMaterials/ValidPlacement");
        _invalidPlacementMaterial = Resources.Load<Material>("ProceduralMaterials/InvalidPlacement");
    }

    public void Enter()
    {
        _originalPosition = SelectedObject.transform.position;
        _originalRotation = SelectedObject.transform.rotation;

        if (_objectCollider != null) _objectCollider.enabled = false;

        CacheOriginalMaterials();
        SetFeedbackMaterial(_invalidPlacementMaterial);

        Debug.Log("Entered Edit Object State");
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
            SelectedObject.transform.SetPositionAndRotation(_originalPosition, _originalRotation);
        }

        // Return to the correct idle state
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

        if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            // Increase Size
            IncreaseSize();
        }
        else if(Input.GetKeyDown(KeyCode.DownArrow))
        {
            // Decrease Size
            DecreaseSize();
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
        if (SelectedObject== null)
            return;
        SelectedObject.transform.position = new Vector3(SelectedObject.transform.position.x, SelectedObject.transform.localScale.y/2, SelectedObject.transform.position.z);
    }

    private void IncreaseSize()
    {
        if (SelectedObject == null)
            return;
        SelectedObject.transform.localScale += SelectedObject.transform.localScale * 0.5f;
        GroundObject();
    }

    private void DecreaseSize()
    {
        if (SelectedObject == null)
            return;
        SelectedObject.transform.localScale -= SelectedObject.transform.localScale * 0.5f;
        GroundObject();
    }
}