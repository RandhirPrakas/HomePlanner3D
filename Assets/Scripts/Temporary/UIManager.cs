using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Button _clearButton;
    [SerializeField] private Button _drawButton;
    [SerializeField] private Button _toggleButton;
    [SerializeField] private Button _addDoorButton;
    [SerializeField] private Button _addWindowButton;

    public EditUI _editUIPrefab;

    private void OnEnable()
    {
        _toggleButton.onClick.AddListener(() => ToggleCamerStates());
        _clearButton.onClick.AddListener(() => ResetSceneAndCreateNewRoom());
        _drawButton.onClick.AddListener(() => GameManager.Instance.SetSubState(new DrawState(GameManager.Instance.GetOrthoCamera())));
        _addDoorButton.onClick.AddListener(() => GameManager.Instance.SetSubState(new AddOpeningState<Door>(GameManager.Instance.GetOrthoCamera(), Constants.PATH_DOOR_VISUALIZER)));

        if(GameManager.Instance != null && GameManager.Instance._window)
        {
            _addWindowButton.gameObject.SetActive(true);
        }
        _addWindowButton.onClick.AddListener(() => GameManager.Instance.SetSubState(new AddOpeningState<Window>(GameManager.Instance.GetOrthoCamera(), Constants.PATH_WINDOW_VISUALIZER)));
    }

    private void OnDisable()
    {
        _drawButton.onClick.RemoveAllListeners();
        _clearButton.onClick.RemoveAllListeners();
        _addDoorButton.onClick.RemoveAllListeners();
        _toggleButton.onClick.RemoveAllListeners();
        _addWindowButton.onClick.RemoveAllListeners();
    }

    private void ResetSceneAndCreateNewRoom()
    {
        foreach(WallPoint wp in WallPointManager.Instance._allWallPoints)
        {
            Destroy(wp.gameObject);
        }
        WallPointManager.Instance._allWallPoints.Clear();

        foreach (Opening opening in OpeningManager.Instance.GetAllOpenings())
        {
            Destroy(opening.gameObject);
        }
        OpeningManager.Instance.GetAllOpenings().Clear();

        foreach(Wall wall in WallManager.Instance._allWalls)
        {
            Destroy(wall.gameObject);
        }
        WallManager.Instance._allWalls.Clear();

        foreach (Room room in RoomManager.Instance._allRooms)
        {
            room.RemoveRoom();
            //Destroy(room.gameObject);
        }
        RoomManager.Instance._allRooms.Clear();
        GameManager.Instance.GetSubStateManager().SetOrthoIdleState();
    }

    public void SetButtonActive(Button btn, bool val)
    {
        btn.gameObject.SetActive(val);
    }

    public void OnEnterOrhtoIdleState()
    {
        _drawButton.gameObject.SetActive(true);
        _addDoorButton.gameObject.SetActive(true);
    }

    public void OnExitOrthoIdleState()
    {
        _drawButton.gameObject.SetActive(false);
        _addDoorButton.gameObject.SetActive(false);
    }

    private void ToggleCamerStates()
    {
        GameManager.Instance.GetCameraStateManager().ToggleCamera();
    }
}
