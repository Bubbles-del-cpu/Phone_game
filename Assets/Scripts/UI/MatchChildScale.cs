using UnityEngine;

public class MatchChildScale : MonoBehaviour
{
    public RectTransform parent;
    public RectTransform[] children;

    [SerializeField] private Vector2 _padding;
    [SerializeField] private bool _includeWidth;
    [SerializeField] private bool _paddingPerChild = false;
    [SerializeField] private bool _computeTotal = false;
    private float _maxY = 0;
    private float _maxX = 0;

    void Update()
    {
        SetMinMaxValues();
        FitToChildren();
    }

    private void FitToChildren()
    {
        parent.sizeDelta = GetNewRect();
        //parent.anchoredPosition = GetTopLeftCornerPositon();
    }

    private void SetMinMaxValues()
    {
        var tempMaxY = 0f;
        var tempMaxX = 0f;
        _maxY = 0;
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].gameObject.activeInHierarchy == false)
                continue;

            tempMaxY = children[i].sizeDelta.y;
            tempMaxX = children[i].sizeDelta.x;
            if (_computeTotal)
            {
                _maxY += tempMaxY;
                if (_includeWidth)
                    _maxX += tempMaxX;
            }
            else
            {
                if (tempMaxY > _maxY)
                    _maxY = tempMaxY;

                if (_includeWidth && tempMaxX > _maxX)
                    _maxX = tempMaxX;
            }
        }
    }

    private Vector2 GetNewRect()
    {
        return new Vector2(_includeWidth ? _maxX : parent.sizeDelta.x, _maxY) + (_paddingPerChild ? _padding * children.Length : _padding);
    }
}
