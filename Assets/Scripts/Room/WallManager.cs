using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallManager : MonoBehaviour
{
    public static WallManager Instance;

    public List<Wall> _allWalls = new List<Wall>();

    public static int _wallIndex = 0;

    // Make it Singleton
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
                    wall.transform.SetParent(room.transform);
                }
            }
        }
    }
}
