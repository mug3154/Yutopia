using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SummonUnitCellInfo_ETC : SummonUnitCellInfo
{
    [SerializeField] protected TextMeshProUGUI _CountText;

    public override void SetData(UnitData data)
    {
        base.SetData(data);
        
        DrawCount();
    }

    public override void Use()
    {
        base.Use();

        DrawCount();
    }

    private void DrawCount()
    {
        _CountText.text = $"{Data.Count - _UseCount}/{Data.Count}";
    }

    public override bool Usable()
    {
        return Data.Count != _UseCount;
    }
}
