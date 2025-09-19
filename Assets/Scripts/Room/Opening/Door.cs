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

        OpeningPosition = wall.transform.InverseTransformPoint(worldPosition);
        OpeningPosition = new Vector3(OpeningPosition.x, 3f, OpeningPosition.z);

        transform.SetParent(wall.transform, worldPositionStays: true);

        if (!wall._allOpenings.Contains(this))
            wall._allOpenings.Add(this);

        OpeningManager.Instance.AddOpening(this);
    }

    
}
