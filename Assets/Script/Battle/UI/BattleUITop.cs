using System;
using TMPro;
using UnityEngine;
using UniRx;

public class BattleUITop : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _EnemyText;
    [SerializeField] TextMeshProUGUI _LifeText;
    [SerializeField] TextMeshProUGUI _TimeText;

    IDisposable _RemainEnemyDisposal;
    IDisposable _RemainLifeDisposal;
    IDisposable _RemainTimeDisposal;

    int _TotalMonsterCount;
    int _TotalLifeCount;
    int _TotalTimeCount;

    public void Initialize(MapData data)
    {
        _TotalMonsterCount = data.MonsterData.Length;
        _TotalLifeCount = data.LifeCount;
        _TotalTimeCount = data.BattleSec;

        _EnemyText.text = $"{_TotalMonsterCount}/{_TotalMonsterCount}";
        _LifeText.text = $"{_TotalLifeCount}/{_TotalLifeCount}";
        _TimeText.text = $"{_TotalTimeCount}/{_TotalTimeCount}";


        _RemainEnemyDisposal = BattleManager.Instance.RemainEnemyCount.Subscribe(DrawEnemyCount);
        _RemainLifeDisposal = BattleManager.Instance.RemainLifeCount.Subscribe(DrawLifeCount);
        _RemainTimeDisposal = BattleManager.Instance.RemainTimeSec.Subscribe(DrawTimeCount);
    }

    public void DrawEnemyCount(int count)
    {
        _EnemyText.text = $"{count}/{_TotalMonsterCount}";
    }

    public void DrawLifeCount(int count)
    {
        _LifeText.text = $"{count}/{_TotalLifeCount}";
    }

    public void DrawTimeCount(double count)
    {
        _TimeText.text = $"{(int)count}/{_TotalTimeCount}";
    }

    private void OnDestroy()
    {
        _RemainEnemyDisposal?.Dispose();
        _RemainEnemyDisposal = null;

        _RemainLifeDisposal?.Dispose();
        _RemainLifeDisposal = null;

        _RemainTimeDisposal?.Dispose();
        _RemainTimeDisposal = null;
    }
}
