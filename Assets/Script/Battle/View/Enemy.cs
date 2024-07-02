using System;
using UnityEngine;
using static Table_Monster;

public class Enemy : MonoBehaviour
{
    [SerializeField] SpriteRenderer _SpriteRenderer;

    Action<Enemy> _DisposeCallback;


    public MonsterData Data;

    public Enemy_Behavior Behavior {  get; private set; }
    public Table_MonsterData TableData { get; private set; }


    public void Create(MonsterData data, Action<Enemy> disposeCallback)
    {
        Data = data;
        _DisposeCallback = disposeCallback;

        TableData = Table_Monster.Instance.GetData(Data.MonsterId);

        _SpriteRenderer.sprite = BattleManager.Instance.BattleAtlas.GetSprite(TableData.resource_name);

        Behavior = new Enemy_Behavior();

        if(Behavior.Initialize(this) == false)
        {
            Dispose();
            return;
        }

        Behavior.SetBehavior();

        gameObject.SetActive(true);
    }

    public void Dispose()
    {
        Behavior.AllBehaviorStop();

        _DisposeCallback?.Invoke(this);
    }

    
}
