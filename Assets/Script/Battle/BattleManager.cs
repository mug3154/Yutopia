using System;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

public class BattleManager : Singleton<BattleManager>
{
    [SerializeField] BattleView _View;
    public BattleView View => _View;

    [SerializeField] BattleUI _UI;
    [SerializeField] BattleDragUnitUI _DragUnitUI;
    public BattleDragUnitUI DragUnitUI => _DragUnitUI;


    public SpriteAtlas BattleAtlas;


    public MapData MapData { get; private set; }
    public CurrentTileData[,] CurrentTileDatas; 

    public ReactiveProperty<int> RemainEnemyCount { get; private set; } = new ReactiveProperty<int>();
    public ReactiveProperty<int> RemainLifeCount { get; private set; } = new ReactiveProperty<int>();
    public ReactiveProperty<double> RemainTimeSec { get; private set; } = new ReactiveProperty<double>();

    IDisposable _UpdateDisposal;
    IDisposable _LifeDisposal;

    Queue<ReadyEnemyData> _EnemyDatas;
    double _PlayTimeSec;

    

    private void Start()
    {
        MapData = new MapData()
        {
            BattleSec = 50,
            LifeCount = 3,

            NormalTileResourceIdx = 0,

            TileDatas = new TileData[]
            {
                new TileData()
                {
                    Type = BATTLE_TILE.DECORATION,
                    ResourceIdx = 0,
                    Pos = new Vector3Int(5, 4)
                },

                new TileData()
                {
                    Type = BATTLE_TILE.DECORATION,
                    ResourceIdx = 0,
                    Pos = new Vector3Int(6, 4)
                }
            },

            GoalTileDatas = new GoalTileData[]
            {
                new GoalTileData()
                {
                    Idx = 0,
                    ResourceIdx = 0,
                    Pos = new Vector3Int(8, 5)
                }
            },

            RespawnTileDatas = new RespawnTileData[]
            {
                new RespawnTileData()
                {
                    Idx = 0,
                    ResourceIdx = 0,
                    Pos = new Vector3Int(1, 1)
                }
            },

            MonsterData = new MonsterData[]
            {
                new MonsterData()
                {
                    Idx = 1,
                    MonsterId = 1,
                    AppearTimeSec = 3,
                    GoalIdx = 0,
                    RespawnIdx = 0,
                    MoveSpeed = 0.5f,
                    Routes = new Vector3Int[]
                    {
                        new Vector3Int()
                        {
                            x = 3,
                            y = 0
                        },
                    }
                },

                new MonsterData()
                {
                    Idx = 2,
                    MonsterId = 2,
                    AppearTimeSec = 6,
                    GoalIdx = 0,
                    RespawnIdx = 0,
                    MoveSpeed = 1f,
                    Routes = new Vector3Int[]
                    {
                        new Vector3Int()
                        {
                            x = 3,
                            y = 5
                        },
                    }
                },

                new MonsterData()
                {
                    Idx = 3,
                    MonsterId = 3,
                    AppearTimeSec = 6,
                    GoalIdx = 0,
                    RespawnIdx = 0,
                    MoveSpeed = 1f,
                    Routes = new Vector3Int[]
                    {
                        new Vector3Int()
                        {
                            x = 6,
                            y = 5
                        },
                    }
                }
            },

            UnitData = new UnitData[]
            {
                new UnitData()
                {
                    Idx = 1,
                    Type = UNIT_TYPE.OBSTACLES,
                    IconName = "Unit_Icon_Obstacle",
                    ResourceName = "Unit_Obstacle",
                    Count = 10,
                },

                new UnitData()
                {
                    Idx = 2,
                    Type = UNIT_TYPE.DISPOSABLE_OBSTACLES,
                    IconName = "Unit_Icon_Disposable_obstacle",
                    ResourceName = "Unit_Disposal_Obstacle",
                    Count = 10,
                    HP = 10
                }
            }
        };


        CreateCurrentTileDatas();

        RemainEnemyCount.Value = MapData.MonsterData.Length;
        RemainLifeCount.Value = MapData.LifeCount;
        RemainTimeSec.Value = MapData.BattleSec;

        _EnemyDatas = new Queue<ReadyEnemyData>();
        foreach(var m in MapData.MonsterData)
        {
            _EnemyDatas.Enqueue(new ReadyEnemyData()
            {
                IsNoticed = false,
                NoticeSec = m.AppearTimeSec - 3,
                Data = m
            });
        }

        _DragUnitUI.gameObject.SetActive(false);
        
        _PlayTimeSec = 0;
        
        _LifeDisposal = RemainLifeCount.Subscribe(CheckLifeCount);

        _TotalResourceStep = 2;
        _CurrentResourceStep = 0;

        _View.LoadingResources(MapData, CheckCompleteLoadResources);
        _UI.LoadingResources(CheckCompleteLoadResources);
    }

    private float _TotalResourceStep;
    private float _CurrentResourceStep;
    private void CheckCompleteLoadResources(float progress)
    {
        if(progress == 1)
        {
            ++_CurrentResourceStep;
        }

        if(_CurrentResourceStep == _TotalResourceStep)
        {
            Initialize();

            GameStart();
        }
    }

    private void Initialize()
    {
        _View.Initialize(MapData);
        _UI.Initialize(MapData);
        _DragUnitUI.Initialize();
    }

    private void GameStart()
    { 
        _UpdateDisposal = this.FixedUpdateAsObservable().Subscribe(PlayGameUpdate);
    }

    private void CreateCurrentTileDatas()
    {
        CurrentTileDatas = new CurrentTileData[16, 10];
        for(int x= 0; x < 16; ++x)
        {
            for(int y = 0; y < 10; ++y)
            {
                CurrentTileDatas[x, y] = new CurrentTileData()
                {
                    Type = BATTLE_TILE.NORMAL,
                    Idx = 0
                };
            }
        }

        foreach (var tileData in MapData.TileDatas)
        {
            CurrentTileDatas[tileData.Pos.x, tileData.Pos.y].Type = tileData.Type;
        }

        CurrentTileData cell;
        foreach (var tileData in MapData.GoalTileDatas)
        {
            cell = CurrentTileDatas[tileData.Pos.x, tileData.Pos.y];
            cell.Type = BATTLE_TILE.GOAL;
            cell.Idx = tileData.Idx;
        }

        foreach (var tileData in MapData.RespawnTileDatas)
        {
            cell = CurrentTileDatas[tileData.Pos.x, tileData.Pos.y];
            cell.Type = BATTLE_TILE.MONSTER_RESPONE;
            cell.Idx = tileData.Idx;
        }
    }


    private void CheckLifeCount(int obj)
    {
        if(obj == 0)
        {
            GameFailure();
        }
    }

    private void PlayGameUpdate(Unit unit)
    {
        if(RemainTimeSec.Value > 0)
        {
            RemainTimeSec.Value -= Time.fixedDeltaTime;
        
            if (RemainTimeSec.Value < 0) 
                RemainTimeSec.Value = 0;
        }

        _PlayTimeSec += Time.fixedDeltaTime;

        CreateMonsters();
    }

    private void CreateMonsters()
    {
        while (_EnemyDatas.Count > 0)
        {
            if (_EnemyDatas.TryPeek(out ReadyEnemyData data))
            {
                if(data.IsNoticed == false)
                {
                    if(data.NoticeSec <= _PlayTimeSec)
                    {
                        data.IsNoticed = true;

                        _View.CreateEnemyRouteLine(ref data.Data);
                    }
                }
                else if (data.Data.AppearTimeSec <= _PlayTimeSec)
                {
                    _View.CreateMonster(ref data.Data);
                    _EnemyDatas.Dequeue();

                    RemainEnemyCount.Value = _EnemyDatas.Count;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }
    }


    public void CreateDragUnit(ref UnitData data)
    {
        _DragUnitUI.ShowDragUnit(ref data);
        _View.ClearSelectDirection();
    }

    public void HideDragUnit()
    {
        _DragUnitUI.HideDragUnit();
        _View.ClearSelectDirection();
    }

    public void CreateUnit(ref UnitData data, Vector3Int tilePos)
    {
        Time.timeScale = 1;

        _DragUnitUI.DragUnit.gameObject.SetActive(false);
        
        if (CurrentTileDatas[tilePos.x, tilePos.y].MyUnit != null)
            return;

        CurrentTileDatas[tilePos.x, tilePos.y].MyUnit = _View.CreateUnit(ref data, tilePos);

        _View.RecheckEnemyRoute(tilePos);
        _View.RecheckEnemyAttackTarget();
    }

    
    public Vector3Int GetTilemapPos(Vector3 worldPosition)
    {
        Vector3Int pos = _View.Tilemap.WorldToCell(worldPosition);

        if (pos.x < 0) return BattleConfig.TrashValueVector3Int;
        if (pos.x > 15) return BattleConfig.TrashValueVector3Int;

        if (pos.y < 0) return BattleConfig.TrashValueVector3Int;
        if (pos.y > 9) return BattleConfig.TrashValueVector3Int;

        return pos;
    }



    private void GameSuccess()
    {
        EditorUtility.DisplayDialog("성공!!", "게임 클리어!!", "확인");

        AllDisposalDispose();
    }

    private void GameFailure()
    {
        EditorUtility.DisplayDialog("실패!!", "게임 오버!!", "확인");

        AllDisposalDispose();
    }

    private void AllDisposalDispose()
    {
        _LifeDisposal?.Dispose();
        _LifeDisposal = null;

        _UpdateDisposal?.Dispose();
        _UpdateDisposal = null;
    }




    public Vector3Int GetRespawnPosition(int respawnIdx)
    {
        Vector3Int tilePos = BattleConfig.TrashValueVector3Int;

        foreach (var tile in MapData.RespawnTileDatas)
        {
            if (tile.Idx == respawnIdx)
            {
                tilePos.x = tile.Pos.x;
                tilePos.y = tile.Pos.y;
                break;
            }
        }

        return tilePos;
    }

    public Vector3Int GetGoalPosition(int goalIdx)
    {
        Vector3Int tilePos = BattleConfig.TrashValueVector3Int;

        foreach (var tile in MapData.GoalTileDatas)
        {
            if (tile.Idx == goalIdx)
            {
                tilePos.x = tile.Pos.x;
                tilePos.y = tile.Pos.y;
                break;
            }
        }

        return tilePos;
    }

}


public class CurrentTileData
{
    public BATTLE_TILE Type;
    public int Idx;

    public UNIT_TYPE UnitType = UNIT_TYPE.NONE;
    public MyUnit MyUnit;
}

public class ReadyEnemyData
{
    public bool IsNoticed;
    public double NoticeSec;
    public MonsterData Data;
}
