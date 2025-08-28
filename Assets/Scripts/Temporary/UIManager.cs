using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Button _clearButton;

    [SerializeField] private Button _drawButton;


    private void Awake()
    {
        _clearButton.onClick.AddListener(() => ResetSceneAndCreateNewRoom());
    }

    private void OnEnable()
    {
        _drawButton.onClick.AddListener(() => GameManager.Instance.SetSubState(new DrawRoomState()));
    }

    private void OnDisable()
    {
        _drawButton.onClick.RemoveAllListeners();
    }

    public void ResetSceneAndCreateNewRoom()
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
        GameManager.Instance.GetSubStateManager().SetIdleState();
    }

    public void SetDrawButtonActive(bool val)
    {
        _drawButton.gameObject.SetActive(val);
    }
}
