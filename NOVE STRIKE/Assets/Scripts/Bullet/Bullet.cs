using UnityEngine;

public class Bullet
{
    private readonly GameObject m_bulletObject;
    private readonly BulletMover m_bulletMover;
    private BulletData m_bulletData;

    private float m_currentLifeTime;

    public bool IsDead { get; private set; }
    public GameObject GameObject => m_bulletObject; // プールアクセス用

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="arg_data"></param>
    /// <param name="arg_direction"></param>
    public Bullet(GameObject arg_gameObject)
    {
        m_bulletObject = arg_gameObject;
        m_bulletMover = m_bulletObject.GetComponent<BulletMover>();
        if (m_bulletMover != null)
        {
            // インスタンス化時に1度だけイベントを購読（プール中は解除不要で安全です）
            m_bulletMover.OnTriggerEntered += HandleTriggerEnter;
        }
    }

    /// <summary>
    /// セットアップ処理
    /// </summary>
    /// <param name="arg_data"></param>
    public void Setup(BulletData arg_data)
    {
        m_bulletData = arg_data;
        m_currentLifeTime = 0f;
        IsDead = false;

        if (m_bulletMover != null && m_bulletData != null)
        {
            m_bulletMover.InitializeMover(m_bulletData.Speed, m_bulletData.Direction);
        }
    }

    /// <summary>
    /// 更新処理
    /// </summary>
    /// <param name="arg_deltaTime"></param>
    public void TickUpdate(float arg_deltaTime)
    {
        if (IsDead || m_bulletData == null) { return; }

        m_currentLifeTime += arg_deltaTime;
        if (m_currentLifeTime >= m_bulletData.LifeTime)
        {
            KillBullet();
        }
    }

    /// <summary>
    /// 物理演算の更新処理
    /// </summary>
    public void TickFixedUpdate()
    {
        if (IsDead || m_bulletData == null) { return; }

        if (m_bulletMover != null)
        {
            m_bulletMover.TickFixedUpdate();
        }
    }

    /// <summary>
    /// Moverから通知された衝突判定を処理する（汎用ロジック）
    /// </summary>
    private void HandleTriggerEnter(Collider arg_other)
    {
        if (IsDead || m_bulletData == null) { return; }

        // 発射元に応じた味方への誤射防止判定
        if (m_bulletData.OwnerType == BulletOwnerType.Player)
        {
            // プレイヤーが撃った弾は、プレイヤー（自分・味方）には当たらない
            if (arg_other.GetComponent<PlayerController>() != null) return;
        }
        else if (m_bulletData.OwnerType == BulletOwnerType.Enemy)
        {
            // エネミーが撃った弾は、エネミー（味方）には当たらない
            if (arg_other.GetComponent<EnemyController>() != null) return;
        }

        // ダメージ処理の通知
        IDamageable target = arg_other.GetComponent<IDamageable>();
        if (target != null)
        {
            // キャラクターから付与された攻撃力をそのままターゲットに与える
            target.TakeDamage(m_bulletData.Damage);
            KillBullet();
        }
    }

    /// <summary>
    /// 弾丸を破棄する（プールに戻す）処理
    /// </summary>
    private void KillBullet()
    {
        if (IsDead) return;
        IsDead = true;
    }
}