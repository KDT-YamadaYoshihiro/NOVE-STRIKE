using UnityEngine;

public class EnemyStatus : CharaStatusModel
{
    public float AttackPower { get; private set; }
    public float AttackRange { get; private set; }
    public float AttackCooldown {  get; private set; }

    public float CurrentCooldownTimer { get; private set; }

    public EnemyStatus(float arg_maxHealth, float arg_baseMoveSpeed, float arg_attackPower, float arg_attackRange, float arg_cooldown)
        :base(arg_maxHealth,arg_baseMoveSpeed)
    {
        AttackPower = arg_attackPower;
        AttackRange = arg_attackRange;
        AttackCooldown = arg_cooldown;
        CurrentCooldownTimer = 0f;
    }

    public void TickCooldown(float arg_deltaTime)
    {
        if(CurrentCooldownTimer > 0f)
        {
            CurrentCooldownTimer -= arg_deltaTime;
        }
    }

    public void ResetCooldown()
    {
        CurrentCooldownTimer = AttackCooldown;
    }

    public bool CanAttack => CurrentCooldownTimer <= 0f;
}
