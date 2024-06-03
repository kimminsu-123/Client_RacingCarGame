using System;
using UnityEngine;

public class SingletonMonobehavior<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance != null) return _instance;
                
                _instance = FindObjectOfType<T>();
                if (_instance != null) return _instance;
                
                GameObject newObj = new GameObject($"{typeof(T)}");
                _instance = newObj.AddComponent<T>();

                return _instance;
            }
        }
    }
    
    private static T _instance;
    private static object _lock = new();
}