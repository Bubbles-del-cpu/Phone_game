using UnityEngine;

public class MatchChildScale : MonoBehaviour
{
    public RectTransform parent;
    public RectTransform[] children;

    [SerializeField] private bool _computeTotal = false;
    private float _maxY = 0;

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
        _maxY = 0;
        for (int i = 0; i < children.Length; i++)
        {
            tempMaxY = children[i].sizeDelta.y;
            if (_computeTotal)
            {
                _maxY += tempMaxY;
            }
            else
            {
                if (tempMaxY > _maxY)
                    _maxY = tempMaxY;
            }
        }
    }

    private Vector2 GetNewRect()
    {
        return new Vector2(parent.sizeDelta.x, _maxY);
    }
}
