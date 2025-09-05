using UnityEngine;
public interface ITouchInputReceiver
{
    public void OnTouchStart(Vector3 worldPos, Vector2 screenPos);
    public void OnTouchEnd(Vector3 worldPos, Vector2 screenPos);
    public void OnTouchHold(Vector3 worldPos, Vector2 screenPos);

    void OnPinch(float delta);
}