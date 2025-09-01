using UnityEditor;
using UnityEngine;

public class OrthographicState : CameraState
{
    public override void Enter()
    {
        Debug.Log("Switched to Orthographic Mode");
        Camera.main.orthographic = true;
        SetCameraOrientation();
        //RemoveWallMeshes();
        ShowLineRendereAndCanvas();
        GameManager.Instance.GetSubStateManager().SetOrthoIdleState();
    }

    public override void Exit()
    {
        Debug.Log("Exiting Orthographic Mode");
    }

    private void ShowLineRendereAndCanvas()
    {
        foreach (Room room in RoomManager.Instance._allRooms)
        {
           
        }
    }
    
    private void RemoveWallMeshes()
    {
        foreach (Room room in RoomManager.Instance._allRooms)
        {
            foreach (Wall wall in room._roomWalls)
            {
                GameObject.Destroy(wall.GetComponent<MeshRenderer>());
                GameObject.Destroy(wall.GetComponent<MeshFilter>());
            }
        }
    }

    private void SetCameraOrientation()
    {
        Camera.main.transform.position = new Vector3(0, 50, 0);
        Camera.main.transform.rotation = Quaternion.Euler(90, 0, 0);
        Camera.main.fieldOfView = 45;

    }
}
