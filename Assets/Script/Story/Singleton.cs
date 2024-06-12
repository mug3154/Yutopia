using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// https://icat2048.tistory.com/426
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static bool _applicationQuit = false;
    private static object _lock = new object();

    static T _Instance;
    public static T Instance
    {
        get
        {
            // 고스트 객체 생성 방지용
            // 위에서 설명한 leak을 방지한다.
            if (_applicationQuit)
            {
                // null 리턴
                return null;
            }

            lock (_lock)
            {

                if (_Instance == null)
                {
                    _Instance = Object.FindAnyObjectByType<T>();

                    if (_Instance == null)
                    {
                        GameObject go = new GameObject(typeof(T).Name);
                        _Instance = go.AddComponent<T>();
                    }

                    DontDestroyOnLoad(_Instance.gameObject);
                }
            }
            return _Instance;
        }
    }

    private void Awake()
    {
        if( _Instance != null && _Instance != this )
        {
            Destroy( gameObject );
            return;
        }

        _Instance = GetComponent<T>();
        DontDestroyOnLoad(gameObject);
    }

    // 앱이 종료될때 호출
    protected virtual void OnApplicationQuit()
    {
        _applicationQuit = true;
    }

    public void Destroy()
    {
        Destroy(_Instance);
        Destroy(gameObject);
        _Instance = null;
    }

    // 객체가 파괴될때 호출
    public virtual void OnDestroy()
    {
        _applicationQuit = false;
    }
}
