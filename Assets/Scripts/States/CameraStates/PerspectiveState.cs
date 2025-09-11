using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PerspectiveState : CameraState
{
    public override void Enter()
    {
        ProceduarlwallGenerator.Init();

        // Switch camera to perspective
        if (Camera.main != null)
            Camera.main.orthographic = false;

        HideGameobjectsFromOrthoState();

        GenerateWalls();

        ThreeD_Settings();

        // Generate colliders for openings (doors/windows)
        GenerateOpeningColliders();

        SetCameraOrientation();

        // Return to perspective idle substate (keep your original call)
        GameManager.Instance.GetSubStateManager().SetPerspIdleState();
    }

    public override void Exit()
    {
        Debug.Log("Exiting Perspective Mode");
    }

    private void SetCameraOrientation()
    {
        if (Camera.main == null) return;

        Camera.main.transform.position = new Vector3(0, 25, -25);
        Camera.main.transform.rotation = Quaternion.Euler(45, 0, 0);
        Camera.main.fieldOfView = 45;
    }

    private void HideGameobjectsFromOrthoState()
    {
        if (WallManager.Instance != null)
        {
            foreach (Wall wall in WallManager.Instance._allWalls)
            {
                if (wall == null) continue;
                if (wall._lineRenderer != null) wall._lineRenderer.enabled = false;
                if (wall._boxCollider != null) wall._boxCollider.enabled = false;
            }
        }

        var allOpenings = OpeningManager.Instance?.GetAllOpenings();
        if (allOpenings != null)
        {
            foreach (Opening opening in allOpenings)
            {
                if (opening == null) continue;
                if (opening.OpeningVisual != null) opening.OpeningVisual.SetActive(false);
            }
        }
    }

    private void GenerateWalls()
    {
        if (WallManager.Instance == null)
        {
            Debug.LogWarning("WallManager.Instance not found. Skipping wall generation.");
            return;
        }

        foreach (Wall wall in WallManager.Instance._allWalls)
        {
            if (wall == null) continue;

            // Use the centralized generator that understands openings
            WallMeshGenerator.GenerateWallWithOpenings(wall);
        }
    }

    private void GenerateOpeningColliders()
    {
        if (WallManager.Instance == null)
        {
            Debug.LogError("WallManager.Instance not found. Cannot generate opening colliders.");
            return;
        }

        foreach (Wall wall in WallManager.Instance._allWalls)
        {
            if (wall == null) continue;
            if (wall._allOpenings == null || wall._allOpenings.Count == 0) continue;

            foreach (var opening in wall._allOpenings)
            {
                if (opening == null) continue;

                /*BoxCollider boxCollider = opening.GetComponent<BoxCollider>();
                if (boxCollider == null)
                    boxCollider = opening.gameObject.AddComponent<BoxCollider>();
                else
                    boxCollider.enabled = true;

                boxCollider.isTrigger = true;

                // Position the opening gameobject relative to wall (local space)
                Vector3 openingCenterLS = wall.transform.InverseTransformPoint(opening.OpeningPosition);
                if (typeof(Opening) == typeof(Door))
                    openingCenterLS.y -= opening.OpeningPosition.y;
                else if (typeof(Opening) == typeof(Window))
                    openingCenterLS.y = 0;
                    opening.transform.localPosition = openingCenterLS;

                boxCollider.center = new Vector3(0f, opening.Height / 2f, 0f);
                Vector3 colliderSize = new Vector3(Mathf.Max(0.01f, opening.Width - 0.5f), opening.Height, AppHelper._wallThickness);
                boxCollider.size = colliderSize;*/

                BoxCollider boxCollider = opening.GetComponent<BoxCollider>();
                if (boxCollider == null)
                {
                    boxCollider = opening.AddComponent<BoxCollider>();
                }
                else
                {
                    boxCollider.enabled = true;
                }
                boxCollider.isTrigger = true;

                Vector3 openingCenterLS = wall.transform.InverseTransformPoint(opening.OpeningPosition);

                if (opening.OpeningType == OpeningType.Door)
                {
                    openingCenterLS.y += 0;
                }
                opening.transform.localPosition = openingCenterLS;
                if (opening.OpeningType == OpeningType.Door)
                    boxCollider.center = new Vector3(0, opening.Height / 2 - opening.transform.position.y, 0);
                else
                    boxCollider.center = new Vector3(0, 0, 0);

                Vector3 colliderSize = new Vector3(opening.Width - 0.5f, opening.Height, AppHelper._wallThickness);

                boxCollider.size = colliderSize;

            }
        }
    }

    private void ThreeD_Settings()
    {
        var openings = OpeningManager.Instance?.GetAllOpenings();
        if (openings == null) return;

        foreach (var opening in openings)
        {
            if (opening == null) continue;
            if (opening.OpeningVisual != null) opening.OpeningVisual.SetActive(false);
        }
    }
}
