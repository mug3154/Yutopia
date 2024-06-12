using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class Enemy : MonoBehaviour
{
    [SerializeField] SpriteRenderer _SpriteRenderer;

    Action<Enemy> _DisposeCallback;


    protected MonsterData _Data;
    protected IDisposable _MoveDisposal;

    protected float _MoveSpeedPerSec;
    protected float _MoveSec;
    protected Vector3 _CurrPos;
    protected Vector3 _TargetPos;

    //�ݵ�� ���İ����ϴ� ���. �� �� ���� ����Ʈ�� �ǰų� ������ ��� _TargetRoutes���� �����Ͽ� ���� ��ġ�������� _TargetRoutes���� �����Ͽ� �ٽ� ��θ� ã�� �Ѵ�.
    List<Vector3Int> _TargetRoutes = new List<Vector3Int>(); 


    public void Create(MonsterData data, Action<Enemy> disposeCallback)
    {
        _Data = data;
        _DisposeCallback = disposeCallback;

        _SpriteRenderer.sprite = BattleManager.Instance.BattleAtlas.GetSprite($"Enemy_{_Data.ResourceIdx}");

        Vector3Int tilePos = BattleManager.Instance.GetRespawnPosition(data.RespawnIdx);
        if(tilePos == BattleConfig.TrashValueVector3Int)
        {
            Dispose();
            return;
        }

        SetTargetRouteTiles();
       
        transform.position = BattleConfig.GetTilePosition(tilePos);

        _MoveSpeedPerSec = 0.9f / _Data.MoveSpeed;
        
        _MoveSec = 0;
        _CurrPos = transform.position;
        _TargetPos = BattleConfig.GetTilePosition(_TargetRoutes[0]);

        if (_MoveDisposal == null)
            _MoveDisposal = Observable.EveryFixedUpdate().Subscribe(Move);

        gameObject.SetActive(true);
    }

    private void Move(long count)
    {
        _MoveSec += Time.fixedDeltaTime;

        transform.position = Vector2.Lerp(_CurrPos, _TargetPos, _MoveSec / _MoveSpeedPerSec);

        if(transform.position == _TargetPos)
        {
            _MoveSec = 0;
            _TargetRoutes.RemoveAt(0);

            if(_TargetRoutes.Count == 0)
            {
                gameObject.SetActive(false);

                BattleManager.Instance.RemainLifeCount.Value -= 1;

                Dispose();
            }
            else
            {
                _CurrPos = transform.position;
                _TargetPos = BattleConfig.GetTilePosition(_TargetRoutes[0]);
            }
        }
    }

    protected void SetTargetRouteTiles()
    {
        _TargetRoutes = BattleConfig.GetEnemyRouteTilePos(ref _Data, ref BattleManager.Instance.CurrentTileDatas);
    }


    protected void Dispose()
    {
        _MoveDisposal?.Dispose();
        _MoveDisposal = null;

        _DisposeCallback?.Invoke(this);
    }

    
}
