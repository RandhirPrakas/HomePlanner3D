using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Window : Opening
{
    public override void Initialize(Wall wall, Vector3 worldPosition)
    {
        // It's good practice to call the base method first
        base.Initialize(wall, worldPosition); 

        Height = 2f;
        Width = 2f;
        ParentWall = wall;
        OpeningType = OpeningType.Window;
        transform.position = worldPosition;

        // --- ADD THESE TWO LINES ---
        // This calculates its position relative to the wall's start/end points.
        CalculateAndSetNormalizedPosition(worldPosition);
        // This uses the wall's direction to set the correct rotation.
        UpdatePositionAndRotation();
        // -------------------------

        // This part is a bit redundant now since UpdatePositionAndRotation handles it,
        // but it's good for setting the initial height. We'll adjust the Y position
        // AFTER the rotation is set.
        transform.position = new Vector3(transform.position.x, 2f, transform.position.z);
        OpeningPosition = wall.transform.InverseTransformPoint(transform.position);

        transform.SetParent(wall.transform, worldPositionStays: true);

        if (!wall._allOpenings.Contains(this))
            wall._allOpenings.Add(this);

        OpeningManager.Instance.AddOpening(this);
    }
}