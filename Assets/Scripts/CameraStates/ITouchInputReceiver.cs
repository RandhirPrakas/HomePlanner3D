using UnityEngine;
public interface ITouchInputReceiver
{
    void OnTouchStart(Vector3 worldPos, Vector2 screenPos);
    void OnTouchEnd(Vector3 worldPos, Vector2 screenPos);
    void OnTouchHold(Vector3 worldPos, Vector2 screenPos);
}