using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class WallMeshGenerator
{
    public static void GenerateWallWithOpenings(Wall wall)
    {
        List<GameObject> segments = new List<GameObject>();

        if (wall._allOpenings == null || wall._allOpenings.Count == 0)
        {
            segments.AddRange(
                ProceduarlwallGenerator.GenerateWallSegment(
                    wall.GetStartPosition(),
                    wall.GetEndPosition(),
                    wall.transform));
        }
        else
        {
            Vector3 startWS = wall.GetStartPosition();
            Vector3 endWS = wall.GetEndPosition();

            Vector3 startLS = wall.transform.InverseTransformPoint(startWS);
            Vector3 endLS = wall.transform.InverseTransformPoint(endWS);
            Vector3 dirLS = (endLS - startLS).normalized;

            var orderedOpenings = wall._allOpenings
                .OrderBy(o =>
                {
                    Vector3 openingLS = wall.transform.InverseTransformPoint(o.OpeningPosition);
                    return Vector3.Dot(openingLS - startLS, dirLS) - (o.Width * 0.5f);
                })
                .ToList();

            Vector3 cursorLS = startLS;

            foreach (var opening in orderedOpenings)
            {
                var strategy = OpeningCreationFactory.CreateOpening(opening.OpeningType);
                strategy.AddOpeningSegments(wall, opening, startLS, endLS, dirLS, ref cursorLS, segments);
            }

            // After last opening
            if (Vector3.Distance(cursorLS, endLS) > 0.01f)
            {
                segments.AddRange(
                    ProceduarlwallGenerator.GenerateWallSegment(
                        wall.transform.TransformPoint(cursorLS),
                        wall.transform.TransformPoint(endLS),
                        wall.transform));
            }
        }

        ProceduarlwallGenerator.CombineChildMeshes(wall.transform, segments);
    }
}
