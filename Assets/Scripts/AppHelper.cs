
// Will Contain wrapper, calculations, some unique feature which will be used later etcs
using System;
using UnityEngine;

public static class AppHelper
{
    #region Variables_ProceduralWallGeneration

    public static readonly float _minimumWallLength = 4f;
    public static readonly float _minimumWallHeight = 5f;
    public static readonly float _wallThickness = 1f;
    public static readonly float _wallHeight = 7f;

    #endregion

    #region Variables_PointsManagements

    public static readonly float _pointSnapThreshold = 10f;

    #endregion


    #region Events

    public static event Action OnWallCreation;

    #region Invoker Functions
    public static void InvokeOnWallCreation()
    {
        OnWallCreation?.Invoke();
    }
    #endregion

    #endregion


    public static readonly float _lrYPos = 1f;
    public static readonly float _lrThickness = 2f;

    // this check if distance between two point is 
    public static bool CanSnapPoint(Vector3 a, Vector3 b)
    {
        return Vector3.Distance(a, b) < _pointSnapThreshold;
    }

    // pointToSnap will be snapped to snapPosition
    public static Vector3 SnapPoint(Vector3 snapPosition, Vector3 pointToSnap)
    {
        if (CanSnapPoint(snapPosition, pointToSnap))
        {
            pointToSnap = snapPosition;
        }
        return pointToSnap;
    }

    public static float DistanceBetweenTwoPoints(Vector3 a, Vector3 b)
    {
        return Vector3.Distance(a, b);
    } 

    public static float DistanceBetweenTwoPoints(Vector2 a, Vector2 b)
    {
        return Vector2.Distance(a, b);
    }


    public static Vector3 WrapPosition(Vector3 startPosition, Vector3 endPosition)
    {
        if (Mathf.Abs(startPosition.x - endPosition.x) < _pointSnapThreshold)
        {
            endPosition = new Vector3(startPosition.x, endPosition.y, endPosition.z);
        }
        else if (Mathf.Abs(startPosition.z - endPosition.z) < _pointSnapThreshold)
        {
            endPosition = new Vector3(endPosition.x, endPosition.y, startPosition.z);
        }

        return endPosition;
    }

}
