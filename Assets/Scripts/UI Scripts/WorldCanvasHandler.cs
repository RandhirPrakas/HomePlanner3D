using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
// using Exoa.Designer;
// using Exoa.Cameras;
using UnityEngine.EventSystems;
using System;
using System.Numerics;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class WorldCanvasHandler : MonoBehaviour
{
   // public CameraTopDownOrtho camOrtho;
    public RadialLayout radialLayout;
    public Sprite lockSprite, unlockSprite;
    public Button sizeUpBtn;
    public Button sizeDownBtn;    
    public Button rotateBtn;    
    public Button cloneBtn;    
    public Button lockBtn;    
    public Button deleteBtn;
    public Image lockIcon;
    public RectTransform[] btnTransformList;
    public Vector3 yOffset=Vector3.zero;
    private RectTransform _rectTransform;
    public GameObject _selectedObject;
    private float radialDistance;
    private bool isLock = false;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        var canvas = GetComponent<Canvas>();
        canvas.worldCamera = GameManager.Instance.GetCameraStateManager().GetCurrentState() is OrthographicState
            ? GameManager.Instance.GetOrthoCamera()._mainCamera
            : GameManager.Instance.GetPerspCam()._mainCamera;
        sizeUpBtn.onClick.AddListener(SizeUpCallBack);
        sizeDownBtn.onClick.AddListener(SizeDownCallBack);
        rotateBtn.onClick.AddListener(RotateObjectCallBack);
        lockBtn.onClick.AddListener(LockCallBack);
        cloneBtn.onClick.AddListener(CloneCallBack);
        deleteBtn.onClick.AddListener(delegate
        {
            if (_selectedObject != null)
            {
                StartCoroutine(DeleteWithAnimation(1f));
                Hide();
            }
                
        });

        Initialize(_selectedObject);
    }

    private void LateUpdate()
    {
        if (_rectTransform != null && radialLayout.IsActive())
        {
            if (GameManager.Instance.GetCameraStateManager().GetCurrentState() is OrthographicState)
            {
                ChangeIconSizeOrthographic();
            }
            else
            {
                if (!radialLayout.gameObject.activeSelf) return;
                
                radialLayout.transform.LookAt(Camera.main.transform);
                radialLayout.transform.Rotate(Vector3.up - new Vector3(0, 180, 0));
                ChangeIconSizePerspective(); 
            }
        }
    }

    private IEnumerator DeleteWithAnimation(float duration)
    {
        Transform t = _selectedObject.transform;

        float elapsedTime = 0.0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / duration; // 0-->1
            normalizedTime = Mathf.SmoothStep(0, 1, normalizedTime);
            t.localScale = Vector3.Lerp(t.localScale, Vector3.zero, normalizedTime);
            yield return null;
        }

        t.localScale = Vector3.zero;
        DestroyImmediate(_selectedObject.gameObject);
        Destroy(this.gameObject);
    }

    /// <summary>
    /// Changes the size of Radial layout acc. to camera distance in Othrographic mode.
    /// </summary>
    private void ChangeIconSizeOrthographic()
    {
        float f1 = 1f; //camOrtho.FinalSize;
        if (f1 > 4)
        {
            foreach (var item in btnTransformList)
            {
                item.localScale = Vector3.one * (f1 / 4.0f);
            }
            if (f1 < 10)
                f1 = 10;
            radialLayout.fDistance = f1 * 20 * radialDistance;
            radialLayout.CalculateRadial();
        }
        else
        {
            foreach (var item in btnTransformList)
            {
                item.localScale = Vector3.one;
            }
            radialLayout.fDistance = radialDistance * 150;
            radialLayout.CalculateRadial();
        }
    }

    /// <summary>
    /// Changes the size of Radial layout acc. to camera distance in Perspective mode.
    /// </summary>
    private void ChangeIconSizePerspective()
    {
        float _lastDistance = -1f;
        
        float distance = Vector3.Distance(
            transform.position,
            Camera.main.transform.position
        );

        // Clamp to avoid extreme scaling
        distance = Mathf.Clamp(distance, 6f, 30f);

        // Only update if distance changed enough
        if (Mathf.Abs(distance - _lastDistance) < 0.3f)
            return;

        _lastDistance = distance;

        // 1️⃣ Keep icon size consistent (screen-space feel)
        float iconScale = distance / 10f;

        foreach (var item in btnTransformList)
        {
            item.localScale = Vector3.one * iconScale;
        }

        // 2️⃣ Radial distance should be proportional but STABLE
        radialLayout.fDistance = radialDistance * 120f * iconScale;

        // 3️⃣ Recalculate layout ONCE per meaningful change
        radialLayout.CalculateRadial();
    }

    /// <summary>
    /// Initiates the Radial layout for a module.
    /// </summary>
    /// <param name="go"></param>
    public void Initialize(GameObject go)
    {
        radialLayout.MaxAngle = 0;
        // Calculate and assigning Radial distance.
        BoxCollider bc = go.GetComponent<BoxCollider>();
        bc.enabled = true;
        radialDistance = bc.bounds.size.magnitude;
        radialDistance = radialDistance > 2f ? 2f : radialDistance;
        radialLayout.fDistance = radialDistance * 150;

        //Moving the WorldCanvas to the position of this module.
        _rectTransform.position = new(go.transform.position.x,
            go.transform.position.y + (bc.bounds.size.y / 2) + yOffset.y, go.transform.position.z);
        _rectTransform.LookAt(Camera.main.transform);
        _rectTransform.Rotate(Vector3.up - new Vector3(0, 180, 0));

        //Reset and Enabling the Radial layout object.
        RectTransform _rt = radialLayout.GetComponent<RectTransform>();
        _rt.localPosition = new(0, 0, 0);
        _rt.localRotation = new(0, 0, 0, 0);
        _rt.sizeDelta = new(bc.bounds.size.x * 100, bc.bounds.size.y * 100);
        radialLayout.gameObject.SetActive(true);

        bc.enabled = false;

        // Framing camera and UI on it.
        GameManager.Instance.GetPerspCam().FrameObjectWithWorldUI(_selectedObject, _rt);

        //Checking if the module is locked.
        isLock = go.GetComponent<PlaceableObject>().IsLock;
        lockIcon.sprite = isLock ? lockSprite : unlockSprite;

        StartCoroutine(MyCoroutine());
    }

    /// <summary>
    /// This method animates the Radial layout.
    /// </summary>
    /// <returns></returns>
    IEnumerator MyCoroutine()
    {
        float timeToStart = Time.time;
        while (radialLayout.MaxAngle != 360) // This is your target size of object.
        {
            float tempTime = Mathf.Lerp(0, 360, (Time.time - timeToStart) * 2f);//Here speed is the 1 or any number which decides the how fast it reach to one to other end.
            radialLayout.MaxAngle = tempTime;
            radialLayout.CalculateRadial();
            yield return null;
        }

        print("Reached the target.");

    }

    private void SizeUpCallBack()
    {
        if (_selectedObject == null || _selectedObject.CompareTag(Constants.TAG_PLACABLES) == false)
            return;
        // Calling current State Increase Scale Method
       var editObjectIn3DSubState =  GameManager.Instance.GetSubStateManager().GetCurrentSubState() as EditObjectIn3D;
       editObjectIn3DSubState?.ChangeSize(0.125f);

       GameManager.Instance.GetPerspCam().ReframeAfterScale(_selectedObject, _rectTransform);
    }

    private void SizeDownCallBack()
    {
        Debug.Log("SizeDownCallBack");
        if (_selectedObject == null ||_selectedObject.CompareTag(Constants.TAG_PLACABLES) == false)
            return;
        // Calling current State Increase Scale Method
        var editObjectIn3DSubState =  GameManager.Instance.GetSubStateManager().GetCurrentSubState() as EditObjectIn3D;
        editObjectIn3DSubState?.ChangeSize(-0.125f);
        
        GameManager.Instance.GetPerspCam().ReframeAfterScale(_selectedObject, _rectTransform);
    }

    private void RotateObjectCallBack()
    {
        if (_selectedObject == null || _selectedObject.CompareTag(Constants.TAG_PLACABLES) == false)
            return;

        // Calling current State Increase Scale Method
        var editObjectIn3DSubState =  GameManager.Instance.GetSubStateManager().GetCurrentSubState() as EditObjectIn3D;
        editObjectIn3DSubState?.RotateObject(10.0f);
    }

    private void CloneCallBack()
    {
        if (_selectedObject == null || _selectedObject.CompareTag(Constants.TAG_PLACABLES) == false)
            return;
        
        var editObjectIn3DSubState =  GameManager.Instance.GetSubStateManager().GetCurrentSubState() as EditObjectIn3D;
        editObjectIn3DSubState.CloneObject();
        
        Hide();
    }

    private void LockCallBack()
    {
        if (_selectedObject == null || _selectedObject.CompareTag(Constants.TAG_PLACABLES) == false)
            return;

        var placeableData = _selectedObject.GetComponentInChildren<PlaceableObject>();
        placeableData.IsLock = !placeableData.IsLock;
        lockIcon.sprite = placeableData.IsLock ? lockSprite : unlockSprite;
        
        Hide();
    }

    public void Hide()
    {
        if(radialLayout!=null)
            radialLayout.gameObject.SetActive(false);
    }
}
