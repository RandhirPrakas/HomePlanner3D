using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Button _clearButton;
    [SerializeField] private Button _drawButton;
    [SerializeField] private Button _toggleButton;

    private void Awake()
    {

    }

    private void OnEnable()
    {
        _toggleButton.onClick.AddListener(() => ToggleCamerStates());
        _clearButton.onClick.AddListener(() => ResetSceneAndCreateNewRoom());
        _drawButton.onClick.AddListener(() => GameManager.Instance.SetSubState(new DrawState()));
    }

    private void OnDisable()
    {
        _drawButton.onClick.RemoveAllListeners();
        _clearButton.onClick.RemoveAllListeners();
    }

    private void ResetSceneAndCreateNewRoom()
    {
        foreach(WallPoint wp in WallPointManager.Instance._allWallPoints)
        {
            Destroy(wp.gameObject);
        }
        WallPointManager.Instance._allWallPoints.Clear();

        foreach (Room room in RoomManager.Instance._allRooms)
        {
            Destroy(room.gameObject);
        }
        RoomManager.Instance._allRooms.Clear();
        GameManager.Instance.GetSubStateManager().SetOrthoIdleState();
    }

    public void SetDrawButtonActive(bool val)
    {
        _drawButton.gameObject.SetActive(val);
    }

    private void ToggleCamerStates()
    {
        GameManager.Instance.GetCameraStateManager().ToggleCamera();
    }
}
