using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class EditWallin3D : ICameraSubState
{
    private Wall _selectedWall;
    private Material _material;

    #region Property

    public Wall SelectedWall
    {
        get => _selectedWall;
        set
        {
            if (_selectedWall == value) return;
            _selectedWall = value;
            WallManager.Instance._currentSelectedWall = value;
        }
    }

    #endregion
    public EditWallin3D(Wall wall)
    {
        SelectedWall = wall;
        _material = wall.GetComponent<MeshRenderer>().material;
    }

    public void Enter()
    {
        Debug.Log("Entered Edit Wall in 3D State");
        //throw new System.NotImplementedException();
    }

    public void Exit()
    {
        //throw new System.NotImplementedException();
    }

    public void Init(Vector3 worldPos, Vector2 screenPos)
    {
        //throw new System.NotImplementedException();
    }

    public void OnPinch(float delta)
    {
        //throw new System.NotImplementedException();
    }

    public void OnTouchEnd(Vector3 worldPos, Vector2 screenPos)
    {
        //throw new System.NotImplementedException();
    }

    public void OnTouchHold(Vector3 worldPos, Vector2 screenPos)
    {
        //throw new System.NotImplementedException();
    }

    public void OnTouchStart(Vector3 worldPos, Vector2 screenPos)
    {
        //throw new System.NotImplementedException();
    }

    public void Update()
    {
        //throw new System.NotImplementedException();
        if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            if(_material!= null)
            {
                _material.color = RandomizeColor();
                SelectedWall._material = _material;
            }
        }
    }

    private Color RandomizeColor()
    {
        return new Color(Random.value,Random.value,Random.value,1f);
    }
}
