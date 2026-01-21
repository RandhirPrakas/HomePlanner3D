using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicUnderstanding : MonoBehaviour
{
    private Transform ts;
    private Camera mainCamera;
    public Transform target;

    private void Start()
    {
        ts = this.transform;
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        FollowMouse();
    }

    private void FollowMouse()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = Mathf.Abs(mainCamera.transform.position.z - ts.position.z);


        Vector3 positionInWorldSpace  = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        
        positionInWorldSpace = (positionInWorldSpace - ts.position);
        positionInWorldSpace.z = 0;
        
        float angle = Mathf.Atan2(positionInWorldSpace.y, positionInWorldSpace.x) * Mathf.Rad2Deg;
        
        transform.rotation = Quaternion.Euler(0f, 0f, angle -180);

    }

    private void RotateTowards2DTarget()
    {
        Vector2 direction = target.position - transform.position;
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; //Vector2.SignedAngle(Vector2.right, direction);
        
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 180);
    }
}
