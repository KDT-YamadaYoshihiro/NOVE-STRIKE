using UnityEngine;

public class EnemyStatus : CharaStatusModel
{
    public float AttackPower { get; private set; }

    public EnemyStatus(float arg_maxHealth, float arg_baseMoveSpeed, float arg_attackPower)
        :base(arg_maxHealth,arg_baseMoveSpeed)
    {
        AttackPower = arg_attackPower;
    }
}
