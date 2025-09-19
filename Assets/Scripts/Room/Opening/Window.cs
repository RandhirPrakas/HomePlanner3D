using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Window : Opening
{
    public override void Initialize(Wall wall, Vector3 worldPosition)
    {
        Height = 2f;
        Width = 2f;
        _parentWall = wall;
        OpeningType = OpeningType.Window;

        OpeningPosition = wall.transform.InverseTransformPoint(worldPosition);
        OpeningPosition = new Vector3(OpeningPosition.x, 2f, OpeningPosition.z);

        transform.SetParent(wall.transform, worldPositionStays: true);

        if (!wall._allOpenings.Contains(this))
            wall._allOpenings.Add(this);

        OpeningManager.Instance.AddOpening(this);
    }
}
