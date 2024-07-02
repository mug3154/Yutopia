using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class MyUnit : MonoBehaviour
{
    [SerializeField] RectTransform _HpBar;

    public UnitData Data { get; private set; }

    float _CurrHp;
    public float CurrentHp => _CurrHp;

    public float Defence = 0;

    GameObject _UnitObj;
    Vector3Int _TilePos;

    public Action<MyUnit, Vector3Int> DisposeCallback;
    public Action<string, GameObject> ObjDisposeCallback;

    public void SetData(UnitData data, GameObject unitObj, Vector3Int tilePos)
    {
        Data = data;
        _TilePos = tilePos;

        _CurrHp = Data.HP;
            
        _HpBar.parent.gameObject.SetActive(Data.HP > 0);

        _UnitObj = unitObj;
        _UnitObj.transform.SetParent(transform);
        _UnitObj.transform.localScale = Vector3.one;
        _UnitObj.transform.localPosition = Vector3.zero;
        _UnitObj.gameObject.SetActive(true);

        DrawHP();
    }

    private void DrawHP()
    {
        if (_CurrHp == 0)
           _HpBar.localScale = Vector3.zero;
        else
           _HpBar.localScale = new Vector3(_CurrHp / Data.HP, 1, 1);
    }

    public void Hit(float damage)
    {
        _CurrHp -= damage;

        if (_CurrHp <= 0)
        {
            Dispose();
        }
        else
        {
            DrawHP();
        }
    }

    private void Dispose()
    {
        DisposeCallback?.Invoke(this, _TilePos);

        ObjDisposeCallback?.Invoke(Data.ResourceName, _UnitObj);
        _UnitObj = null;

    }
}
