using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Opening
{
    public override void Initialize(Wall wall, Vector3 worldPosition)
    {
        Height = 4;
        //Height = 2.1336f;
        Width = 2.5f;
        //Width = 1.400001264f;
        _parentWall = wall;
        OpeningType = OpeningType.Door;

        CalculateAndSetNormalizedPosition(worldPosition);
        UpdatePositionAndRotation();


        transform.SetParent(wall.transform, worldPositionStays: true);

        if (!wall._allOpenings.Contains(this))
            wall._allOpenings.Add(this);

        OpeningManager.Instance.AddOpening(this);
    }

    
}
