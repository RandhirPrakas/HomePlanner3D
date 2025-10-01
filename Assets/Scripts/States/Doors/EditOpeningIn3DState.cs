using UnityEngine;

public class EditOpeningIn3DState<T> : ICameraSubState where T : Opening
{
    private T _selectedOpening;
    private Camera _camera;
    // We no longer need the dragPlane or dragOffset for this logic
    // private Plane _dragPlane; 
    // private Vector3 _dragOffset;

    [SerializeField] private LayerMask _wallLayerMask;

    private float _snapThreshold = 2f;
    public GameObject circularHandlePrefab;

    public T SelectedOpening
    {
        get => _selectedOpening;
        set
        {
            if (_selectedOpening == value) return;
            _selectedOpening = value;
        }
    }

    public EditOpeningIn3DState(Camera camera, T Opening)
    {
        _camera = camera ?? Camera.main;
        if (circularHandlePrefab == null)
        {
            circularHandlePrefab = Resources.Load<GameObject>("Prefabs/circularHandlePrefab");
        }

        _wallLayerMask = LayerMask.GetMask(Constants.LAYER_WALL);
        Debug.Log(_wallLayerMask.value);

        SelectedOpening = Opening;
        ShowResizer(Opening);
    }
    public void Enter()
    {
        Debug.Log($"Entered EditOpeningIn3DState<{typeof(T).Name}>");
    }

    public void Init(Vector3 worldPos, Vector2 screenPos) { }
    public void Update() { }
    public void Exit()
    {
        Debug.Log($"Exited EditOpeningIn3DState<{typeof(T).Name}>");
        _selectedOpening = null;
    }

    public void OnTouchStart(Vector3 worldPos, Vector2 screenPos)
    {
        Ray ray = _camera.ScreenPointToRay(screenPos);
        // We only care if we hit an opening to start the drag
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            if (hit.collider.TryGetComponent<T>(out var opening))
            {
                SelectedOpening = opening;
            }
        }
    }

    public void OnTouchHold(Vector3 worldPos, Vector2 screenPos)
    {
        if (SelectedOpening == null) return;

        // Call the updated placement logic
        MoveOpeningOnWall(screenPos, false);
    }

    public void OnTouchEnd(Vector3 worldPos, Vector2 screenPos)
    {
        if (SelectedOpening != null)
        {
            Debug.Log($"Moved {SelectedOpening.name} to {SelectedOpening.OpeningPosition} on {SelectedOpening.ParentWall?.name}");
            ClearWallSegment();
            // Final placement with collider generation
            MoveOpeningOnWall(screenPos, true);

            // Deselect after the operation is complete
            SelectedOpening = null;
        }
    }

    public void OnPinch(float delta)
    {
        if (_camera.orthographic)
        {
            _camera.orthographicSize = Mathf.Max(0.1f, _camera.orthographicSize - delta);
        }
        else
        {
            _camera.fieldOfView = Mathf.Clamp(_camera.fieldOfView - delta, 20f, 80f);
        }
    }


    #region Helpers

    /*private void MoveOpeningOnWall(Vector2 screenPos, bool createCol = true)
    {
        Ray ray = _camera.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, _wallLayerMask))
        {
            Debug.Log("Collided on Wall " + hit.collider.gameObject.name);
            Vector3 targetPos = hit.point;

            if (!hit.collider.TryGetComponent<Wall>(out Wall nearestWall))
            {
                nearestWall = hit.collider.GetComponentInParent<Wall>();
                Debug.Log("Collided on Wall whose parent is " + hit.collider.gameObject.name);
            }

            if (nearestWall != null)
            {
                targetPos.y = SelectedOpening.transform.position.y;

                if (SelectedOpening.ParentWall != nearestWall)
                {
                    Wall oldWall = SelectedOpening.ParentWall;
                    SelectedOpening._lastWall = oldWall;
                    if (oldWall != null)
                        oldWall._allOpenings.Remove(SelectedOpening);

                    SelectedOpening.transform.SetParent(nearestWall.transform, true);
                    SelectedOpening._parentWall = nearestWall;

                    if (!nearestWall._allOpenings.Contains(SelectedOpening))
                        nearestWall._allOpenings.Add(SelectedOpening);
                }

                SelectedOpening.transform.position = targetPos;
                SelectedOpening.OpeningPosition = nearestWall.transform.InverseTransformPoint(targetPos);

                Vector3 dir = (nearestWall.GetEndPosition() - nearestWall.GetStartPosition()).normalized;
                SelectedOpening.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

                WallMeshGenerator.GenerateWallWithOpenings(nearestWall, createCol);

                if (SelectedOpening._lastWall != null)
                {
                    WallMeshGenerator.GenerateWallWithOpenings(SelectedOpening._lastWall, createCol);
                    SelectedOpening._lastWall = null;
                }
            }
        }
    }*/

    private void MoveOpeningOnWall(Vector2 screenPos, bool createCol = true)
    {
        Ray ray = _camera.ScreenPointToRay(screenPos);

        // VISUAL DEBUG: Draw a red line in the scene view to show the ray's path
        Debug.DrawRay(ray.origin, ray.direction * 1000f, Color.red);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, _wallLayerMask))
        {
            Debug.Log($"Raycast hit: {hit.collider.name}");

            Vector3 targetPos = hit.point;

            if (!hit.collider.TryGetComponent<Wall>(out Wall nearestWall))
            {
                nearestWall = hit.collider.GetComponentInParent<Wall>();
            }

            if (nearestWall != null)
            {
                targetPos.y = SelectedOpening.transform.position.y;

                if (SelectedOpening.ParentWall != nearestWall)
                {
                    Wall oldWall = SelectedOpening.ParentWall;
                    SelectedOpening._lastWall = oldWall;
                    if (oldWall != null)
                        oldWall._allOpenings.Remove(SelectedOpening);

                    SelectedOpening.transform.SetParent(nearestWall.transform, true);
                    SelectedOpening._parentWall = nearestWall;

                    if (!nearestWall._allOpenings.Contains(SelectedOpening))
                        nearestWall._allOpenings.Add(SelectedOpening);
                }

                SelectedOpening.transform.position = targetPos;
                SelectedOpening.OpeningPosition = nearestWall.transform.InverseTransformPoint(targetPos);

                Vector3 dir = (nearestWall.GetEndPosition() - nearestWall.GetStartPosition()).normalized;
                SelectedOpening.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

                WallMeshGenerator.GenerateWallWithOpenings(nearestWall, createCol);

                if (SelectedOpening._lastWall != null)
                {
                    WallMeshGenerator.GenerateWallWithOpenings(SelectedOpening._lastWall, createCol);
                    SelectedOpening._lastWall = null;
                }
            }
        }
        else
        {
            Debug.Log("Raycast did not hit anything on the 'Walls' layer.");
        }
    }

    private void ClearWallSegment()
    {
        if (SelectedOpening == null || SelectedOpening.ParentWall == null) return;

        foreach (GameObject go in SelectedOpening.ParentWall.WallSegmentColliders)
        {
            GameObject.Destroy(go);
        }
        SelectedOpening.ParentWall.WallSegmentColliders.Clear();
    }

    public void ShowResizer(T opening)
    {
        SelectedOpening = opening;
    }

    public void HideResizer()
    {
        SelectedOpening = null;
    }

    #endregion
}