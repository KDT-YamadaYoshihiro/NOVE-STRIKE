using UnityEngine;

public class EnemyAIPresenter
{
    private readonly CharaBase m_selfController;
    private readonly EnemyStatus m_status;

    private readonly IEnemyMoveBehavior m_moveBehavior;
    private readonly IEnemyAttackBehavior m_attackBehavior;

    private Transform m_targetTransform;

    // ターゲットと接触しているか判定
    private bool m_isTouchingTarget = false;

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

    /// <summary>
    /// ターゲットの設定
    /// </summary>
    /// <param name="arg_target"></param>
    public void SetTarget(Transform arg_target)
    {
        m_targetTransform = arg_target;
    }

    /// <summary>
    /// 指定したTransformが現在のターゲットと一致するかを判定する。
    /// EnemyControllerの衝突コールバックが、衝突相手がターゲット（プレイヤー）かどうかを
    /// 調べるために使用する。
    /// </summary>
    public bool IsTarget(Transform arg_transform)
    {
        return m_targetTransform != null && arg_transform == m_targetTransform;
    }

    /// <summary>
    /// ターゲットと物理的に接触している状態を外部（EnemyController）から設定する
    /// </summary>
    public void SetTargetContact(bool arg_isTouching)
    {
        m_isTouchingTarget = arg_isTouching;
    }

    /// <summary>
    /// 攻撃判定
    /// </summary>
    /// <param name="arg_deltaTime"></param>
    public void TickUpdate(float arg_deltaTime)
    {
        if (m_status == null || m_targetTransform == null) return;

        // ターゲットと物理的に接触している間は、それ以上押し込まない。
        // AttackRange（攻撃を仕掛ける距離）で止めるのではなく、実際の接触で止めることで
        // 押し出し→再突入の繰り返し（瞬間移動・円形に滑る動き）を防ぐ。
        if (m_isTouchingTarget)
        {
            m_selfController.ExecuteMove(Vector3.zero);

            // 接触中も向きだけはターゲットに合わせ続ける
            Vector3 lookDirection = m_targetTransform.position - m_selfController.transform.position;
            lookDirection.y = 0f;

            if (lookDirection.sqrMagnitude > 0.001f)
            {
                m_selfController.ExecuteRotation(Quaternion.LookRotation(lookDirection.normalized));
            }

            return;
        }

        m_moveBehavior?.ExecuteMove(m_selfController, m_targetTransform, m_status.BaseMoveSpeed);
    }

    /// <summary>
    /// 
    /// </summary>
    public void TickFixedUpdate()
    {
        if (m_status == null || m_targetTransform == null) return;

        m_moveBehavior?.ExecuteMove(m_selfController, m_targetTransform, m_status.BaseMoveSpeed);
    }
}