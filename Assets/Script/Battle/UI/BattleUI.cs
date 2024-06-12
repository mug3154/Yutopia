using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleUI : MonoBehaviour
{
    [SerializeField] BattleUITop _Top;

    [SerializeField] DragUnit _DragUnit;

    public void Setting(MapData data)
    {
        _Top.Initialize(data);
    }


    public void CreateDragUnit(ref UnitData data)
    {
        _DragUnit.SetData(ref data);
    }

    public void InvisibleDragUnit()
    {
        _DragUnit.gameObject.SetActive(false);
    }

}
