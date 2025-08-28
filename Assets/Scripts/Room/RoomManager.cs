using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    public List<Room> _allRooms = new List<Room>();

    public Room _activeRoom;

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

    public Room FindOrCreateRoomForWall(Wall wall)
    {
        // check if this wall connects to an existing room
        foreach (Room room in _allRooms)
        {
            if (room.HasCornerNear(wall.GetStartPosition()) || room.HasCornerNear(wall.GetEndPosition()))
            {
                room._allRoomWalls.Add(wall);
                room._wallCorners.Add(wall.GetStartPosition());
                room._wallCorners.Add(wall.GetEndPosition());
                return room;
            }
        }

        // no room found → create a new one
        GameObject newRoomGO = new GameObject("Room");
        Room newRoom = newRoomGO.AddComponent<Room>();
        newRoom._allRoomWalls.Add(wall);
        newRoom._wallCorners.Add(wall.GetStartPosition());
        newRoom._wallCorners.Add(wall.GetEndPosition());
        _allRooms.Add(newRoom);

        return newRoom;
    }

    public bool CheckIfRoomClosed(Room room)
    {
        if (room._wallCorners.Count < 3) return false;

        foreach (Wall wall in room._allRoomWalls)
        {
            foreach (Wall other in room._allRoomWalls)
            {
                if (wall != other)
                {
                    if (wall.GetStartPosition() == other.GetEndPosition() ||
                        wall.GetEndPosition() == other.GetStartPosition())
                    {
                        // loop found → complete room
                        room.OnWallCreation();
                        return true;
                    }
                }
            }
        }
        return false;
    }


}
