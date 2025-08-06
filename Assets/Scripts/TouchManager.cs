using System.ComponentModel;
using UnityEngine;

/*public class TouchManager : MonoBehaviour
{
    [Header("Screen Touch / 2D details")]
    [SerializeField] private Vector2 _initialTouchScreenPosition;
    [SerializeField] private Vector2 _finalTouchScreenPosition;

    [Space(10f)]
    [Header("3D Details")]
    [SerializeField] private Vector3 _initialTouchPosition;
    [SerializeField] private Vector3 _finalTouchPosition;

    #region Temporary Variables
    [Tooltip("Temporary Game object, must be deleted later")]
    public GameObject TestGO;
    public float testZpos;

    #endregion

    private void Start()
    {
        Debug.Log($"Width = {Screen.width}");
        Debug.Log($"Height = {Screen.height}");
    }

    private void Update()
    {

        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    _initialTouchScreenPosition = touch.position;
                    _initialTouchPosition = Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, testZpos));
                    Debug.Log($"Initial Touch World Position = {_initialTouchPosition}");
                    Instantiate(TestGO, _initialTouchPosition, Quaternion.identity);
                    break;

                case TouchPhase.Ended:
                    _finalTouchScreenPosition = touch.position;
                    _finalTouchPosition = Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, testZpos));
                    Debug.Log($"Final Touch World Position = {_finalTouchPosition}");
                    Instantiate(TestGO, _finalTouchPosition, Quaternion.identity);

                    if(AppHelper.DistanceBetweenTwoPoints(_initialTouchPosition, _finalTouchPosition) >=5f)
                    {
                        ProceduarlwallGenerator proceduarlwall = new ProceduarlwallGenerator();
                        proceduarlwall.MapAllRequiredPoints(_initialTouchPosition, _finalTouchPosition, this.transform);
                    }
                    break;
            }
        }
    }
}
*/

public class TouchManager : MonoBehaviour
{
    [SerializeField] private float testZpos = 0f;

    private void Update()
    {
        var currentSubState = GameManager.Instance.GetSubState();
        if (GameManager.Instance.GetSubState() == null) return;


#if UNITY_EDITOR
        // Emulate touch with mouse
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, testZpos));
            GameManager.Instance.GetSubState().OnTouchStart(worldPos);
        }
        else if (Input.GetMouseButton(0))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, testZpos));
            GameManager.Instance.GetSubState().OnTouchHold(worldPos);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, testZpos));
            GameManager.Instance.GetSubState().OnTouchEnd(worldPos);
        }
#else
    // Original touch logic here
        if (Input.touchCount != 1) return;

        Touch touch = Input.GetTouch(0);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, testZpos));

        switch (touch.phase)
        {
            case TouchPhase.Began:
                currentSubState.OnTouchStart(worldPos);
                break;

            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                currentSubState.OnTouchHold(worldPos);
                break;

            case TouchPhase.Ended:
                currentSubState.OnTouchEnd(worldPos);
                break;
        }
#endif
    }
}

