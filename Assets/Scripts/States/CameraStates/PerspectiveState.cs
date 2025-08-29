using UnityEngine;

public class PerspectiveState : CameraState
{
    private ProceduarlwallGenerator _wallGenerator;
    public override void Enter()
    {
        Camera.main.orthographic = false;
        GameManager.Instance.GetSubStateManager().SetPerspIdleState();
        GenerateWalls();
        HideLineRenderersAndCanvas();
        SetCameraOrientation();
    }

    public override void Exit()
    {
        Debug.Log("Exiting Perspective Mode");
    }

    public void GenerateWalls()
    {
        if (_wallGenerator == null)
        {
            _wallGenerator = new ProceduarlwallGenerator();
        }

        foreach (Room room in RoomManager.Instance._allRooms)
        {
            for (int i = 0; i < room._allRoomWalls.Count; i++)
            {
                Wall wall = room._allRoomWalls[i];
                _wallGenerator.MapAllRequiredPoints(wall.GetStartPosition(), wall.GetEndPosition(), wall.gameObject.transform);
            }
        }
    }

    private void HideLineRenderersAndCanvas()
    {
        foreach(Room room in RoomManager.Instance._allRooms)
        {
            room._roomCanvas.gameObject.SetActive(false);
            foreach(Wall wall in room._allRoomWalls)
            {
                wall.GetComponent<LineRenderer>().enabled = false;
            }
        }
    }

    private void SetCameraOrientation()
    {
        Camera.main.transform.position = new Vector3(0, 25, -25);
        Camera.main.transform.rotation = Quaternion.Euler(45, 0, 0);
        Camera.main.fieldOfView = 45;

    }
}