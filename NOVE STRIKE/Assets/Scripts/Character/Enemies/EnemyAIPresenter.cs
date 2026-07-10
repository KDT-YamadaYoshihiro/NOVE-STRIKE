using UnityEngine;

public class EnemyAIPresenter
{
    private readonly CharaBase m_selfController;
    private readonly EnemyStatus m_status;

    private readonly IEnemyMoveBehavior m_moveBehavior;
    private readonly IEnemyAttackBehavior m_attackBehavior;

    private Transform m_targetTransform;

    public EnemyAIPresenter(
        CharaBase arg_selfController,
        EnemyStatus arg_status,
        IEnemyMoveBehavior arg_moveBehavior,
        IEnemyAttackBehavior arg_attackBehavior)
    {
        m_selfController = arg_selfController;
        m_status = arg_status;
        m_moveBehavior = arg_moveBehavior;
        m_attackBehavior = arg_attackBehavior;
    }

    public void SetTarget(Transform arg_target)
    {
        m_targetTransform = arg_target;
    }

    public void TickUpdate(float arg_deltaTime)
    {
        if (m_status == null || m_targetTransform == null) return;

        m_status.TickCooldown(arg_deltaTime);

        float distanceToTarget = Vector3.Distance(m_selfController.transform.position, m_targetTransform.position);

        if (distanceToTarget <= m_status.AttackRange && m_status.CanAttack)
        {
            if (m_attackBehavior != null)
            {
                m_attackBehavior.ExecuteAttack(m_selfController, m_targetTransform, m_status, BulletOwnerType.Enemy);
                m_status.ResetCooldown();
            }
        }
    }

    public void TickFixedUpdate()
    {
        if (m_status == null || m_targetTransform == null) return;

        m_moveBehavior?.ExecuteMove(m_selfController, m_targetTransform, m_status.BaseMoveSpeed);
    }
}