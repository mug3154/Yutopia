using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Table_Monster
{
    public struct Table_MonsterData
    {
        public int id;
        public string name;
        public string desc;
        public string resource_name;
        public MOVE_TYPE move_type;
        public ATTACK_TYPE attack_type;
        public List<Vector3Int> attack_range;
        public float attack_delay;
        public float damage;
    }
    
    public static Table_Monster _Instance;
    public static Table_Monster Instance => _Instance ??= new Table_Monster();

    public Dictionary<uint, Table_MonsterData> Data { get; private set; }


    Table_Monster()
    {
        Init();
    }

    public void Init()
    {
        if (Data != null) return;

        Data = new Dictionary<uint, Table_MonsterData>();
        Data.Add(1, new Table_MonsterData()
        {
            id = 1,
            name = "1",
            desc = "1",
            resource_name = "Enemy_1",
            move_type = MOVE_TYPE.LAND,
            attack_type = ATTACK_TYPE.ONE,
            attack_range = new List<Vector3Int>()
        {
            new Vector3Int(1, 0, 0)
        },
            attack_delay = 0.5f,
            damage = 1
        });

        Data.Add(2, new Table_MonsterData()
        {
            id = 2,
            name = "2",
            desc = "2",
            resource_name = "Enemy_2",
            move_type = MOVE_TYPE.LAND,
            attack_type = ATTACK_TYPE.RANGE,
            attack_range = new List<Vector3Int>()
        {
            new Vector3Int(1, -1, 0),
            new Vector3Int(1, 0, 0),
            new Vector3Int(1, 1, 0),
            new Vector3Int(0, -1, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(-1, -1, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(-1, 1, 0),
        },
            attack_delay = 0.5f,
            damage = 1
        });


        Data.Add(3, new Table_MonsterData()
        {
            id = 3,
            name = "3",
            desc = "3",
            resource_name = "Enemy_3",
            move_type = MOVE_TYPE.FLYING,
            attack_type = ATTACK_TYPE.ONE,
            attack_range = new List<Vector3Int>()
        {
            new Vector3Int(1, 0, 0),
            new Vector3Int(2, 0, 0),
            new Vector3Int(3, 0, 0),
        },
            attack_delay = 0.5f,
            damage = 1
        });
    }

    public Table_MonsterData GetData(uint id)
    {
        return Data[id];
    }

}
