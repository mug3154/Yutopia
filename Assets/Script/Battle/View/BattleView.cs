using DG.Tweening;
using System.Collections;
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
    [SerializeField] Transform _UnitContainer;

    [SerializeField] Enemy _EnemyPrefab;
    [SerializeField] TrailRenderer _EnemyRouteLinePrefab;
   
    [SerializeField] MyUnit _MyUnitPrefab;


    Dictionary<string, TileBase> _TileBaseDic;

    ObjectPool<Enemy> _EnemyPool;
    List<Enemy> _Enemys;

    ObjectPool<TrailRenderer> _EnemyRouteLinePool;
    List<TrailRenderer> _EnemyRouteLines;


    ObjectPool<MyUnit> _MyUnitPool;
    List<MyUnit> _MyUnits;

    public void Setting(MapData data)
    {
        BattleConfig.TILEMAP_X = _Tilemap.transform.position.x;
        BattleConfig.TILEMAP_Y = _Tilemap.transform.position.y;

        BattleConfig.CELL_W = _Tilemap.cellSize.x;
        BattleConfig.CELL_HW = BattleConfig.CELL_W * 0.5f;
        BattleConfig.CELL_H = _Tilemap.cellSize.y;
        BattleConfig.CELL_HH = BattleConfig.CELL_H * 0.5f;


        _EnemyPool ??= new ObjectPool<Enemy>(() => Instantiate(_EnemyPrefab, _UnitContainer));
        _Enemys ??= new List<Enemy>();
        _Enemys.Clear();

        _EnemyRouteLinePool ??= new ObjectPool<TrailRenderer>(() => Instantiate(_EnemyRouteLinePrefab, _UnitContainer));
        _EnemyRouteLines ??= new List<TrailRenderer>();
        _EnemyRouteLines.Clear();

        _MyUnitPool ??= new ObjectPool<MyUnit>(() => Instantiate(_MyUnitPrefab, _UnitContainer));
        _MyUnits ??= new List<MyUnit>();
        _MyUnits.Clear();


        LoadTileBases(data);
        DrawMap(data);
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

    //몬스터 경로 받아오기
    

    public void CreateEnemyRouteLine(ref MonsterData data)
    {
        TrailRenderer trailRenderer = _EnemyRouteLinePool.Get();

        List<Vector3Int> routePoses = BattleConfig.GetEnemyRouteTilePos(ref data, ref BattleManager.Instance.CurrentTileDatas);
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


    public void CreateUnit(ref UnitData data, Vector3Int tilePos)
    {
        var myUnit = _MyUnitPool.Get();
        myUnit.SetData(data);

        myUnit.transform.localPosition = _Tilemap.GetCellCenterWorld(tilePos);

        myUnit.gameObject.SetActive(true);

        _MyUnits.Add(myUnit);
    }

}
