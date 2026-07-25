using UnityEngine;

public class EnemyController : CharaBase
{
    private EnemyAIPresenter m_aiPresenter;
    private EnemyData m_enemyData;

    public EnemyStatus EnemyStatus => Status as EnemyStatus;

    /// <summary>
    /// 初期設定
    /// </summary>
    protected override void SetupStatusModel()
    {
        Status = new EnemyStatus(10f, 1f, 1f, 0f, 1f, 1f);
    }

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="arg_data"></param>
    /// <param name="arg_moveBehavior"></param>
    /// <param name="arg_attackBehavior"></param>
    public void InitializeEnemy(EnemyData arg_data, IEnemyMoveBehavior arg_moveBehavior, IEnemyAttackBehavior arg_attackBehavior)
    {
        base.InitializeCharacter();
        m_enemyData = arg_data;

        // データテーブルの値でステータスモデルを正確に構築
        Status = new EnemyStatus(
            arg_data.MaxHealth,
            arg_data.MoveSpeed,
            arg_data.AttackPower,
            arg_data.DefensePower,
            arg_data.AttackRange,
            arg_data.AttackCooldown
        );

        m_aiPresenter = new EnemyAIPresenter(
            this,
            EnemyStatus,
            arg_moveBehavior,
            arg_attackBehavior
        );
    }

    /// <summary>
    /// 更新処理
    /// </summary>
    /// <param name="arg_deltaTime"></param>
    public void TickUpdate(float arg_deltaTime)
    {
        m_aiPresenter?.TickUpdate(arg_deltaTime);
    }

    /// <summary>
    /// 計算系更新
    /// </summary>
    public void TickFixedUpdate()
    {
        m_aiPresenter?.TickFixedUpdate();
    }

    /// <summary>
    /// ターゲット設定
    /// </summary>
    /// <param name="arg_playerTransform"></param>
    public void SetTarget(Transform arg_playerTransform)
    {
        m_aiPresenter?.SetTarget(arg_playerTransform);
    }

    /// <summary>
    /// ターゲットとの物理的接触通知
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        if (m_aiPresenter != null && m_aiPresenter.IsTarget(collision.transform))
        {
            m_aiPresenter.SetTargetContact(true);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionExit(Collision collision)
    {
        if (m_aiPresenter != null && m_aiPresenter.IsTarget(collision.transform))
        {
            m_aiPresenter.SetTargetContact(false);
        }
    }
}