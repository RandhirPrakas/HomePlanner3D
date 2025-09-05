// Isko Uda dena, iska Kaam ka nahi hai ab

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DrawRoomState : ICameraSubState
{
    private OrthoCam _orthoCam;
    public DrawRoomState(OrthoCam orthoCam)
    {
        _orthoCam = orthoCam;
    }

    public void Enter()
    {
        throw new System.NotImplementedException();
    }

    public void Exit()
    {
        throw new System.NotImplementedException();
    }

    public void Init(Vector3 worldPos, Vector2 screenPos)
    {
        throw new System.NotImplementedException();
    }

    public void OnPinch(float delta)
    {
        throw new System.NotImplementedException();
    }

    public void OnTouchEnd(Vector3 worldPos, Vector2 screenPos)
    {
        throw new System.NotImplementedException();
    }

    public void OnTouchHold(Vector3 worldPos, Vector2 screenPos)
    {
        throw new System.NotImplementedException();
    }

    public void OnTouchStart(Vector3 worldPos, Vector2 screenPos)
    {
        throw new System.NotImplementedException();
    }

    public void Update()
    {
        throw new System.NotImplementedException();
    }
}
