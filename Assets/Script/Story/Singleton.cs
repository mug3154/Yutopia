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
            // ��Ʈ ��ü ���� ������
            // ������ ������ leak�� �����Ѵ�.
            if (_applicationQuit)
            {
                // null ����
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

    // ���� ����ɶ� ȣ��
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

    // ��ü�� �ı��ɶ� ȣ��
    public virtual void OnDestroy()
    {
        _applicationQuit = false;
    }
}
