using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class DoorOpeningStrategy : IOpeningCreationPlan
{
    public void AddOpeningSegments(Wall wall, Opening opening,Vector3 startLS, Vector3 endLS, Vector3 dirLS,ref Vector3 cursorLS, List<GameObject> segments, bool createCol = true)
    {
        Vector3 openingLS = wall.transform.InverseTransformPoint(opening.OpeningPosition);
        float along = Vector3.Dot(openingLS - startLS, dirLS);
        float half = opening.Width * 0.5f;

        Vector3 openingStartLS = startLS + dirLS * (along - half);
        Vector3 openingEndLS = startLS + dirLS * (along + half);

        opening.OpeningStart = openingStartLS;
        opening.OpeningEnd = openingEndLS;

        // Fill before opening
        if (Vector3.Distance(cursorLS, openingStartLS) > 0.01f)
        {
            segments.AddRange(
                ProceduarlwallGenerator.GenerateWallSegment(
                    wall.transform.TransformPoint(cursorLS),
                    wall.transform.TransformPoint(openingStartLS),
                    wall, createCol: createCol));
        }

        // Gap for door → starts at floor
        segments.AddRange(
            ProceduarlwallGenerator.GenerateWallSegment(
                wall.transform.TransformPoint(openingStartLS),
                wall.transform.TransformPoint(openingEndLS),
                wall,
                AppHelper._wallHeight - opening.Height,
                opening.Height, createCol:createCol));

        cursorLS = openingEndLS;
    }
}
