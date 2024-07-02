using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;

public class BattleUIBottom : MonoBehaviour
{
    ObjectPool<SummonUnitCell> _Pool;
    ObjectPool<SummonUnitCellInfo> _InfoPool;
    ObjectPool<SummonUnitCellInfo_ETC> _InfoETCPool;

    List<SummonUnitCell> _Cells;

    public void LoadingResources(Action<float> progressCallback)
    {
        if(_Pool == null )
        {
            var handler = Addressables.LoadAssetAsync<GameObject>("SummonUnitCell").WaitForCompletion();
            _Pool = new ObjectPool<SummonUnitCell>(()=> Instantiate(handler.GetComponent<SummonUnitCell>(), transform));
            Addressables.Release(handler);

            progressCallback?.Invoke(0.3f);

            var handler2 = Addressables.LoadAssetAsync<GameObject>("SummonUnitCellInfo").WaitForCompletion();
            _InfoPool = new ObjectPool<SummonUnitCellInfo>(() => Instantiate(handler2.GetComponent<SummonUnitCellInfo>(), transform));
            Addressables.Release(handler2);
            
            progressCallback?.Invoke(0.3f);

            var handler3 = Addressables.LoadAssetAsync<GameObject>("SummonUnitCellInfo_ETC").WaitForCompletion();
            _InfoETCPool = new ObjectPool<SummonUnitCellInfo_ETC>(() => Instantiate(handler3.GetComponent<SummonUnitCellInfo_ETC>(), transform));
            Addressables.Release(handler3);

            progressCallback?.Invoke(1f);

        }
        else
        {
            progressCallback?.Invoke(1);
        }
    }


    public void Initialize(MapData data)
    {
        if(_Cells == null )
        {
            _Cells = new List<SummonUnitCell>();
        }
        else
        {
            Dispose();
        }

        int cnt = 0;

        var unitData = data.UnitData;
        foreach(var unit in unitData)
        {
            var cell = _Pool.Get();

            if(unit.Type == UNIT_TYPE.OBSTACLES || unit.Type == UNIT_TYPE.DISPOSABLE_OBSTACLES)
            {
                cell.SetData(unit, _InfoETCPool.Get());
            }
            else
            {
                cell.SetData(unit, _InfoPool.Get());
            }

            cell.transform.localScale = Vector3.one;
            cell.GetComponent<RectTransform>().anchoredPosition = new Vector2((150 * ((cnt * -1) - 0.5f)) - (10 * cnt), 75f);

            _Cells.Add(cell);

            cnt++;
        }
    }

    public void Dispose()
    {
        if(_Cells != null)
        {
            foreach( var cell in _Cells)
            {
                var info = cell.Info;
                info.transform.SetParent(transform);
                info.gameObject.SetActive(false);

                if (info as SummonUnitCellInfo_ETC != null)
                {
                    _InfoETCPool.Release(info as SummonUnitCellInfo_ETC);
                }
                else
                {
                    _InfoPool.Release(info as SummonUnitCellInfo);
                }

                cell.Info = null;

                cell.gameObject.SetActive(false);
                _Pool.Release(cell);
            }

            _Cells.Clear();
        }
    }
    

}
