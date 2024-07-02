using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Behavior_Attack_One : Enemy_Behavior_Attack
{

    protected override void Attack()
    {
        var target = GetTarget();
        target.Hit(_AttackDMG);

        if (target.CurrentHp <= 0)
        {
            _AttackTargets.Remove(target);

            if (_AttackTargets.Count == 0)
            {
                Stop();

                _Behavior.SetBehavior();
            }
        }
    }

    public override MyUnit GetTarget()
    {
        int count = _AttackTargets.Count;
        if (count == 0)
            return null;

        if(count == 1)
            return _AttackTargets[0];

        //HP
        _AttackTargets.Sort((MyUnit a, MyUnit b) =>
        {
            if (a.CurrentHp > b.CurrentHp) return -1;
            else if (a.CurrentHp < b.CurrentHp) return 1;
            else return 0;
        });

        if (_AttackTargets[0].CurrentHp == _AttackTargets[1].CurrentHp)
        {
            //Defence
            if (_AttackTargets[0].Defence > _AttackTargets[1].Defence)
                return _AttackTargets[0];
            else
                return _AttackTargets[1];
        }
        else
        {
            return _AttackTargets[0];
        }
    }
}
