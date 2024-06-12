using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleUIBottom : MonoBehaviour
{
    [SerializeField] SummonUnitCell _Obstacle;
    [SerializeField] SummonUnitCell _DisposableObstacle;

    private void Start()
    {
        _Obstacle.SetData(new UnitData()
        { 
            Idx = 1,
            Type = UNIT_TYPE.OBSTACLES,
            IconName = "Unit_Icon_Obstacle",
            ResourceName = "Unit_obstacle",
            Count = 10,
        });

        _DisposableObstacle.SetData(new UnitData()
        {
            Idx = 2,
            Type = UNIT_TYPE.OBSTACLES,
            IconName = "Unit_Icon_Disposable_obstacle",
            ResourceName = "Unit_disposal_obstacle",
            Count = 10,
            HP = 10
        });
    }

}
