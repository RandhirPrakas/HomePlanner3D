using UnityEditor;
using UnityEngine;

public class OrthographicState : CameraState
{
    public OrthoCam _orthoCam;
    public override void Enter()
    {

        _orthoCam = GameManager.Instance.GetOrthoCamera();

        Debug.Log("Switched to Orthographic Mode");
        Camera.main.orthographic = true;
        SetCameraOrientation();
        RemoveWallMeshes();
        ShowGameobjectsFromOrthoState();
        GameManager.Instance.GetSubStateManager().SetOrthoIdleState();
        _orthoCam.FitToAllWalls();

        Two_D_Settings();
    }

    public override void Exit()
    {
        _orthoCam.enabled = false;
    }

    private void RemoveWallMeshes()
    {
        foreach (Wall wall in WallManager.Instance._allWalls)
        {
            GameObject.Destroy(wall.GetComponent<MeshRenderer>());
            GameObject.Destroy(wall.GetComponent<MeshFilter>());
        }
    }

    private void SetCameraOrientation()
    {
        Camera.main.transform.position = new Vector3(0, 50, 0);
        Camera.main.transform.rotation = Quaternion.Euler(90, 0, 0);
        Camera.main.fieldOfView = 45;

        _orthoCam.enabled = true;
        GameManager.Instance.GetPerspCam().enabled = false;
    }

    private void ShowGameobjectsFromOrthoState()
    {
        // Disable All the Line Renderers from Wall
        foreach (Wall wall in WallManager.Instance._allWalls)
        {
            wall._lineRenderer.enabled = true;
            wall._boxCollider.enabled = true;
        }

        // Disable Openings 2d meshes
        // Sometime Instance is null, shouldn't be but, may be because its instace is not yet created but still called.
        if (OpeningManager.Instance != null)
        {
            foreach (Opening opening in OpeningManager.Instance.GetAllOpenings())
            {
                opening.OpeningVisual.gameObject.SetActive(true);
            }
        }
    }

    // Disable BoxColliders if it exists and Enable Sphere colliders for all opening
    private void Two_D_Settings()
    {
        foreach(Opening opening in OpeningManager.Instance.GetAllOpenings())
        {
            if(opening.GetComponent<BoxCollider>()!= null)
                opening.GetComponent<BoxCollider>().enabled = false;
        }
    }
}
