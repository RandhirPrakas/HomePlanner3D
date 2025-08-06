using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Room : MonoBehaviour
{
    public List<Wall> _allRoomWalls = new List<Wall>();
    public Canvas _roomCanvas;

    public void SpawnWallLabelCanvas()
    {
        GameObject canvasGO = new GameObject("WallLabelsCanvas");
        canvasGO.transform.SetParent(transform);

        _roomCanvas = canvasGO.AddComponent<Canvas>();
        _roomCanvas.renderMode = RenderMode.WorldSpace;
        _roomCanvas.worldCamera = Camera.main;

        CanvasScaler cs = canvasGO.AddComponent<CanvasScaler>();
        cs.dynamicPixelsPerUnit = 10;

        canvasGO.AddComponent<GraphicRaycaster>();

        RectTransform rt = _roomCanvas.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(150, 150);
        canvasGO.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
    }

}
