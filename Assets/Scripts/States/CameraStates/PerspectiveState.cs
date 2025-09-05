using System.Collections.Generic;
using System.Linq;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;

public class PerspectiveState : CameraState
{
    private ProceduarlwallGenerator _wallGenerator;
    public override void Enter()
    {
        Camera.main.orthographic = false;
        HideGameobjectsFromOrthoState();
        GenerateWalls();
        SetCameraOrientation();
        GameManager.Instance.GetSubStateManager().SetPerspIdleState();
    }

    public override void Exit()
    {
        Debug.Log("Exiting Perspective Mode");
    }

    private void SetCameraOrientation()
    {
        Camera.main.transform.position = new Vector3(0, 25, -25);
        Camera.main.transform.rotation = Quaternion.Euler(45, 0, 0);
        Camera.main.fieldOfView = 45;
    }

    public void GenerateWalls()
    {
        if (_wallGenerator == null)
        {
            _wallGenerator = new ProceduarlwallGenerator();
        }

        foreach (Wall wall in WallManager.Instance._allWalls)
        {
            _wallGenerator.GenerateWallSegment(wall.GetStartPosition(), wall.GetEndPosition(), wall.gameObject.transform);
        }
    }

    private void HideGameobjectsFromOrthoState()
    {
        // Disable All the Line Renderers from Wall
        foreach(Wall wall in WallManager.Instance._allWalls)
        {
            wall._lineRenderer.enabled = false;
            wall._boxCollider.enabled = false;
        }

        // Disable Openings 2d meshes
        foreach(Opening opening in OpeningManager.Instance._allOpenings)
        {
            opening.GetComponent<MeshRenderer>().enabled = false;
        }
    }


    /*public void GenerateWalls()
    {
        if (_wallGenerator == null)
            _wallGenerator = new ProceduarlwallGenerator();

        foreach (Wall wall in WallManager.Instance._allWalls)
        {
            List<GameObject> allSegments = new List<GameObject>();

            if (wall._allOpenings == null || wall._allOpenings.Count == 0)
            {
                // no openings → full wall
                allSegments.AddRange(
                    _wallGenerator.GenerateWallSegment(
                        wall.GetStartPosition(),
                        wall.GetEndPosition(),
                        wall.transform));
            }
            else
            {
                // --- Work in local space ---
                Vector3 startWS = wall.GetStartPosition();
                Vector3 endWS = wall.GetEndPosition();

                Vector3 startLS = wall.transform.InverseTransformPoint(startWS);
                Vector3 endLS = wall.transform.InverseTransformPoint(endWS);
                Vector3 dirLS = (endLS - startLS).normalized;

                var spans = wall._allOpenings
                    .Select(o =>
                    {
                        Vector3 openingLS = wall.transform.InverseTransformPoint(o.OpeningPosition);

                        float along = Vector3.Dot(openingLS - startLS, dirLS);
                        float half = o.Width * 0.5f;

                        return new
                        {
                            left = along - half,
                            right = along + half,
                            centerY = openingLS.y,   // 🔹 interpret as bottom (Door) or center (Window)
                            height = o.Height,
                            type = o.OpeningType
                        };
                    })
                    .OrderBy(s => s.left)
                    .ToList();

                Vector3 cursorLS = startLS;

                foreach (var s in spans)
                {
                    Vector3 openingStartLS = startLS + dirLS * s.left;
                    Vector3 openingEndLS = startLS + dirLS * s.right;

                    // --- Wall before the opening
                    if (Vector3.Distance(cursorLS, openingStartLS) > 0.01f)
                    {
                        allSegments.AddRange(
                            _wallGenerator.GenerateWallSegment(
                                wall.transform.TransformPoint(cursorLS),
                                wall.transform.TransformPoint(openingStartLS),
                                wall.transform));
                    }

                    // --- Differentiate Opening Types ---
                    if (s.type == OpeningType.Door)
                    {
                        // Doors → gap starts at floor
                        allSegments.AddRange(
                            _wallGenerator.GenerateWallSegment(
                                wall.transform.TransformPoint(openingStartLS),
                                wall.transform.TransformPoint(openingEndLS),
                                wall.transform,
                                AppHelper._wallHeight - s.height, // strip above door
                                s.height));                       // door height
                    }
                    else if (s.type == OpeningType.Window)
                    {
                        float center = s.centerY;                // interpret Y as window center
                        float bottom = center - (s.height * 0.5f);
                        float top = center + (s.height * 0.5f);

                        // bottom strip (floor → window bottom)
                        if (bottom > 0.01f)
                        {
                            allSegments.AddRange(
                                _wallGenerator.GenerateWallSegment(
                                    wall.transform.TransformPoint(openingStartLS),
                                    wall.transform.TransformPoint(openingEndLS),
                                    wall.transform,
                                    bottom,   // strip height
                                    0f));     // from floor
                        }

                        // top strip (window top → ceiling)
                        if (AppHelper._wallHeight - top > 0.01f)
                        {
                            allSegments.AddRange(
                                _wallGenerator.GenerateWallSegment(
                                    wall.transform.TransformPoint(openingStartLS),
                                    wall.transform.TransformPoint(openingEndLS),
                                    wall.transform,
                                    AppHelper._wallHeight - top,
                                    top));
                        }
                    }

                    // move cursor past this opening
                    cursorLS = openingEndLS;
                }

                // --- After last opening
                if (Vector3.Distance(cursorLS, endLS) > 0.01f)
                {
                    allSegments.AddRange(
                        _wallGenerator.GenerateWallSegment(
                            wall.transform.TransformPoint(cursorLS),
                            wall.transform.TransformPoint(endLS),
                            wall.transform));
                }
            }

            // Combine All Created Walls 
            _wallGenerator.CombineChildMeshes(wall.transform, allSegments);
        }
    }*/

}