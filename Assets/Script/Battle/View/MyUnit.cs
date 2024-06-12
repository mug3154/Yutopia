using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyUnit : MonoBehaviour
{
    [SerializeField] SpriteRenderer _SpR;

    [SerializeField] RectTransform _HpBar;

    UnitData _Data;

    float _CurrHp;

    public void SetData(UnitData data)
    {
        _Data = data;

        _SpR.sprite = BattleManager.Instance.BattleAtlas.GetSprite(data.ResourceName);

        _CurrHp = _Data.HP;
            
        _HpBar.parent.gameObject.SetActive(_Data.HP > 0);

        DrawHP();
    }

    private void DrawHP()
    {
        if (_CurrHp == 0)
           _HpBar.localScale = Vector3.zero;
        else
           _HpBar.localScale = new Vector3(_CurrHp / _Data.HP, 1, 1);
    }
}
