using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Opening
{
    public override void Initialize(Wall wall, Vector3 worldPosition)
    {
        Height = 4f;
        Width = 2.5f;
        _parentWall = wall;
        OpeningType = OpeningType.Door;

        // Convert world position into local space of the wall
        OpeningPosition = wall.transform.InverseTransformPoint(worldPosition);
        OpeningPosition = new Vector3(OpeningPosition.x, 3f, OpeningPosition.z);
        
        // Attach opening to wall
        transform.SetParent(wall.transform, worldPositionStays: true);

        // Add to wall's list if not already there
        if (!wall._allOpenings.Contains(this))
            wall._allOpenings.Add(this);

        // Add the wall it is connected walls
        ConnectedWall.Add(wall);

        // Add it to OpeningManager
        OpeningManager.Instance.AddOpening(this);
    }
}
