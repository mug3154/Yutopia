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
        //��ȯ�� ��
        List<Vector3Int> totalPathList = new List<Vector3Int>();

        //���� ������ ������ �߰� ���, ��ǥ ������ x,y ���ϱ�
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


        //��� ã��
        totalPathList.Add(enemyRoutePoses[0]); //�ؿ��� ��� ã�� �� ���� ���� ���ĺ��� �� ���������� �����ϵ��� ó��

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

        // ���� �ű��
        // F = G _ H
        // F = ���� ���� (���� ���� ����. ��ο� ���� �޶���)
        // G = ���������� �ش� ��ǥ���� �̵��ϴµ� ��� ��� (���� ���� ����. ��ο� ���� �޶���)
        // H = �������κ��� �󸶳� ����� �������� ������ ���ʽ� ���� (���� ���� ����. ������ ��)

        int MAP_W = tileDatas.GetLength(0);
        int MAP_H = tileDatas.GetLength(1);


        // (y, x) �̹� �湮 �ߴ��� ���� (�湮 = closed ����)
        bool[,] findPathClosed = new bool[MAP_W, MAP_H];

        // ��������� (y, x) ���µ��� ������� ������ �ִܰŸ�
        // (y, x) ���� ���� �ѹ��̶� �߰� �߾����� ���ΰ� �� ���� �ִ�. (�ѹ��̶� ���� ���ִ��� ����)
        // �߰�(����)�� �ȵƴٸ� Int32.MaxValue �� ������ �Ǿ� ���� ��.
        // F = G + H ���� ����ȴ�. (�� F ���� ���� ���� ������ �湮 �������� ���õ� ��)
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


        // �켱���� ť : ����� �͵� �� ���� ���� �ĺ��� ������ �̾ƿ��� ���� ����. 
        PriorityQueue<PQNode> pq = new PriorityQueue<PQNode>(PRIORITY_SORT_TYPE.DESCENDING);

        // ������ �߰� (������ ���� ����)
        findPathOpen[startPos.x, startPos.y] = 10 * (Math.Abs(targetPos.y - startPos.y) + Math.Abs(targetPos.x - startPos.x));  // �������̴ϱ� G�� 0�̰�, H ����.

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
            // ���� ���� �ĺ��� ã�´�.
            PQNode node = pq.Pop();

            // ������ ��ǥ�� ���� ��η� ã�Ƽ�, �� ���� ��η� ���ؼ� �̹� �湮(closed) �� ��� ��ŵ
            if (findPathClosed[node.X, node.Y])
                continue;

            // �湮 �Ѵ�.
            findPathClosed[node.X, node.Y] = true;

            // �������� ���������� �ٷ� ����
            if (node.Y == targetPos.y && node.X == targetPos.x)
                break;

            // �����¿� �� �̵��� �� �ִ� ��ǥ���� Ȯ���ؼ� ����(open)�Ѵ�.
            for (int i = 0; i < deltaXLenght; i++)
            {
                int nextX = node.X + deltaX[i];
                int nextY = node.Y + deltaY[i];

                // ��ȿ ������ ������� ��ŵ
                if (nextY < 0 || nextY >= MAP_H || nextX < 0 || nextX >= MAP_W)
                    continue;
                // �̹� �湮�� ���̸� ��ŵ
                if (findPathClosed[nextX, nextY])
                    continue;

                // �̵��� �� ���� �����̸� ��ŵ
                if (IsPassableTile(tileDatas[nextX, nextY], moveType) == false)
                    continue;

                // ��� ���
                int g = node.G + cost[i];
                int h = 10 * (Math.Abs(targetPos.x - nextX) + Math.Abs(targetPos.y - nextY));

                // �ٸ� ��ο��� �� ���� ���� �̹� ã������ ��ŵ (���� �� �ʿ䰡 �����ϱ�)
                if (findPathOpen[nextX, nextY] < g + h)
                    continue;

                // ���� ����
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
        // F�� G�� �˾Ƶ� H �� ���� �� ������ ����
        public int Y;
        public int X;

        public int CompareTo(PQNode other)
        {
            if (F == other.F)  // F ���� �������� ũ�⸦ ��
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
    NORMAL = 1, //�̹��� ��ȣ�� 1���ʹϱ� �ٲ��� �� ��! ���� �̹��� ������ �� 0���ͷ� ����� ������ ��!
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
    public float AppearTimeSec; //���� ���ۺ����� ���� ��.
    public int RespawnIdx; //���� ������ Ÿ�� ��ȣ
    public int GoalIdx; //��ǥ �� ��ȣ
    public float MoveSpeed; //��ĭ �� �̵� ��
    public Vector3Int[] Routes; //�߰� ���
    //public Vector3Int[] AttackRange; //���� ����. ĳ���Ͱ� �������� ���� �̵��Ѵٴ� ����.
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