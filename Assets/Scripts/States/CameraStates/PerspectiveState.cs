using UnityEngine;

public class PerspectiveState : CameraState
{
    private ProceduarlwallGenerator _wallGenerator;
    public override void Enter()
    {
        Camera.main.orthographic = false;
        GameManager.Instance.GetSubStateManager().SetPerspIdleState();
        //GenerateWalls();
        SetCameraOrientation();
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

        foreach (Room room in RoomManager.Instance._allRooms)
        {
            for (int i = 0; i < room._roomWalls.Count; i++)
            {
                Wall wall = room._roomWalls[i];
                _wallGenerator.MapAllRequiredPoints(wall.GetStartPosition(), wall.GetEndPosition(), wall.gameObject.transform);
            }
        }
    }
}