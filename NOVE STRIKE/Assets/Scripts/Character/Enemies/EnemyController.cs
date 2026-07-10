using UnityEngine;

public class EnemyController : CharaBase
{
    private EnemyAIPresenter m_aiPresenter;
    private EnemyData m_enemyData;

    public EnemyStatus EnemyStatus => Status as EnemyStatus;

    protected override void SetupStatusModel()
    {
        // 直接シーンに置かれた場合のエラー回避用ダミーデータ
        Status = new EnemyStatus(10f, 1f, 1f, 1f, 1f);
    }

    /// <summary>
    /// Factoryから呼び出され、データとAI部品を注入して自身を構築する
    /// </summary>
    public void InitializeEnemy(EnemyData arg_data, IEnemyMoveBehavior arg_moveBehavior, IEnemyAttackBehavior arg_attackBehavior)
    {
        base.InitializeCharacter();
        m_enemyData = arg_data;

        // データテーブルの値でステータスモデルを正確に構築
        Status = new EnemyStatus(
            arg_data.MaxHealth,
            arg_data.MoveSpeed,
            arg_data.AttackPower,
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

    public void TickUpdate(float arg_deltaTime)
    {
        m_aiPresenter?.TickUpdate(arg_deltaTime);
    }

    public void TickFixedUpdate()
    {
        m_aiPresenter?.TickFixedUpdate();
    }

    public void SetTarget(Transform arg_playerTransform)
    {
        m_aiPresenter?.SetTarget(arg_playerTransform);
    }
}