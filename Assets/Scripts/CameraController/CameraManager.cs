using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Camera _mainCamera;
    public float _translationSpeed = 50f; // default value 50f for smooth translation

    public Vector3 _touchStartWorldPos;

    public Vector2 _touchStartScreenPos;


    private void Awake()
    {
        _mainCamera = Camera.main;
    }

}
