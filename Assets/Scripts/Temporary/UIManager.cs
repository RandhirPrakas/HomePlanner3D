using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Canvas _canvas;

    [SerializeField] private Button _clearButton;
    [SerializeField] private Button _drawButton;
    [SerializeField] private Button _2D3DModeSwitchButton;
    [SerializeField] private Button _addDoorButton;
    [SerializeField] private Button _addWindowButton;
    [SerializeField] private Button _addModuleButton;

    public EditUI _editUIPrefab;
    public GameObject _modelSelectionUI;
    // This prefab refer to UI comes on top of 3D Object placed.
    public WorldCanvasHandler worldCanvasHandlerPlacedObject;

    #region Model Items
    public PlacementManager _placementManager;
    #endregion


    private void OnEnable()
    {
        _modelSelectionUI.SetActive(false);
        _2D3DModeSwitchButton.onClick.AddListener(() => ToggleCamerStates());
        _clearButton.onClick.AddListener(() => ResetSceneAndCreateNewRoom());
        _drawButton.onClick.AddListener(() => GameManager.Instance.SetSubState(new DrawState(GameManager.Instance.GetOrthoCamera())));
        _addDoorButton.onClick.AddListener(() => GameManager.Instance.SetSubState(new AddOpeningState<Door>(GameManager.Instance.GetOrthoCamera(), Constants.DOOR_VISUALIZER)));

        if (GameManager.Instance != null && GameManager.Instance._window)
        {
            _addWindowButton.gameObject.SetActive(true);
        }
        _addWindowButton.onClick.AddListener(() => GameManager.Instance.SetSubState(new AddOpeningState<Window>(GameManager.Instance.GetOrthoCamera(), Constants.WINDOW_VISUALIZER)));
        _addModuleButton.onClick.AddListener(() => ShowModelSelectionUI());
    }

    private void OnDisable()
    {
        _drawButton.onClick.RemoveAllListeners();
        _clearButton.onClick.RemoveAllListeners();
        _addDoorButton.onClick.RemoveAllListeners();
        _2D3DModeSwitchButton.onClick.RemoveAllListeners();
        _addWindowButton.onClick.RemoveAllListeners();
        _addModuleButton.onClick.RemoveAllListeners();
    }

    private void ResetSceneAndCreateNewRoom()
    {
        foreach (WallPoint wp in WallPointManager.Instance._allWallPoints)
        {
            Destroy(wp.gameObject);
        }
        WallPointManager.Instance._allWallPoints.Clear();
        WallPointManager.WallPointCountIndex = 0;

        foreach (Opening opening in OpeningManager.Instance.GetAllOpenings())
        {
            Destroy(opening.gameObject);
        }
        OpeningManager.Instance.GetAllOpenings().Clear();

        foreach (Wall wall in WallManager.Instance._allWalls)
        {
            Destroy(wall.gameObject);
        }
        WallManager.Instance._allWalls.Clear();
        WallManager.WallCountIndex = 0;

        foreach (Room room in RoomManager.Instance._allRooms)
        {
            room.RemoveRoom();
        }
        RoomManager.Instance._allRooms.Clear();
        GameManager.Instance.GetCameraStateManager().SetOrthographicState();
    }

    public void SetButtonActive(Button btn, bool val)
    {
        btn.gameObject.SetActive(val);
    }

    public void OnEnterOrhtoIdleState()
    {
        _drawButton.gameObject.SetActive(true);
        _addDoorButton.gameObject.SetActive(true);
        _addWindowButton.gameObject.SetActive(true);
    }

    public void OnExitOrthoIdleState()
    {
        _drawButton.gameObject.SetActive(false);
        _addDoorButton.gameObject.SetActive(false);
        _addWindowButton.gameObject.SetActive(false);
    }

    private void ToggleCamerStates()
    {
        GameManager.Instance.GetCameraStateManager().ToggleCamera();
    }

    private void ShowModelSelectionUI()
    {
        if (GameManager.Instance.GetCameraStateManager().GetCurrentState() is OrthographicState)
            GameManager.Instance.GetSubStateManager().SetOrthoIdleState();
        else
            GameManager.Instance.GetSubStateManager().SetPerspIdleState();
        _modelSelectionUI.SetActive(true);
    }

    public void HideModelSelectionUI()
    {
        _modelSelectionUI.SetActive(false);
        if (GameManager.Instance.GetCameraStateManager().GetCurrentState() is OrthographicState)
            GameManager.Instance.GetSubStateManager().SetOrthoIdleState();
        else
            GameManager.Instance.GetSubStateManager().SetPerspIdleState();
    }

}
