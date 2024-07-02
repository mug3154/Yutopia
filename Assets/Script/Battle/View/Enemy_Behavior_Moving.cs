using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class Enemy_Behavior_Moving
{
    protected IDisposable _MoveDisposal;

    protected float _MoveSpeedPerSec;
    protected float _MoveSec;

    protected Vector3 _CurrPosition;
    protected Vector3 _TargetPosition;

    //반드시 거쳐가야하는 경로. 갈 수 없는 포인트가 되거나 지나간 경우 _TargetRoutes에서 삭제하여 현재 위치에서부터 _TargetRoutes값을 참고하여 다시 경로를 찾게 한다.
    List<Vector3Int> _TargetRoutes = new List<Vector3Int>();


    Enemy_Behavior _Behavior;

    public bool Initialize(Enemy_Behavior behavior)
    {
        _Behavior = behavior;

        _Behavior.CurrTilePos = BattleManager.Instance.GetRespawnPosition(_Behavior.Enemy.Data.RespawnIdx);
        if (_Behavior.CurrTilePos == BattleConfig.TrashValueVector3Int)
        {
            return false;
        }

        SetTargetRouteTiles(_Behavior.CurrTilePos);

        _Behavior.Enemy.transform.position = BattleConfig.GetTilePosition(_Behavior.CurrTilePos);

        _Behavior.CurrTilePos = _TargetRoutes[0];
        _TargetRoutes.RemoveAt(0);

        if (_TargetRoutes.Count == 0)
        {
            return false;
        }

        _MoveSpeedPerSec = 0.9f / _Behavior.Enemy.Data.MoveSpeed;

        return true;
    }

    public void SetTargetRouteTiles(Vector3Int startPos)
    {
        _TargetRoutes = BattleConfig.GetEnemyRouteTilePos(ref _Behavior.Enemy.Data, startPos, ref BattleManager.Instance.CurrentTileDatas, _Behavior.Enemy.TableData.move_type);
    }

    public Vector2Int GetMoveDirection()
    {
        return GetMoveDirection(_Behavior.CurrTilePos, _TargetRoutes[0]);
    }

    private Vector2Int GetMoveDirection(Vector3Int curr, Vector3Int next)
    {
        if (curr.x < next.x) return Vector2Int.right;
        else if (curr.x > next.x) return Vector2Int.left;
        else if (curr.y < next.y) return Vector2Int.up;
        else return Vector2Int.down;
    }


    public void Start()
    {
        if (_TargetRoutes.Count == 0)
            return;

        if (_MoveDisposal == null)
        {
            Vector3 newTarget = BattleConfig.GetTilePosition(_TargetRoutes[0]);
            if (_TargetPosition != newTarget)
            {
                _MoveSec = 0;
                _CurrPosition = _Behavior.Enemy.transform.position;
                _TargetPosition = newTarget;
            }

            _MoveDisposal = Observable.EveryFixedUpdate().Subscribe(Move);
        }
    }

    public void Stop()
    {
        _MoveDisposal?.Dispose();
        _MoveDisposal = null;
    }

    private void Move(long count)
    {
        _MoveSec += Time.fixedDeltaTime;

        _Behavior.Enemy.transform.position = Vector2.Lerp(_CurrPosition, _TargetPosition, _MoveSec / _MoveSpeedPerSec);

        if (_Behavior.Enemy.transform.position == _TargetPosition)
        {
            _MoveSec = 0;

            _Behavior.CurrTilePos = _TargetRoutes[0];
            _TargetRoutes.RemoveAt(0);

            if (_TargetRoutes.Count == 0)
            {
                _Behavior.Enemy.gameObject.SetActive(false);

                BattleManager.Instance.RemainLifeCount.Value -= 1;

                _Behavior.Enemy.Dispose();
            }
            else
            {
                Stop();

                _Behavior.SetBehavior();
            }
        }
    }

    public void RecheckRoute(Vector3Int userUnitPos)
    {
        if (_TargetRoutes.Count == 0)
            return;

        if (_TargetRoutes.Contains(userUnitPos))
        {
            _Behavior.AllBehaviorStop();

            SetTargetRouteTiles(_TargetRoutes[0]);

            _Behavior.CurrTilePos = _TargetRoutes[0];
            _TargetRoutes.RemoveAt(0);


            _Behavior.SetBehavior();
        }
    }

}
