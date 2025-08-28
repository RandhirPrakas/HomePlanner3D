
using System;
using UnityEngine;

public static class AppEventHandler
{
    public static event Action OnTap;
    public static event Action<GameObject, Vector3, Vector2> OnTouchEnd;


    public static void InvokeOnTap()
    {
        OnTap?.Invoke();
    }

    public static void InvokeOnTouchEnd(GameObject gameObject, Vector3 worldPos, Vector2 screenPos)
    {
        OnTouchEnd?.Invoke(gameObject, worldPos, screenPos);
    }
}
