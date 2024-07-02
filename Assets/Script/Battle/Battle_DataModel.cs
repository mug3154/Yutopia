using System;
using System.Collections.Generic;
using UnityEngine;

public static class BattleConfig
{
    public static float CELL_W = 0.8f;
    public static float CELL_HW = 0.9f;

    public static float CELL_H = 0.8f;
    public static float CELL_HH = 0.9f;

    public static float TILEMAP_X = 0;
    public static float TILEMAP_Y = 0;

    public static Vector2 TrashValueVec2 = new Vector2(-11111, -11111);
    public static Vector3Int TrashValueVector3Int = new Vector3Int(-11111, -11111);


    public static Vector2 GetTilePosition(Vector3Int tilePos)
    {
        return new Vector2()
        {
            x = (tilePos.x * BattleConfig.CELL_W) + BattleConfig.CELL_HW + BattleConfig.TILEMAP_X,
            y = (tilePos.y * BattleConfig.CELL_H) + BattleConfig.CELL_HH + BattleConfig.TILEMAP_Y
        };
    }

    public static List<Vector3Int> GetEnemyRouteTilePos(ref MonsterData data, Vector3Int startPos, ref CurrentTileData[,] tileDatas, MOVE_TYPE moveType)
    {
        //반환할 값
        List<Vector3Int> totalPathList = new List<Vector3Int>();

        //적의 리스폰 지점과 중간 경로, 목표 지점의 x,y 구하기
        Vector3Int[] enemyRoutePoses = new Vector3Int[data.Routes.Length + 2];

        Vector3Int tilePos;

        enemyRoutePoses[0] = startPos;

        int size = data.Routes.Length;
        for (int i = 1; i <= size; ++i)
        {
            tilePos = data.Routes[i - 1];

            if (BattleConfig.IsPassableTile(tileDatas[tilePos.x, tilePos.y], moveType) == false)
                continue;

            enemyRoutePoses[i] = data.Routes[i - 1];
        }

        tilePos = BattleManager.Instance.GetGoalPosition(data.GoalIdx);
        if (tilePos == BattleConfig.TrashValueVector3Int)
        {
            return totalPathList;
        }
        enemyRoutePoses[^1] = tilePos;


        //경로 찾기
        totalPathList.Add(enemyRoutePoses[0]); //밑에서 경로 찾기 시 시작 지점 이후부터 끝 지점까지만 저장하도록 처리

        List<Vector3Int> tempPathList = new List<Vector3Int>();

        size = enemyRoutePoses.Length - 1;
        for (int i = 0; i < size; ++i)
        {
            tempPathList.Clear();

            BattleConfig.FindRouteTilePos(ref tempPathList, ref BattleManager.Instance.CurrentTileDatas, enemyRoutePoses[i], enemyRoutePoses[i + 1], moveType);

            if (tempPathList.Count > 0)
            {
                totalPathList.AddRange(tempPathList.GetRange(1, tempPathList.Count - 1));
            }
        }

        return totalPathList;
    }

    public static bool IsPassableTile(CurrentTileData tile, MOVE_TYPE moveType)
    {
        BATTLE_TILE tileType = tile.Type;

        if (tileType == BATTLE_TILE.DECORATION || tileType == BATTLE_TILE.SINKHOLE)
            return false;

        if (tile.MyUnit == null)
            return true;

        UNIT_TYPE unitType = tile.MyUnit.Data.Type;

        if(unitType == UNIT_TYPE.OBSTACLES)
            if(moveType == MOVE_TYPE.LAND)
                return false;

        return true;
    }


    public static void FindRouteTilePos(ref List<Vector3Int> results, ref CurrentTileData[,] tileDatas, Vector3Int startPos, Vector3Int targetPos, MOVE_TYPE moveType)
    {
        int[] deltaX = new int[] { 0, -1, 0, 1/*, 1, -1, -1, 1 */};
        int[] deltaY = new int[] { -1, 0, 1, 0/*, -1, 1, -1, 1 */};
        int[] cost = new int[] { 10, 10, 10, 10/*, 14, 14, 14, 14 */};

        // 점수 매기기
        // F = G _ H
        // F = 최종 점수 (작을 수록 좋음. 경로에 따라 달라짐)
        // G = 시작점에서 해당 좌표까지 이동하는데 드는 비용 (작을 수록 좋음. 경로에 따라 달라짐)
        // H = 목적지로부터 얼마나 가까운 곳인지를 따지는 보너스 점수 (작을 수록 좋음. 고정인 값)

        int MAP_W = tileDatas.GetLength(0);
        int MAP_H = tileDatas.GetLength(1);


        // (y, x) 이미 방문 했는지 여부 (방문 = closed 상태)
        bool[,] findPathClosed = new bool[MAP_W, MAP_H];

        // 출발지에서 (y, x) 가는데에 현재까지 업뎃된 최단거리
        // (y, x) 가는 길을 한번이라도 발견 했었는지 여부가 될 수도 있다. (한번이라도 예약 되있는지 여부)
        // 발견(예약)이 안됐다면 Int32.MaxValue 로 저장이 되어 있을 것.
        // F = G + H 값이 저장된다. (이 F 값이 가장 작은 정점이 방문 정점으로 선택될 것)
        int[,] findPathOpen = new int[MAP_W, MAP_H];

        Pos[,] findPathParent = new Pos[MAP_W, MAP_H];

        for (int w = 0; w < MAP_W; ++w)
        {
            for (int h = 0; h < MAP_H; ++h)
            {
                findPathClosed[w, h] = false;
                findPathOpen[w, h] = Int32.MaxValue;
            }
        }

        int y = 0;
        int x = 0;


        // 우선순위 큐 : 예약된 것들 중 가장 좋은 후보를 빠르게 뽑아오기 위한 도구. 
        PriorityQueue<PQNode> pq = new PriorityQueue<PQNode>(PRIORITY_SORT_TYPE.DESCENDING);

        // 시작점 발견 (시작점 예약 진행)
        findPathOpen[startPos.x, startPos.y] = 10 * (Math.Abs(targetPos.y - startPos.y) + Math.Abs(targetPos.x - startPos.x));  // 시작점이니까 G는 0이고, H 값임.

        pq.Add(new PQNode()
        {
            F = 10 * (Math.Abs(targetPos.x - startPos.x) + Math.Abs(targetPos.y - startPos.y)),
            G = 0,
            X = startPos.x,
            Y = startPos.y
        });

        Pos pos = new Pos();
        pos.Set(startPos.x, startPos.y);
        findPathParent[startPos.x, startPos.y] = pos;

        int deltaXLenght = deltaX.Length;

        while (pq.Count > 0)
        {
            // 제일 좋은 후보를 찾는다.
            PQNode node = pq.Pop();

            // 동일한 좌표를 여러 경로로 찾아서, 더 빠른 경로로 인해서 이미 방문(closed) 된 경우 스킵
            if (findPathClosed[node.X, node.Y])
                continue;

            // 방문 한다.
            findPathClosed[node.X, node.Y] = true;

            // 목적지에 도착했으면 바로 종료
            if (node.Y == targetPos.y && node.X == targetPos.x)
                break;

            // 상하좌우 등 이동할 수 있는 좌표인지 확인해서 예약(open)한다.
            for (int i = 0; i < deltaXLenght; i++)
            {
                int nextX = node.X + deltaX[i];
                int nextY = node.Y + deltaY[i];

                // 유효 범위를 벗어났으면 스킵
                if (nextY < 0 || nextY >= MAP_H || nextX < 0 || nextX >= MAP_W)
                    continue;
                // 이미 방문한 곳이면 스킵
                if (findPathClosed[nextX, nextY])
                    continue;

                // 이동할 수 없는 지형이면 스킵
                if (IsPassableTile(tileDatas[nextX, nextY], moveType) == false)
                    continue;

                // 비용 계산
                int g = node.G + cost[i];
                int h = 10 * (Math.Abs(targetPos.x - nextX) + Math.Abs(targetPos.y - nextY));

                // 다른 경로에서 더 빠른 길을 이미 찾았으면 스킵 (업뎃 할 필요가 없으니까)
                if (findPathOpen[nextX, nextY] < g + h)
                    continue;

                // 예약 진행
                findPathOpen[nextX, nextY] = g + h;

                pq.Add(new PQNode()
                {
                    F = g + h,
                    G = g,
                    X = nextX,
                    Y = nextY
                });

                pos = new Pos();
                pos.Set(node.X, node.Y);
                findPathParent[nextX, nextY] = pos;
            }
        }


        results.Clear();

        if (findPathParent[targetPos.x, targetPos.y] != null)
        {
            x = targetPos.x;
            y = targetPos.y;
            while (findPathParent[x, y].Y != y || findPathParent[x, y].X != x)
            {
                results.Add(new Vector3Int(x, y));
                pos = findPathParent[x, y];
                x = pos.X;
                y = pos.Y;
            }
            results.Add(new Vector3Int(x, y));
            results.Reverse();
        }
    }

    public class Pos
    {
        public void Set(int x, int y) { Y = y; X = x; }
        public int Y;
        public int X;
    }

    public struct PQNode : IComparable<PQNode>
    {
        public int F;
        public int G;
        // F랑 G만 알아도 H 는 구할 수 있으니 생략
        public int Y;
        public int X;

        public int CompareTo(PQNode other)
        {
            if (F == other.F)  // F 값을 기준으로 크기를 비교
                return 0;
            return F < other.F ? 1 : -1;
        }

        public int CompareTo(object obj)
        {
            throw new NotImplementedException();
        }
    }
}

[Serializable]
public enum BATTLE_TILE : ushort
{
    NORMAL = 1, //이미지 번호가 1부터니까 바꾸지 말 것! 새로 이미지 세팅할 때 0부터로 만들고 변경할 것!
    GOAL,
    MONSTER_RESPONE,
    HIGHLANDS,
    SINKHOLE,
    DECORATION
}

public enum MOVE_TYPE : ushort
{
    LAND,
    FLYING
}

public enum ATTACK_TYPE : ushort
{
    ONE,
    RANGE
}


public enum UNIT_TYPE : ushort
{
    NONE,
    OBSTACLES,
    DISPOSABLE_OBSTACLES,
}


[Serializable]
public class MapData
{
    public int BattleSec;
    public int LifeCount;

    public int NormalTileResourceIdx;
    public TileData[] TileDatas;
    public GoalTileData[] GoalTileDatas;
    public RespawnTileData[] RespawnTileDatas;
    public MonsterData[] MonsterData;
    public UnitData[] UnitData;

}

[Serializable]
public struct TileData
{
    public BATTLE_TILE Type;
    public int ResourceIdx;
    public Vector3Int Pos;
}

[Serializable]
public struct GoalTileData
{
    public int Idx;
    public int ResourceIdx;
    public Vector3Int Pos;
}

[Serializable]
public struct RespawnTileData
{
    public int Idx;
    public int ResourceIdx;
    public Vector3Int Pos;
}

[Serializable]
public struct MonsterData
{
    public uint Idx;
    public uint MonsterId;
    public float AppearTimeSec; //게임 시작부터의 등장 초.
    public int RespawnIdx; //등장 리스폰 타일 번호
    public int GoalIdx; //목표 골 번호
    public float MoveSpeed; //한칸 당 이동 초
    public Vector3Int[] Routes; //중간 경로
    //public Vector3Int[] AttackRange; //공격 범위. 캐릭터가 오른쪽을 보고 이동한다는 기준.
}

public struct UnitData
{
    public UNIT_TYPE Type;
    public int Idx;
    public string IconName;
    public string ResourceName;
    public int Count;

    public float HP;
}