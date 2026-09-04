using System;
using UnityEngine;

public class PlayerCombatPresenter : MonoBehaviour
{
    [Header("Weapon Settings")]
    // 弾プレハブ
    [SerializeField] private GameObject m_bulletPrefab;
    // 発射位置
    [SerializeField] private Transform m_firePoint;
    // 弾の親オブジェクト
    [SerializeField] private Transform m_bulletContainer;

    [Header("Vampire Survivor Auto-Attack")]
    // 発射間隔
    [SerializeField] private float m_fireInterval = 0.5f;
    // 索敵範囲
    [SerializeField] private float m_attackRange = 15f;
    // 敵のレイヤー
    [SerializeField] private LayerMask m_enemyLayer;

    private BulletPool m_bulletPool;
    private float m_fireTimer;
    private PlayerStatus m_playerStatus;

    public event Action<Bullet> OnBulletSpawned;
    public BulletPool Pool => m_bulletPool;

    /// <summary>
    /// 初期化
    /// </summary>
    public void InitializeCombat(PlayerStatus arg_status)
    {
        m_playerStatus = arg_status;
        m_bulletPool = new BulletPool(m_bulletPrefab, m_bulletContainer, 30);
        m_fireTimer = m_fireInterval;
    }

    /// <summary>
    /// 解放メソッド
    /// </summary>
    public void ReleaseCombat()
    {
    }

    /// <summary>
    /// 更新ルーチン
    /// </summary>
    public void TickUpdate(float arg_deltaTime)
    {
        if(m_playerStatus == null) { return; }

        m_fireTimer -= arg_deltaTime;
        if (m_fireTimer <= 0f)
        {
            ExecuteAutoFire();
            m_fireTimer = m_fireInterval;
        }
    }

    private void ExecuteAutoFire()
    {
        if (m_bulletPrefab == null || m_firePoint == null) return;

        // 範囲内の一番近くの敵を探す
        Collider[] hits = Physics.OverlapSphere(transform.position, m_attackRange, m_enemyLayer);
        Transform nearestEnemy = null;
        float minDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearestEnemy = hit.transform;
            }
        }

        Vector3 shootDirection;

        // 「敵がいればその方向へ、いなければプレイヤーが向いている方向へ発射」
        if (nearestEnemy != null)
        {
            shootDirection = (nearestEnemy.position - m_firePoint.position).normalized;
        }
        else
        {
            shootDirection = m_firePoint.forward;
        }

        shootDirection.y = 0f;
        if (shootDirection.sqrMagnitude < 0.01f) shootDirection = transform.forward;

        // プールから弾を取得し、プレイヤーの攻撃力を乗せて発射
        BulletData bulletData = new BulletData(
            m_playerStatus.AttackPower,
            20f,
            3f,
            BulletOwnerType.Player,
            shootDirection
        );

        Bullet playerBullet = m_bulletPool.Rent(bulletData, m_firePoint.position, Quaternion.LookRotation(shootDirection));
        OnBulletSpawned?.Invoke(playerBullet);
    }
}
