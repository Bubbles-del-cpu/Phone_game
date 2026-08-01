using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class MatchChildScaleAutomatic : MonoBehaviour
{
    private RectTransform _self;
    [SerializeField] private List<RectTransform> _children;
    [SerializeField] private bool _autoUpdate = true;
    [SerializeField] private bool _autoCollectChildren = true;
    [SerializeField] private Vector2 _padding;

    [Header("Calculation Settings")]
    [SerializeField] private bool _paddingPerChild = false;
    [SerializeField] private bool _useSizeOnly = false;
    [SerializeField] private bool _computeTotalY = false;
    [SerializeField] private bool _computeTotalX = false;

    private void Awake()
    {
        _self = GetComponent<RectTransform>();
        UpdateSize();
    }

    private void Update()
    {
        if (_autoUpdate)
        {
            if (_autoCollectChildren)
                UpdateChildren();

            ComputeSize();
        }
    }

    private void UpdateChildren()
    {
        _children.Clear();
        for (var index = 0; index < transform.childCount; index++)
        {
            var child = transform.GetChild(index).GetComponent<RectTransform>();
            if (child != null && !_children.Contains(child))
            {
                _children.Add(child);
            }
        }
    }

    [ContextMenu("Update Size")]
    public void UpdateSize()
    {
        if (_autoCollectChildren)
            UpdateChildren();

        StartCoroutine(CoUpdateSizeNextFrame());
    }

    private IEnumerator CoUpdateSizeNextFrame()
    {
        yield return null;
        if (_children.Count == 0)
        {
            _self.sizeDelta = _padding;
            yield break;
        }

        ComputeSize();
    }

    private void ComputeSize()
    {
        if (!_self)
            _self = GetComponent<RectTransform>();

        if (_useSizeOnly)
            ComputeSizeUsingSizeOnly();
        else
            ComputeSizeUsingPosition();
    }

    /// <summary>
    /// Computes the size required to encompass all child RectTransforms using their positions and sizeDelta values
    /// </summary>
    private void ComputeSizeUsingPosition()
    {
        Vector2 maxSize = new Vector2(0, 0);
        Vector2 minSize = new Vector2(float.MaxValue, float.MaxValue);

        // Loop the children and grab that min and max X and Y positions, then set the sizeDelta of this rect transform to match
        for (int i = 0; i < _children.Count; i++)
        {
            if (_children[i] == null || _children[i].gameObject == null)
                continue;

            if (_children[i].gameObject.activeInHierarchy == false)
                continue;

            var posX = Mathf.Abs(_children[i].anchoredPosition.x);
            var posY = Mathf.Abs(_children[i].anchoredPosition.y);
            var width = _children[i].sizeDelta.x + _padding.x;
            var height = _children[i].sizeDelta.y + _padding.y;

            if (posX < minSize.x)
                minSize.x = posX;

            if (posY < minSize.y)
                minSize.y = posY;

            if (posX + width > maxSize.x)
                maxSize.x = posX + width;

            if (posY + height > maxSize.y)
                maxSize.y = posY + height;
        }

        _self.sizeDelta = new Vector2(_computeTotalX ? (maxSize.x - minSize.x) : _self.sizeDelta.x,
                                      _computeTotalY ? (maxSize.y - minSize.y) : _self.sizeDelta.y);
    }

    /// <summary>
    /// Computes the size required to encompass all child RectTransforms using only their sizeDelta values
    /// </summary>
    private void ComputeSizeUsingSizeOnly()
    {
        float totalX = 0;
        float totalY = 0;
        float largestX = 0;
        float largestY = 0;

        for (int i = 0; i < _children.Count; i++)
        {
            if (_children[i] == null || _children[i].gameObject == null)
                continue;

            if (_children[i].gameObject.activeInHierarchy == false)
                continue;

            if (_computeTotalX)
                totalX += _children[i].sizeDelta.x + (_paddingPerChild ? _padding.x : 0);
            else if (_children[i].sizeDelta.x > largestX)
                largestX = _children[i].sizeDelta.x;

            if (_computeTotalY)
                totalY += _children[i].sizeDelta.y + (_paddingPerChild ? _padding.y : 0);
            else if (_children[i].sizeDelta.y > largestY)
                largestY = _children[i].sizeDelta.y;
        }

        _self.sizeDelta = new Vector2((_computeTotalX ? totalX : largestX) + (_paddingPerChild ? 0 : _padding.x),
                                      (_computeTotalY ? totalY : largestY) + (_paddingPerChild ? 0 : _padding.y));
    }
}