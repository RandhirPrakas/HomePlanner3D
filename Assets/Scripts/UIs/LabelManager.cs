using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LabelManager : MonoBehaviour
{
    public static LabelManager Instance;

    private Dictionary<Wall, TextMeshPro> _wallLabels = new Dictionary<Wall, TextMeshPro>();
    private Dictionary<Room, TextMeshPro> _roomLabels = new Dictionary<Room, TextMeshPro>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Keep hierarchy clean
        gameObject.name = "LabelManager";
    }

    #region Wall Labels
    public void RequestWallLabel(Wall wall, Vector3 start, Vector3 end, float length)
    {
        if (!_wallLabels.ContainsKey(wall))
        {
            GameObject go = new GameObject("WallLengthLabel");
            go.transform.SetParent(transform);
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.fontSize = 0.5f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.black;
            tmp.enableAutoSizing = true;
            _wallLabels[wall] = tmp;
        }

        var label = _wallLabels[wall];
        label.text = $"{length:F2} ft";

        Vector3 center = (start + end) * 0.5f;
        label.transform.position = center + Vector3.up * 0.2f;

        // Rotate with wall
        Vector3 dir = (end - start).normalized;
        float angle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
        label.transform.rotation = Quaternion.Euler(90f, -angle, 0f);
    }

    public void RemoveWallLabel(Wall wall)
    {
        if (_wallLabels.ContainsKey(wall))
        {
            Destroy(_wallLabels[wall].gameObject);
            _wallLabels.Remove(wall);
        }
    }
    #endregion

    #region Room Labels
    public void RequestRoomLabel(Room room, Vector3 centroid, float area)
    {
        if (!_roomLabels.ContainsKey(room))
        {
            GameObject go = new GameObject("RoomAreaLabel");
            go.transform.SetParent(transform);
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.fontSize = 10f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            _roomLabels[room] = tmp;
        }

        var label = _roomLabels[room];
        label.text = $"{area:F1} sq ft";
        label.transform.position = centroid + Vector3.up * 0.2f;

        // Always face camera
        if (Camera.main != null)
            label.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);
    }


    public void RemoveRoomLabel(Room room)
    {
        if (_roomLabels.ContainsKey(room))
        {
            Destroy(_roomLabels[room].gameObject);
            _roomLabels.Remove(room);
        }
    }
    #endregion
}
