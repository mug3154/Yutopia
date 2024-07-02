using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using UnityEngine.Tilemaps;

public class BattleView : MonoBehaviour
{
    [SerializeField] Tilemap _Tilemap;
    public Tilemap Tilemap=> _Tilemap;
    
    [SerializeField] Tilemap _DirectionTilemap;
    [SerializeField] TileBase _DirectionTile;

    [SerializeField] Transform _UnitContainer;

   
    [SerializeField] MyUnit _MyUnitPrefab;


    Dictionary<string, TileBase> _TileBaseDic;

    ObjectPool<Enemy> _EnemyPool;
    List<Enemy> _Enemys;

    ObjectPool<TrailRenderer> _EnemyRouteLinePool;
    List<TrailRenderer> _EnemyRouteLines;

    ObjectPool<MyUnit> _MyUnitPool;
    List<MyUnit> _MyUnits;

    public Dictionary<string, ObjectPool<GameObject>> UnitObjPools { private set; get; }

    public void LoadingResources(MapData mapData, Action<float> progressCallback)
    {
        if (_EnemyPool == null)
        {
            float cnt = 0;

            float total = 3; //Enemy, EnemyRouteTrail, MyUnit
            foreach (var unitData in mapData.UnitData)
            {
                ++total;
            }

            //========== Enemy

            var handler = Addressables.LoadAssetAsync<GameObject>("Enemy").WaitForCompletion();
            _EnemyPool ??= new ObjectPool<Enemy>(() => Instantiate(handler.GetComponent<Enemy>(), _UnitContainer));
            Addressables.Release(handler);

            ++cnt;
            progressCallback?.Invoke(cnt / total);

            //========== EnemyRouteTrail

            var handler2 = Addressables.LoadAssetAsync<GameObject>("EnemyRouteTrail").WaitForCompletion();
            _EnemyRouteLinePool ??= new ObjectPool<TrailRenderer>(() => Instantiate(handler2.GetComponent<TrailRenderer>(), _UnitContainer));
            Addressables.Release(handler2);

            ++cnt;
            progressCallback?.Invoke(cnt / total);

            //========== MyUnit

            var handler3 = Addressables.LoadAssetAsync<GameObject>("MyUnit").WaitForCompletion();
            _MyUnitPool ??= new ObjectPool<MyUnit>(() => Instantiate(handler3.GetComponent<MyUnit>(), _UnitContainer));
            Addressables.Release(handler3);

            ++cnt;
            progressCallback?.Invoke(cnt / total);
            
            //========== MyUnit Resource

            UnitObjPools = new Dictionary<string, ObjectPool<GameObject>>();
            foreach (var unitData in mapData.UnitData)
            {
                var unitHandler = Addressables.LoadAssetAsync<GameObject>(unitData.ResourceName).WaitForCompletion();

                var pool = new ObjectPool<GameObject>(() => Instantiate(unitHandler));
                UnitObjPools.Add(unitData.ResourceName, pool);

                Addressables.Release(unitHandler);

                ++cnt;
                progressCallback?.Invoke(cnt / total);
            }

        }
    }
      
    public void Initialize(MapData data)
    {
        BattleConfig.TILEMAP_X = _Tilemap.transform.position.x;
        BattleConfig.TILEMAP_Y = _Tilemap.transform.position.y;

        BattleConfig.CELL_W = _Tilemap.cellSize.x;
        BattleConfig.CELL_HW = BattleConfig.CELL_W * 0.5f;
        BattleConfig.CELL_H = _Tilemap.cellSize.y;
        BattleConfig.CELL_HH = BattleConfig.CELL_H * 0.5f;

        
        _Enemys ??= new List<Enemy>();
        _Enemys.Clear();

        _EnemyRouteLines ??= new List<TrailRenderer>();
        _EnemyRouteLines.Clear();

        _MyUnits ??= new List<MyUnit>();
        _MyUnits.Clear();


        LoadTileBases(data);
        DrawMap(data);

        _DirectionTilemap.gameObject.SetActive(false);
    }

    private void LoadTileBases(MapData data)
    {
        List<string> tileNames = new List<string>();
        tileNames.Add($"tile_1_{data.NormalTileResourceIdx}");

        StringBuilder sb = new StringBuilder();

        foreach (var tileData in data.TileDatas)
        {
            sb.Clear();

            sb.Append($"tile_{(int)tileData.Type}_{tileData.ResourceIdx}");

            if (tileNames.Contains(sb.ToString()) == false)
            {
                tileNames.Add(sb.ToString());
            }
        }

        foreach (var tileData in data.GoalTileDatas)
        {
            sb.Clear();

            sb.Append($"tile_{(int)BATTLE_TILE.GOAL}_{tileData.ResourceIdx}");

            if (tileNames.Contains(sb.ToString()) == false)
            {
                tileNames.Add(sb.ToString());
            }
        }

        foreach (var tileData in data.RespawnTileDatas)
        {
            sb.Clear();

            sb.Append($"tile_{(int)BATTLE_TILE.MONSTER_RESPONE}_{tileData.ResourceIdx}");

            if (tileNames.Contains(sb.ToString()) == false)
            {
                tileNames.Add(sb.ToString());
            }
        }

        _TileBaseDic = new Dictionary<string, TileBase>();

        for (int i = 0; i < tileNames.Count; ++i)
        {
            var handler = Addressables.LoadAssetAsync<TileBase>($"Assets/Arts/Tilemap/{tileNames[i]}.asset").WaitForCompletion();
            if (handler != null)
            {
                _TileBaseDic.Add(tileNames[i], handler);
            }
            Addressables.Release(handler);
        }
    }

    public void DrawMap(MapData data)
    {
        TileBase tileBase = _TileBaseDic[$"tile_1_{data.NormalTileResourceIdx}"];
        for (int i = 0; i < 16; ++i)
        {
            for (int j = 0; j < 10; ++j)
            {
                _Tilemap.SetTile(new Vector3Int(i, j), tileBase);
            }
        }

        foreach (var tileData in data.TileDatas)
        {
            _Tilemap.SetTile(tileData.Pos, _TileBaseDic[$"tile_{(int)tileData.Type}_{tileData.ResourceIdx}"]);
        }

        foreach (var tileData in data.GoalTileDatas)
        {
            _Tilemap.SetTile(tileData.Pos, _TileBaseDic[$"tile_{(int)BATTLE_TILE.GOAL}_{tileData.ResourceIdx}"]);
        }

        foreach (var tileData in data.RespawnTileDatas)
        {
            _Tilemap.SetTile(tileData.Pos, _TileBaseDic[$"tile_{(int)BATTLE_TILE.MONSTER_RESPONE}_{tileData.ResourceIdx}"]);
        }
    }

    public void DrawSelectDirection(Vector3Int startTilePos, Vector3Int direction, Vector3Int[] area)
    {
        ClearSelectDirection();
        
        foreach(var a in area)
        {
            _DirectionTilemap.SetTile(startTilePos + (a * direction), _DirectionTile);
        }
        
        _DirectionTilemap.gameObject.SetActive(true);
    }

    public void ClearSelectDirection()
    {
        _DirectionTilemap.gameObject.SetActive(false);
        _DirectionTilemap.ClearAllTiles();
    }

    public void CreateEnemyRouteLine(ref MonsterData data)
    {
        TrailRenderer trailRenderer = _EnemyRouteLinePool.Get();

        Vector3Int startTilePos = BattleManager.Instance.GetRespawnPosition(data.RespawnIdx);
        if (startTilePos == BattleConfig.TrashValueVector3Int)
            return;

        Table_Monster.Table_MonsterData monsterTableData = Table_Monster.Instance.GetData(data.MonsterId);
        if (monsterTableData.id == 0) 
            return;

        List<Vector3Int> routePoses = BattleConfig.GetEnemyRouteTilePos(ref data, startTilePos, ref BattleManager.Instance.CurrentTileDatas, monsterTableData.move_type);
        if (routePoses.Count == 0)
            return;

        List<Vector3> tilePathList = new List<Vector3>();
        foreach(var pos in routePoses)
        {
            tilePathList.Add(BattleConfig.GetTilePosition(pos));
        }

        trailRenderer.transform.position = tilePathList[0];

        trailRenderer.gameObject.SetActive(true);

        _EnemyRouteLines.Add(trailRenderer);

        trailRenderer.transform.DOPath(tilePathList.ToArray(), tilePathList.Count * 0.1f).OnComplete(() =>
        {
            trailRenderer.gameObject.SetActive(false);
            _EnemyRouteLines.Remove(trailRenderer);
            _EnemyRouteLinePool.Release(trailRenderer);
        });

    }

    public void CreateMonster(ref MonsterData data)
    {
        Enemy monster = _EnemyPool.Get();
        _Enemys.Add(monster);

        monster.Create(data, DisposeMonster);
    }

    private void DisposeMonster(Enemy monster)
    {
        monster.gameObject.SetActive(false);
        _EnemyPool.Release(monster);
        _Enemys.Remove(monster);
    }

    public void RecheckEnemyRoute(Vector3Int changedPos)
    { 
        foreach(var m in _Enemys)
        {
            m.Behavior.Moving.RecheckRoute(changedPos);
        }
    }
    public void RecheckEnemyAttackTarget()
    {
        foreach (var m in _Enemys)
        {
            m.Behavior.Attack.CheckAttackTarget();
        }
    }

    public MyUnit CreateUnit(ref UnitData data, Vector3Int tilePos)
    {
        if (UnitObjPools.ContainsKey(data.ResourceName) == false)
            return null;

        var myUnit = _MyUnitPool.Get();
        myUnit.SetData(data, UnitObjPools[data.ResourceName].Get(), tilePos);
        myUnit.DisposeCallback = DisposeUnit;
        myUnit.ObjDisposeCallback = DisposeUnitObj;
        myUnit.transform.localPosition = _Tilemap.GetCellCenterWorld(tilePos);

        myUnit.gameObject.SetActive(true);

        _MyUnits.Add(myUnit);

        return myUnit;
    }

    public void DisposeUnit(MyUnit unit, Vector3Int tilePos)
    {
        BattleManager.Instance.CurrentTileDatas[tilePos.x, tilePos.y].MyUnit = null;

        _MyUnitPool.Release(unit);
        unit.gameObject.SetActive(false);
        _MyUnits.Remove(unit);
    }

    public void DisposeUnitObj(string unitObjName, GameObject unitObj)
    {
        UnitObjPools[unitObjName].Release(unitObj);
        unitObj.transform.SetParent(transform);
        unitObj.gameObject.SetActive(false);
    }
}
