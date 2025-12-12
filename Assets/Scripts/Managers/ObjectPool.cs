using System.Collections;
using System.Collections.Generic;
using MeetAndTalk;
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
        DialogueUIManagerObjectPool.Instance.StartCoroutine(CreateObjects());
    }

    private IEnumerator CreateObjects()
    {
        var countBeforePause = 20;
        var count = 0;
        for (int index = 0; index < _increaseSize; index++)
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
        obj.gameObject.SetActive(false);

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
            obj.gameObject.SetActive(false);

            _availableObjects.Add(obj);
        }
    }

    public T GetObject()
    {
        if (_availableObjects.Count <= 0)
        {
            // Pool is empty, create some extras as they are needed
            for (int i = 0; i < _increaseSize; i++)
            {
                Debug.LogWarning($"Object Pool of type {typeof(T)} is empty, increasing pool size by {_increaseSize}");
                _availableObjects.Add(CreateNewObject());
            }
        }

        T obj = _availableObjects[0];
        _availableObjects.RemoveAt(0);
        return obj;
    }
}