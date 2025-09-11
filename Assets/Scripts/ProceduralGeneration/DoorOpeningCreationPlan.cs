using UnityEngine;
using System.Collections.Generic;

public class DoorOpeningStrategy : IOpeningCreationPlan
{
    public void AddOpeningSegments(
        Wall wall, Opening opening,
        Vector3 startLS, Vector3 endLS, Vector3 dirLS,
        ref Vector3 cursorLS, List<GameObject> segments)
    {
        Vector3 openingLS = wall.transform.InverseTransformPoint(opening.OpeningPosition);
        float along = Vector3.Dot(openingLS - startLS, dirLS);
        float half = opening.Width * 0.5f;

        Vector3 openingStartLS = startLS + dirLS * (along - half);
        Vector3 openingEndLS = startLS + dirLS * (along + half);

        // Fill before opening
        if (Vector3.Distance(cursorLS, openingStartLS) > 0.01f)
        {
            segments.AddRange(
                ProceduarlwallGenerator.GenerateWallSegment(
                    wall.transform.TransformPoint(cursorLS),
                    wall.transform.TransformPoint(openingStartLS),
                    wall.transform));
        }

        // Gap for door → starts at floor
        segments.AddRange(
            ProceduarlwallGenerator.GenerateWallSegment(
                wall.transform.TransformPoint(openingStartLS),
                wall.transform.TransformPoint(openingEndLS),
                wall.transform,
                AppHelper._wallHeight - opening.Height, // strip above
                opening.Height));                       // door height

        cursorLS = openingEndLS;
    }
}
