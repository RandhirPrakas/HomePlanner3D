using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrthoCam : CameraManager
{
    private Vector2 _initialScreenTouchPos;

    #region 

    public Vector2 GetInitialScreenTouchPosition()
    {
        return _initialScreenTouchPos;
    }

    public void SetInitialTouchPosition(Vector2 position)
    {
        _initialScreenTouchPos = position;
    }

    #endregion

    public void MoveCameraByDistance(Vector3 distance)
    {
        _mainCamera.transform.position += distance * _translationSpeed * Time.deltaTime;
    }

    public Vector3 GetDistance(Vector2 finalPosition)
    {
        Vector2 distance = (_initialScreenTouchPos - finalPosition).normalized;
        return new Vector3(distance.x, 0, distance.y);
    }
}
