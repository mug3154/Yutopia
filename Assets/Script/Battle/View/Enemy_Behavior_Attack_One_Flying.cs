using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Behavior_Attack_One_Flying : Enemy_Behavior_Attack_One
{
    public override void CheckAttackTarget()
    {
        _AttackTargets.Clear();

        foreach (var attackPos in _Behavior.Enemy.TableData.attack_range)
        {
            Vector3Int tilePos = GetConvertAttackDirectionPos(_Behavior.CurrTilePos, attackPos);

            var unit = BattleManager.Instance.CurrentTileDatas[tilePos.x, tilePos.y].MyUnit;
            if (unit == null) continue;
            if (unit.Data.Type == UNIT_TYPE.OBSTACLES) continue;
            if (unit.Data.Type == UNIT_TYPE.DISPOSABLE_OBSTACLES) continue;

            _AttackTargets.Add(unit);
        }
    }
}
