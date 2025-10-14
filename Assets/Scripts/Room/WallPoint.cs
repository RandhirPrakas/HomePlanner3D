using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Represents a point or vertex in the wall layout system. 
/// It acts as a node connecting different walls and is a corner of one or more rooms.
/// </summary>
public class WallPoint : MonoBehaviour
{
    // Public fields for easy access from other scripts, representing the point's state.
    public Vector3 _position; // The world-space position of this wall point.
    public GameObject _activeSphere; // A visual highlight GameObject (e.g., a sphere) shown when the point is selected or active.

    // Serialized private fields to store relationships with other architectural elements.
    [SerializeField] private List<WallPoint> _connectedWallPoints = new List<WallPoint>(); // A list of other WallPoints directly connected to this one by a wall.
    [SerializeField] private List<Wall> _connectedWalls = new List<Wall>(); // A list of Wall objects that use this WallPoint as a start or end point.
    [SerializeField] private List<Room> _connectedRooms = new List<Room>(); // A list of Room objects that include this WallPoint as one of its corners.

    /// <summary>
    /// Gets the list of WallPoints that are directly connected to this one.
    /// </summary>
    /// <returns>A List of connected WallPoint objects.</returns>
    public List<WallPoint> GetConnectedWallPoints()
    {
        return _connectedWallPoints;
    }

    /// <summary>
    /// Assigns a GameObject to be used as a visual highlight for this point.
    /// </summary>
    /// <param name="visual">The GameObject to use as the highlight visual.</param>
    public void SetHighlightVisual(GameObject visual)
    {
        _activeSphere = visual;
    }

    /// <summary>
    /// Initializes the WallPoint at a specific position upon creation.
    /// </summary>
    /// <param name="position">The initial world-space position for this WallPoint.</param>
    public void Initialize(Vector3 position)
    {
        _position = position;
        transform.position = position;
    }

    /// <summary>
    /// Updates the position of this WallPoint and its associated highlight visual.
    /// </summary>
    /// <param name="newPos">The new world-space position.</param>
    public void SetPosition(Vector3 newPos)
    {
        _position = newPos;
        transform.position = newPos;

        // If a highlight visual exists, move it to the new position as well.
        if (_activeSphere != null)
            _activeSphere.transform.position = newPos;
    }

    

    public void MergeWith(WallPoint target)
    {
        // --- STEP 1: VALIDATION & SETUP ---
        if (target == null || target == this)
        {
            return; // Cannot merge with null or itself.
        }

        // Use HashSets for efficient tracking of elements that need a final update.
        var wallsToUpdate = new HashSet<Wall>();
        var roomsToUpdate = new HashSet<Room>(_connectedRooms); // Copy rooms from this point.

        // --- STEP 2: FIND THE REDUNDANT WALL (IF ANY) ---
        // In a closed-loop scenario, there will be a wall directly connecting `this` and `target`.
        // This wall must be deleted, not re-parented.
        Wall wallToDelete = null;
        foreach (var wall in _connectedWalls)
        {
            if ((wall.StartWallPoint == this && wall.EndWallPoint == target) ||
                (wall.StartWallPoint == target && wall.EndWallPoint == this))
            {
                wallToDelete = wall;
                break;
            }
        }

        // --- STEP 3: PERFORM ALL DATA MODEL CHANGES ---

        // A) Re-parent all other walls from `this` point to `target`.
        foreach (var wall in _connectedWalls.ToList()) // Use .ToList() to create a safe copy.
        {
            if (wall == wallToDelete) continue;

            if (wall.StartWallPoint == this) wall.SetStartWallPoint(target);
            else if (wall.EndWallPoint == this) wall.SetEndWallPoint(target);

            target.AddConnectedWall(wall);
            wallsToUpdate.Add(wall);
        }

        // B) Update all neighboring WallPoint connections.
        foreach (var neighbor in _connectedWallPoints.ToList())
        {
            neighbor.RemoveConnectedWallPoint(this); // The neighbor must forget `this` point.

            if (neighbor != target)
            {
                neighbor.AddConnectedWallPoint(target); // The neighbor now connects to `target`.
                target.AddConnectedWallPoint(neighbor); // `target` connects back to the neighbor.
            }
        }

        // C) Update all affected rooms to use `target` instead of `this`.
        foreach (var room in roomsToUpdate)
        {
            room._roomWallPoints.Remove(this); // Remove the old point.
            if (!room._roomWallPoints.Contains(target))
            {
                room._roomWallPoints.Add(target); // Add the new one if it's not already there.
            }
            target.AddConnectedRoom(room);
        }

        // D) Delete the redundant wall WITHOUT triggering a global refresh.
        if (wallToDelete != null)
        {
            // This is the critical change: we pass `refresh: false`.
            WallManager.Instance.DeleteWall(wallToDelete, true, refresh: false);
        }

        // E) Destroy this obsolete WallPoint's GameObject.
        DestroyHighlightVisual();
        WallPointManager.Instance._allWallPoints.Remove(this);
        Destroy(gameObject);

        // --- STEP 4: TRIGGER UPDATES NOW THAT DATA IS STABLE ---
        // The data model is now fully consistent. It is safe to update meshes and notify other systems.

        foreach (var wall in wallsToUpdate)
        {
            wall.UpdateFromPoints(true);
        }
        foreach (var room in roomsToUpdate)
        {
            room.UpdateFloor();
        }

        // Finally, fire the single, authoritative event to signal that the process is complete.
        AppEventHandler.InvokeOnWallCreation();
    }

    /// <summary>
    /// Destroys the highlight visual GameObject if it exists.
    /// </summary>
    private void DestroyHighlightVisual()
    {
        if (_activeSphere != null)
        {
            GameObject.Destroy(_activeSphere);
            _activeSphere = null; // Set to null to prevent referencing a destroyed object.
        }
    }

    /// <summary>
    /// Adds a WallPoint to the list of connected points, ensuring no duplicates.
    /// </summary>
    /// <param name="newConnectedWallPoint">The WallPoint to connect to.</param>
    public void AddConnectedWallPoint(WallPoint newConnectedWallPoint)
    {
        if (!_connectedWallPoints.Contains(newConnectedWallPoint))
            _connectedWallPoints.Add(newConnectedWallPoint);
    }

    /// <summary>
    /// Removes a WallPoint from the list of connected points if it exists.
    /// </summary>
    /// <param name="wallPoint">The WallPoint to disconnect from.</param>
    public void RemoveConnectedWallPoint(WallPoint wallPoint)
    {
        if (_connectedWallPoints.Contains(wallPoint))
        {
            _connectedWallPoints.Remove(wallPoint);
        }
    }

    /// <summary>
    /// Adds a Wall to the list of connected walls, ensuring no duplicates.
    /// </summary>
    /// <param name="newWall">The Wall to connect.</param>
    public void AddConnectedWall(Wall newWall)
    {
        if (!_connectedWalls.Contains(newWall))
        {
            _connectedWalls.Add(newWall);
        }
    }

    /// <summary>
    /// Removes a Wall from the list of connected walls if it exists.
    /// </summary>
    /// <param name="wall">The Wall to disconnect.</param>
    public void RemoveConnectedWall(Wall wall)
    {
        if (_connectedWalls.Contains(wall))
        {
            _connectedWalls.Remove(wall);
        }
    }

    /// <summary>
    /// Gets the list of Walls connected to this point.
    /// </summary>
    /// <returns>A List of connected Wall objects.</returns>
    public List<Wall> GetConnectedWalls()
    {
        return _connectedWalls;
    }

    /// <summary>
    /// Adds a Room to the list of associated rooms, ensuring no duplicates.
    /// </summary>
    /// <param name="room">The Room this point is a corner of.</param>
    public void AddConnectedRoom(Room room)
    {
        if (!_connectedRooms.Contains(room))
        {
            _connectedRooms.Add(room);
        }
    }

    /// <summary>
    /// Removes a Room from the list of associated rooms if it exists.
    /// </summary>
    /// <param name="room">The Room to disassociate.</param>
    public void RemoveConnectedRoom(Room room)
    {
        if (_connectedRooms.Contains(room))
            _connectedRooms.Remove(room);
    }

    /// <summary>
    /// Gets the list of Rooms this point belongs to.
    /// </summary>
    /// <returns>A List of connected Room objects.</returns>
    public List<Room> GetConnectedRooms()
    {
        return _connectedRooms;
    }
}