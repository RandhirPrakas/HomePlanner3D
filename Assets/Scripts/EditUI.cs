using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditUI : MonoBehaviour
{
    #region Variables_Buttons

    [SerializeField] private Button _deleteButton;
    [SerializeField] private Button _exitButton;

    #endregion

    [SerializeField] private EditUIType _currentUIType = EditUIType.WallPointEdit;
    [SerializeField] private Canvas _canvas;

    public EditUIType CurrentUIType { get => _currentUIType; set => _currentUIType = value; }



    private void Awake()
    {
        if (_deleteButton == null)
            _deleteButton = gameObject.transform.Find("Delete").GetComponent<Button>();
        if(_exitButton == null)
            _exitButton = gameObject.transform.Find("Exit").GetComponent<Button>();
    }

    private void OnEnable()
    {
        _deleteButton.onClick.AddListener(delegate { Delete(); });
        _exitButton.onClick.AddListener(delegate { Exit(); });
    }

    private void OnDisable()
    {
        _exitButton.onClick?.RemoveListener(delegate { Exit(); });
        _deleteButton.onClick?.RemoveListener(delegate { Delete(); });
    }

    public void Initialize(EditUIType editUIType)
    {
        CurrentUIType = editUIType;
        _canvas.renderMode = RenderMode.WorldSpace;
        _canvas.worldCamera = Camera.main;
        _canvas.transform.rotation = Quaternion.Euler(90, 0, 0);
    }

    private void Delete()
    {
        switch(_currentUIType)
        {
            case EditUIType.WallPointEdit:
                Debug.Log("Deleting Wall Point");
                WallPointManager.Instance.DeleteWallPoint(WallPointManager.Instance._currentActiveWallpoint);
                WallPointManager.Instance._currentActiveWallpoint = null;
                break;

            case EditUIType.WallEdit:

                Debug.Log("Deleting Wall ");
                WallManager.Instance.DeleteWall(WallManager.Instance._currentSelectedWall, true);
                WallManager.Instance._currentSelectedWall = null;
                WallPointManager.Instance.RemoveStandaloneWallpoints();
                break;

            case EditUIType.OpeningEdit:

                Debug.Log("Deleting Opening");
                OpeningManager.Instance.DeleteOpening(OpeningManager.Instance._currentSelectedOpening);
                OpeningManager.Instance._currentSelectedOpening = null;
                break;
        }
    }

    private void Exit()
    {
        GameManager.Instance.GetSubStateManager().SetOrthoIdleState();
    }
}

public enum EditUIType
{
    WallEdit,
    WallPointEdit,
    RoomEdit,
    OpeningEdit
}
