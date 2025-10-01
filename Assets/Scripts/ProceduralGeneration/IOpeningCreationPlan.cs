using UnityEngine;
using System.Collections.Generic;

public interface IOpeningCreationPlan
{
    void AddOpeningSegments(Wall wall, Opening opening,
        Vector3 startLS, Vector3 endLS, Vector3 dirLS,
        ref Vector3 cursorLS, List<GameObject> segments, bool createCol = true);
}

