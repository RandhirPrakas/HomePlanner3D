using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Camera _mainCamera;
    public float _translationSpeed = 15f; // default value 15f for smooth translation, at this value looks smooth

    public Vector3 _touchStartWorldPos;

    public Vector2 _touchStartScreenPos;


    private void Awake()
    {
        _mainCamera = Camera.main;
    }

}
