using UnityEngine;
public interface ICameraSubState : ITouchInputReceiver
{
    void Init(Vector3 worldPos, Vector2 screenPos);
    void Enter();
    void Exit();
    void Update();
}