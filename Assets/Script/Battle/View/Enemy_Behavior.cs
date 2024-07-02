using UnityEngine;

public class Enemy_Behavior
{
    public Enemy_Behavior_Moving Moving { get; private set; }
    public Enemy_Behavior_Attack Attack { get; private set; }

    public Vector2Int MoveDirection {  get; private set; }

    public Enemy Enemy {  get; private set; }

    public Vector3Int CurrTilePos;

    public bool Initialize(Enemy enemy)
    {
        Enemy = enemy;

        Moving = new Enemy_Behavior_Moving();

        if(Enemy.TableData.attack_type == ATTACK_TYPE.ONE)
        {
            if (Enemy.TableData.move_type == MOVE_TYPE.LAND)
            {
                Attack = new Enemy_Behavior_Attack_One();
            }
            else
            {
                Attack = new Enemy_Behavior_Attack_One_Flying();
            }
        }
        else
        {
            Attack = new Enemy_Behavior_Attack_Range();
        }

        return Moving.Initialize(this) && Attack.Initialize(this);
    }


    public void SetBehavior()
    {
        MoveDirection = Moving.GetMoveDirection();

        Attack.CheckAttackTarget();

        if (Attack.GetTarget() != null)
        {
            Attack.Start();
        }
        else
        {
            Moving.Start();
        }
    }

    public void AllBehaviorStop()
    {
        Moving.Stop();
        Attack.Stop();
    }
}
