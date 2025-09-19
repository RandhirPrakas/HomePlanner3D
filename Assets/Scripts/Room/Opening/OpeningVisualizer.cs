using UnityEngine;

public class OpeningVisualizer : MonoBehaviour
{
    [SerializeField] private Color _defaultColor, _highlightedColor;
    [SerializeField] private SpriteRenderer _square;

    [SerializeField] private BoxCollider _mainCollider;

    public Transform _leftTransform, _rightTransform;

    [SerializeField] private float LogicalToVisualScaleFactor = 0.4f;

    public void SetDefaultColor()
    {
        _square.color = _defaultColor;
    }

    public void SetHighlightedColor()
    {
        _square.color = _highlightedColor;
    }

    // The method no longer needs the collider passed into it
    public void UpdateWidth(float width)
    {
        float visualScaleX = width * LogicalToVisualScaleFactor;

        Vector3 squareScale = _square.transform.localScale;
        squareScale.x = visualScaleX;
        _square.transform.localScale = squareScale;

        // Update the BoxCollider that belongs to this visualizer
        if (_mainCollider != null)
        {
            Vector3 colliderSize = _mainCollider.size;
            colliderSize.x = visualScaleX;
            _mainCollider.size = colliderSize;
        }

        float handleXPosition = visualScaleX / 2f;
        _leftTransform.localPosition = new Vector3(-width/2 - 0.2f/* this is the scale of the lefthandle*/, 0, 0);
        _rightTransform.localPosition = new Vector3(width/2 + 0.2f/* this is the scale of the righthandle*/, 0, 0);
    }

    public void EnableSizeAdjuster()
    {

    }
}