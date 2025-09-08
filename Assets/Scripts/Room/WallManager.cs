using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class WallManager : MonoBehaviour
{
    public static WallManager Instance;

    public List<Wall> _allWalls = new List<Wall>();

    public static int _wallIndex = 0;

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

    private void OnEnable()
    {
        AppEventHandler.OnRoomCreated += ManageWalls;
    }

    private void OnDisable()
    {
        AppEventHandler.OnRoomCreated -= ManageWalls;
    }

    private void ManageWalls()
    {
        foreach(Room room in RoomManager.Instance._allRooms)
        {
            foreach(Wall wall in _allWalls)
            {
                if(room._roomWallPoints.Contains(wall.GetStartWallPoint()) && room._roomWallPoints.Contains(wall.GetEndWallPoint()))
                {
                    room._roomWalls.Add(wall);
                    //wall.transform.SetParent(room.transform);
                }
            }
        }
    }

    public void DestroyWall(Wall wall)
    {
        if (wall == null) return;

        // Disconnect endpoints first
        wall.GetStartWallPoint()?.RemoveConnectedWall(wall);
        wall.GetEndWallPoint()?.RemoveConnectedWall(wall);

        _allWalls.Remove(wall);
        Destroy(wall.gameObject);
        _allWalls.RemoveAll(w => w == null);
    }

    public void DeleteWall(Wall wall)
    {
        if (wall == null)
            return;

        WallPoint start = wall.GetStartWallPoint();
        WallPoint end = wall.GetEndWallPoint();

        // Remove Connected WalPoints
        start.RemoveConnectedWallPoint(end);
        end.RemoveConnectedWallPoint(start);

        // Remove Connected Wall
        start?.RemoveConnectedWall(wall);
        end?.RemoveConnectedWall(wall);

        Destroy(wall.gameObject);
        AppEventHandler.InvokeOnWallCreation();
    }
}
