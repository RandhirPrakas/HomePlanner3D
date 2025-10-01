using UnityEngine;
using System.Collections.Generic;

public class WindowOpeningStrategy : IOpeningCreationPlan
{
    public void AddOpeningSegments(
        Wall wall, Opening opening,
        Vector3 startLS, Vector3 endLS, Vector3 dirLS,
        ref Vector3 cursorLS, List<GameObject> segments, bool createCol = true)
    {
        Vector3 openingLS = wall.transform.InverseTransformPoint(opening.OpeningPosition);
        float along = Vector3.Dot(openingLS - startLS, dirLS);
        float half = opening.Width * 0.5f;

        Vector3 openingStartLS = startLS + dirLS * (along - half);
        Vector3 openingEndLS = startLS + dirLS * (along + half);

        // Fill before window
        if (Vector3.Distance(cursorLS, openingStartLS) > 0.01f)
        {
            segments.AddRange(
                ProceduarlwallGenerator.GenerateWallSegment(
                    wall.transform.TransformPoint(cursorLS),
                    wall.transform.TransformPoint(openingStartLS),
                    wall, createCol:createCol));
        }

        float center = openingLS.y;
        float bottom = center - (opening.Height * 0.5f);
        float top = center + (opening.Height * 0.5f);

        // bottom strip
        if (bottom > 0.01f)
        {
            segments.AddRange(
                ProceduarlwallGenerator.GenerateWallSegment(
                    wall.transform.TransformPoint(openingStartLS),
                    wall.transform.TransformPoint(openingEndLS),
                    wall,
                    bottom,
                    0f, createCol: createCol));
        }

        // top strip
        if (AppHelper._wallHeight - top > 0.01f)
        {
            segments.AddRange(
                ProceduarlwallGenerator.GenerateWallSegment(
                    wall.transform.TransformPoint(openingStartLS),
                    wall.transform.TransformPoint(openingEndLS),
                    wall,
                    AppHelper._wallHeight - top,
                    top, createCol: createCol));
        }

        cursorLS = openingEndLS;
    }
}
