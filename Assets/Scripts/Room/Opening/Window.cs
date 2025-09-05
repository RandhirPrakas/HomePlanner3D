using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Window : Opening
{
    public override void Initialize(Wall wall, Vector3 worldPosition)
    {
        Height = 3f;    // Default window Height
        Width = 3f;     // Default Window Width
        _parentWall = wall;
        OpeningType = OpeningType.Window;

        OpeningPosition = wall.transform.InverseTransformPoint(worldPosition);

        // Attach to wall
        transform.SetParent(wall.transform, worldPositionStays: true);

        // Register with wall
        if (!wall._allOpenings.Contains(this))
            wall._allOpenings.Add(this);

        // Add the wall it is connected walls
        ConnectedWall.Add(wall);

        // Add it to OpeningManager
        OpeningManager.Instance._allOpenings.Add(this);
    }
}
