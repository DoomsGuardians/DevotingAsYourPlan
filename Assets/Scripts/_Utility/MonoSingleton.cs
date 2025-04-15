using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static bool IsExisted { get; private set; } = false;
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<T>();
                if (instance == null)
                {
                    var singletonObject = new GameObject(typeof(T).Name);
                    instance = singletonObject.AddComponent<T>();
                    IsExisted = true;
                }
            }
            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            if(IsNotDestroyOnLoad())
            {
                DontDestroyOnLoad(gameObject);
            }
            IsExisted = true;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public virtual void OnDestroy()
    {
        if (instance == this)
        {
            IsExisted = false;
        }
    }

    public virtual bool IsNotDestroyOnLoad()
    {
        return true;
    }
}