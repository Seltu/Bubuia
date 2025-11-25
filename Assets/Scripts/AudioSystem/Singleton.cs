using UnityEngine;

/// <summary>
/// A robust, generic Singleton base class for Unity.
/// Derive from this class to create your Singleton.
/// </summary>
/// <typeparam name="T">The type of the Singleton class.</typeparam>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();

    /// <summary>
    /// Accessor for the Singleton instance.
    /// </summary>
    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting)
            {
                Debug.LogWarning($"[Singleton] Instance of {typeof(T)} is already destroyed. Returning null.");
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();

                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject(typeof(T).Name);
                        _instance = singletonObject.AddComponent<T>();
                        Debug.Log($"[Singleton] An instance of {typeof(T)} was created automatically.");
                    }
                }

                return _instance;
            }
        }
    }

    private static bool _applicationIsQuitting = false;

    /// <summary>
    /// Optional flag to make the Singleton persist across scenes.
    /// </summary>
    [SerializeField] private bool _dontDestroyOnLoad = false;

    /// <summary>
    /// Ensures the Singleton instance is initialized and unique.
    /// </summary>
    protected virtual void Awake()
    {
        lock (_lock)
        {
            if (_instance == null)
            {
                _instance = this as T;
                if (_dontDestroyOnLoad)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else if (_instance != this)
            {
                //Debug.LogWarning($"[Singleton] Duplicate instance of {typeof(T)} detected. Destroying: {gameObject.name}");
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Set the application quitting flag when the application quits.
    /// </summary>
    protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }

    /// <summary>
    /// Cleans up the Singleton instance on destroy.
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
