using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    [SerializeField]
    private Pool[] poolArray;

    private Transform _objectPoolTransform;

    private Dictionary<int, List<Component>> _poolDictionary = new Dictionary<int, List<Component>>();

    public static PoolManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    [System.Serializable]
    public struct Pool
    {
        public GameObject prefab;
        public string componentType;
    }

    private void Start()
    {
        _objectPoolTransform = this.gameObject.transform;

        foreach (var pool in poolArray)
        {
            CreatePool(pool.prefab, pool.componentType);
        }
    }

    private void CreatePool(GameObject prefab, string componentType)
    {
        int poolKey = prefab.GetInstanceID();
        string prefabName = prefab.name;
        GameObject parentGameObject = new GameObject(prefabName + "Anchor");
        parentGameObject.transform.SetParent(_objectPoolTransform);

        if (!_poolDictionary.ContainsKey(poolKey))
        {
            _poolDictionary.Add(poolKey, new List<Component>());
        }
    }

    private void AddNewObjectToPool(GameObject prefab, int poolKey, Transform parent, string componentType)
    {
        GameObject newObject = Instantiate(prefab, parent);
        newObject.SetActive(false);
        _poolDictionary[poolKey].Add(newObject.GetComponent(Type.GetType(componentType)));
    }

    public Component ReuseComponent(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        int poolKey = prefab.GetInstanceID();
        if (_poolDictionary.ContainsKey(poolKey))
        {
            Component componentToReuse = GetComponentFromPool(prefab, poolKey);
            ResetObject(position, rotation, componentToReuse, prefab);
            return componentToReuse;
        }
        else
        {
            Debug.Log("No Object Pool for " + prefab);
            return null;
        }
    }

    private Component GetComponentFromPool(GameObject prefab, int poolKey)
    {
        Component componentToReuse = _poolDictionary[poolKey].Find(component => !component.gameObject.activeSelf);

        if (componentToReuse == null)
        {
            Transform parent = _objectPoolTransform.Find(prefab.name + "Anchor");
            AddNewObjectToPool(prefab, poolKey, parent, prefab.GetComponent<MonoBehaviour>().GetType().Name);
            componentToReuse = _poolDictionary[poolKey][_poolDictionary[poolKey].Count - 1];
        }

        return componentToReuse;
    }

    private void ResetObject(Vector3 position, Quaternion rotation, Component componentToReuse, GameObject prefab)
    {
        componentToReuse.transform.position = position;
        componentToReuse.transform.rotation = rotation;
        componentToReuse.gameObject.transform.localScale = prefab.transform.localScale;
        componentToReuse.gameObject.SetActive(true);
    }
}
