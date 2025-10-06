using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Door : Opening
{
    public override void Initialize(Wall wall, Vector3 worldPosition)
    {
        base.Initialize(wall, worldPosition);
        Height = 4;
        //Height = 2.1336f;
        Width = 2.5f;
        //Width = 1.400001264f;

        ParentWall = wall;
        transform.position = worldPosition;

        ParentWall = wall;
        OpeningType = OpeningType.Door;

        CalculateAndSetNormalizedPosition(worldPosition);
        UpdatePositionAndRotation();
        UpdateWidthLabel();


        transform.SetParent(wall.transform, worldPositionStays: true);

        if (!wall._allOpenings.Contains(this))
            wall._allOpenings.Add(this);

        OpeningManager.Instance.AddOpening(this);

        _lastKnownWallStart = null;
        _lastKnownWallEnd = null;
        ParentWall.Refresh();
    }

    
}
