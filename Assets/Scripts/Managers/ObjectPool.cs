using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    private List<T> _availableObjects = new List<T>();
    private HashSet<T> _allObjects = new HashSet<T>();
    private GameObject _prefab;
    private Transform _poolContainer;
    private int _startingSize;
    private int _increaseSize;

    public int AvailableCount => _availableObjects.Count;
    public int TotalCount => _allObjects.Count;
    public int InUseCount => TotalCount - AvailableCount;
    public int IncreasedSize => TotalCount - _startingSize;

    public ObjectPool(int poolMax, int increaseSize, GameObject prefab, Transform poolContainer)
    {
        _prefab = prefab;
        _startingSize = poolMax;
        _increaseSize = increaseSize;
        _poolContainer = poolContainer;

        // Initialize the object pool
        DialogueUIManagerObjectPool.Instance.StartCoroutine(CreateObjects(_startingSize));
    }

    private IEnumerator CreateObjects(int amount)
    {
        var countBeforePause = 20;
        var count = 0;
        for (int index = 0; index < amount; index++)
        {
            _availableObjects.Add(CreateNewObject());
            count++;
            if (count >= countBeforePause)
            {
                count = 0;
                yield return null;
            }
        }
    }

    private T CreateNewObject()
    {
        var obj = GameObject.Instantiate(_prefab, _poolContainer);
        var component = obj.GetComponent<T>();
        _allObjects.Add(component);
        return component;
    }

    public void ReturnObject(T obj)
    {
        if (obj == null || !_allObjects.Contains(obj))
            return;

        if (!_availableObjects.Contains(obj))
        {
            obj.gameObject.transform.SetParent(_poolContainer);
            _availableObjects.Add(obj);
        }
    }

    public T GetObject()
    {
        if (_availableObjects.Count <= 0)
        {
            // Pool is empty, create some extras as they are needed
            DialogueUIManagerObjectPool.Instance.StartCoroutine(CreateObjects(_increaseSize));
        }

        T obj = _availableObjects[0];
        _availableObjects.RemoveAt(0);
        return obj;
    }
}