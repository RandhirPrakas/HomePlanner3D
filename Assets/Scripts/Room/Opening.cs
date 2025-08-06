using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Opening : MonoBehaviour
{    
    [SerializeField] private OpeningType _openingType = OpeningType.Door; // Door will be the default value.
    [SerializeField] private Vector3 _openingPosition;
    [SerializeField] private float _width, _height;

    #region GetterAndSetter/Properties

    public float Width { get=> _width; set => _width = value;}
    public float Height { get =>  _height; set => _height = value;}

    public Vector3 OpeningPosition { get=> _openingPosition; set => _openingPosition = value;}

    public OpeningType OpeningType {get => _openingType;set => _openingType = value;} 

    #endregion
}
