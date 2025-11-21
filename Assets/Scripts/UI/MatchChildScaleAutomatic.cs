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
    [SerializeField] private bool _computeTotalY = false;
    [SerializeField] private bool _computeTotalX = false;
    [SerializeField] private bool _paddingPerChild = false;
    [SerializeField] private bool _includeWidth = false;
    [SerializeField] private Vector2 _padding;

    private void Awake()
    {
        _self = GetComponent<RectTransform>();
        UpdateSize();
    }

    private void Update()
    {
        if (_autoUpdate)
            UpdateSize();
    }

    private void UpdateChildren()
    {
        _children.Clear();
        for(var index =0 ; index < transform.childCount; index++)
        {
            var child = transform.GetChild(index).GetComponent<RectTransform>();
            if(child != null && !_children.Contains(child))
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
        Vector2 maxSize = Vector2.zero;
        Vector2 tempSize = Vector2.zero;
        for (int i = 0; i < _children.Count; i++)
        {
            if (_children[i].gameObject.activeInHierarchy == false)
                continue;

            tempSize.y = _children[i].sizeDelta.y;
            if (_computeTotalY)
            {
                maxSize.y += tempSize.y;
            }
            else
            {
                if (tempSize.y > maxSize.y)
                    maxSize.y = tempSize.y;
            }

            tempSize.x = _children[i].sizeDelta.x;
            if (_computeTotalX)
            {
                maxSize.x += tempSize.x;
            }
            else
            {
                if (tempSize.x > maxSize.x)
                    maxSize.x = tempSize.x;
            }
        }

        _self.sizeDelta = new Vector2(_includeWidth ? maxSize.x : _self.sizeDelta.x, maxSize.y) + (_paddingPerChild ? _padding * _children.Count : _padding);
    }
}