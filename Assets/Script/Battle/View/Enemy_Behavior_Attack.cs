using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class Enemy_Behavior_Attack
{

    protected IDisposable _AttackDisposal;
    protected float _AttackDelay = 0;
    protected float _AttackDelayOrigin = 0;
    protected float _AttackDMG = 0;

    protected List<MyUnit> _AttackTargets = new List<MyUnit>();

    protected Enemy_Behavior _Behavior;

    public bool Initialize(Enemy_Behavior behavior)
    {
        _Behavior = behavior;

        _AttackDelayOrigin = 0.5f;
        _AttackDMG = 1f;

        return true;
    }

    public virtual void CheckAttackTarget()
    {
        _AttackTargets.Clear();

        foreach (var attackPos in _Behavior.Enemy.TableData.attack_range)
        {
            Vector3Int tilePos = GetConvertAttackDirectionPos(_Behavior.CurrTilePos, attackPos);

            var unit = BattleManager.Instance.CurrentTileDatas[tilePos.x, tilePos.y].MyUnit;
            if (unit == null) continue;
            if (unit.Data.Type == UNIT_TYPE.OBSTACLES) continue;

            _AttackTargets.Add(unit);
        }
    }

    protected Vector3Int GetConvertAttackDirectionPos(Vector3Int currPos, Vector3Int attackPos)
    {
        Vector3Int result;

        if (_Behavior.MoveDirection == Vector2Int.right)
        {
            result = currPos + attackPos;
        }
        else if (_Behavior.MoveDirection == Vector2Int.left)
        {
            result = new Vector3Int(currPos.x - attackPos.x, currPos.y + attackPos.y);
        }
        else if (_Behavior.MoveDirection == Vector2Int.up)
        {
            result = new Vector3Int(currPos.x - attackPos.y, currPos.y + attackPos.x);
        }
        else
        {
            result = new Vector3Int(currPos.x + attackPos.y, currPos.y - attackPos.x);
        }

        return result;
    }


    public void Start()
    {
        _AttackDelay = _AttackDelayOrigin;

        _AttackDisposal ??= Observable.EveryFixedUpdate().Subscribe(CountingAttackDelay);

    }
    public void Stop()
    {
        _AttackDisposal?.Dispose();
        _AttackDisposal = null;
    }

    private void CountingAttackDelay(long count)
    {
        _AttackDelay -= Time.fixedDeltaTime;
        if (_AttackDelay > 0)
            return;

        _AttackDelay = _AttackDelayOrigin;

        if (_AttackTargets.Count == 0)
        {
            Stop();

            _Behavior.SetBehavior();
        }
        else
        {
            Attack();
        }
    }

    protected abstract void Attack();

    public abstract MyUnit GetTarget();
}
