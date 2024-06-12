using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SummonUnitCellInfo : MonoBehaviour
{
    [SerializeField] protected Image _Icon;

    public UnitData Data;

    protected int _UseCount = 0;

    public virtual void SetData(UnitData data)
    {
        Data = data;

        _Icon.sprite = BattleManager.Instance.BattleAtlas.GetSprite(data.IconName);
    }

    public virtual void Use()
    {
        _UseCount++;
    }

    public virtual bool Usable()
    {
        return _UseCount == 0;
    }
}
