using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFactory
{
    private readonly EnemyDatabase m_database;
    private readonly Transform m_bulletContainer;
    private readonly Action<Bullet, BulletPool> m_onBulletSpawned;

    // 複数の弾プレハブに対応するため
    private readonly Dictionary<GameObject, BulletPool> m_pools = new Dictionary<GameObject, BulletPool>();

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="arg_database"></param>
    /// <param name="arg_bulletContainer"></param>
    /// <param name="arg_onBulletSpawned"></param>
    public EnemyFactory(EnemyDatabase arg_database, Transform arg_bulletContainer, Action<Bullet, BulletPool> arg_onBulletSpawned)
    {
        m_database = arg_database;
        m_bulletContainer = arg_bulletContainer;
        m_onBulletSpawned = arg_onBulletSpawned;
    }

    /// <summary>
    /// 生成処理
    /// </summary>
    /// <param name="arg_enemyId"></param>
    /// <param name="arg_spawnPosition"></param>
    /// <param name="arg_spawnRotation"></param>
    /// <returns></returns>
    public EnemyController CreateEnemy(string arg_enemyId, Vector3 arg_spawnPosition, Quaternion arg_spawnRotation)
    {
        EnemyData data = m_database.GetEnemyData(arg_enemyId);
        if (data == null || data.BasePrefab == null) return null;

        GameObject enemyObject = UnityEngine.Object.Instantiate(data.BasePrefab, arg_spawnPosition, arg_spawnRotation);
        EnemyController controller = enemyObject.GetComponent<EnemyController>();

        if (controller != null)
        {
            // ステータスの値を見て、自動で行動パターンを決定する
            IEnemyMoveBehavior moveBehavior = GenerateMoveBehavior(data.MoveSpeed);
            IEnemyAttackBehavior attackBehavior = GenerateAttackBehavior(data.IsKamikaze, data.BulletPrefab);

            controller.InitializeEnemy(data, moveBehavior, attackBehavior);
        }
        else
        {
            Debug.LogError("生成したプレハブに EnemyController がアタッチされていません。");
            UnityEngine.Object.Destroy(enemyObject);
        }

        return controller;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="arg_type"></param>
    /// <returns></returns>
    private IEnemyMoveBehavior GenerateMoveBehavior(float arg_moveSpeed)
    {
        if (arg_moveSpeed <= 0f)
        {
            return new MoveBehaviorStationary();
        }
        return new MoveBehaviorChase();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="arg_type"></param>
    /// <param name="arg_bulletPrefab"></param>
    /// <returns></returns>
    private IEnemyAttackBehavior GenerateAttackBehavior(bool arg_isKamikaze, GameObject arg_bulletPrefab)
    {
        if (arg_bulletPrefab != null)
        {
            if (!m_pools.TryGetValue(arg_bulletPrefab, out BulletPool pool))
            {
                pool = new BulletPool(arg_bulletPrefab, m_bulletContainer, 20);
                m_pools.Add(arg_bulletPrefab, pool);
            }
            return new AttackBehaviorSingle(pool, m_onBulletSpawned);
        }

        // 自爆にチェックが入っていれば自爆特攻
        if (arg_isKamikaze)
        {
            return new AttackBehaviorKamikaze();
        }

        // どちらも設定されていなければ攻撃しない
        return new AttackBehaviorNone();
    }
}