using UnityEngine;
using System;

/// <summary>
/// 移動インターフェース
/// </summary>
public interface IEnemyMoveBehavior
{
    void ExecuteMove(CharaBase arg_selfController, Transform arg_targetTransform, float arg_moveSpeed);
}

/// <summary>
/// 攻撃インターフェース
/// </summary>
public interface IEnemyAttackBehavior
{
    void ExecuteAttack(CharaBase arg_selfController, Transform arg_targetTransform, EnemyStatus arg_status, BulletOwnerType arg_ownerType);
}

/// <summary>
/// 移動処理
/// </summary>
public class MoveBehaviorStationary : IEnemyMoveBehavior
{
    public void ExecuteMove(CharaBase arg_selfController, Transform arg_targetTransform, float arg_moveSpeed)
    {
        arg_selfController.ExecuteMove(Vector3.zero);

        Vector3 direction = arg_targetTransform.position - arg_selfController.transform.position;
        direction.y = 0f;

        // 完全に同座標でない限り、向きだけはターゲットの方を向く
        if (direction.sqrMagnitude > 0.001f)
        {
            arg_selfController.ExecuteRotation(Quaternion.LookRotation(direction.normalized));
        }
    }
}

/// <summary>
/// 追跡処理
/// </summary>
public class MoveBehaviorChase : IEnemyMoveBehavior
{
    public void ExecuteMove(CharaBase arg_selfController, Transform arg_targetTransform, float arg_moveSpeed)
    {
        // 先に高さを無視してからベクトルを計算し、最後に正規化する（ぐるぐる回るバグの修正）
        Vector3 direction = arg_targetTransform.position - arg_selfController.transform.position;
        direction.y = 0f;

        // 接触ダメージ用に常に押し入り続ける
        if (direction.sqrMagnitude > 0.001f)
        {
            Vector3 normalizedDir = direction.normalized;
            arg_selfController.ExecuteRotation(Quaternion.LookRotation(normalizedDir));
            arg_selfController.ExecuteMove(normalizedDir * arg_moveSpeed);
        }
    }
}

/// <summary>
/// 攻撃処理
/// </summary>
public class AttackBehaviorSingle : IEnemyAttackBehavior
{
    private readonly BulletPool m_bulletPool;
    private readonly Action<Bullet, BulletPool> m_onBulletSpawned;

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="arg_pool"></param>
    /// <param name="arg_onSpawned"></param>
    public AttackBehaviorSingle(BulletPool arg_pool, Action<Bullet, BulletPool> arg_onSpawned)
    {
        m_bulletPool = arg_pool;
        m_onBulletSpawned = arg_onSpawned;
    }

    /// <summary>
    /// 攻撃処理
    /// </summary>
    /// <param name="arg_selfController"></param>
    /// <param name="arg_targetTransform"></param>
    /// <param name="arg_status"></param>
    /// <param name="arg_ownerType"></param>
    public void ExecuteAttack(CharaBase arg_selfController, Transform arg_targetTransform, EnemyStatus arg_status, BulletOwnerType arg_ownerType)
    {
        Transform firePoint = arg_selfController.FirePoint;
        if (m_bulletPool == null || firePoint == null) return;

        Vector3 direction = arg_targetTransform.position - firePoint.position;
        direction.y = 0f;

        // 完全に同座標にいてゼロベクトルになる場合の安全対策（LookRotationエラー防止）
        Vector3 shootDirection = direction.sqrMagnitude > 0.001f
            ? direction.normalized
            : arg_selfController.transform.forward;

        // ステータスから攻撃力を取得
        BulletData data = new BulletData(
            arg_status.AttackPower, 
            15f,
            5f,
            arg_ownerType,
            shootDirection
        );

        // プールから弾を取得し、データをセットして発射
        Bullet bullet = m_bulletPool.Rent(data, firePoint.position, Quaternion.LookRotation(shootDirection));

        // 生成された弾と、その弾が属するプールをマネージャーに通知
        m_onBulletSpawned?.Invoke(bullet, m_bulletPool);
    }
}

/// <summary>
/// 自爆攻撃
/// </summary>
public class AttackBehaviorKamikaze : IEnemyAttackBehavior
{
    public void ExecuteAttack(CharaBase arg_selfController, Transform arg_targetTransform, EnemyStatus arg_status, BulletOwnerType arg_ownerType)
    {
        // ターゲットに直接ダメージを与える
        IDamageable target = arg_targetTransform.GetComponent<IDamageable>();
        if (target != null)
        {
            target.TakeDamage(arg_status.AttackPower);
        }

        // 自爆したので自身も死亡する（自身に致死量のダメージを与える）
        arg_selfController.TakeDamage(99999f);
    }
}

// なにもしない（攻撃手段がない敵用）
public class AttackBehaviorNone : IEnemyAttackBehavior
{
    public void ExecuteAttack(CharaBase arg_selfController, Transform arg_targetTransform, EnemyStatus arg_status, BulletOwnerType arg_ownerType)
    {
        // 何もしない
    }
}