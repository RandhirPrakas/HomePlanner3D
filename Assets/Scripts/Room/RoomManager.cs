using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;
    
    public static int RoomCountIndex = 0;

    public List<Room> _allRooms = new List<Room>();

    public Room _activeRoom;

    public List<Vector3> _preservedHoleCentroids = new List<Vector3>();
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetActiveRoom(Room room)
    {
        _activeRoom = room;
    }

    public void DeleteRoom(Room room)
    {
        if (room == null)
            return;

        var wallsToProcess = new List<Wall>(room._roomWalls);
        var pointsToProcess = new List<WallPoint>(room._roomWallPoints);

        foreach (Wall wall in wallsToProcess)
        {
          
            if (wall.GetParentRoomCount() <= 1)
            {
                if (wall.GetParentRoomCount() == 1 && wall.GetRoomParent()[0] != room)
                    continue;
                WallManager.Instance.DeleteWall(wall, refresh: false);
            }
            else
            {
                wall.RemoveParentRoom(room);
            }
        }
        AppEventHandler.InvokeOnWallCreation();

        foreach (WallPoint wp in pointsToProcess)
        {
            if (wp == null) continue;

            wp.RemoveConnectedRoom(room);

            if (wp.GetConnectedWalls().Count == 0)
            {
                WallPointManager.Instance.DeleteWallPoint(wp);
            }
        }

        _allRooms.Remove(room);

        if (_activeRoom == room)
        {
            _activeRoom = null;
        }

        Destroy(room.gameObject);
    }
}
