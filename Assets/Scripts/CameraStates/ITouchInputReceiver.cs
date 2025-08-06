using UnityEngine;
public interface ITouchInputReceiver
{
    void OnTouchStart(Vector3 worldPos);
    void OnTouchEnd(Vector3 worldPos);
    void OnTouchHold(Vector3 worldPos);
}