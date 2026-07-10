// EnemyFactory.cs
using UnityEngine;

public class EnemyFactory
{
    private readonly EnemyDatabase m_database;
    private readonly Transform m_bulletContainer;

    public EnemyFactory(EnemyDatabase arg_database, Transform arg_bulletContainer)
    {
        m_database = arg_database;
        m_bulletContainer = arg_bulletContainer;
    }

    /// <summary>
    /// IDを指定してエネミーを生成する
    /// </summary>
    public EnemyController CreateEnemy(string arg_enemyId, Vector3 arg_spawnPosition, Quaternion arg_spawnRotation)
    {
        EnemyData data = m_database.GetEnemyData(arg_enemyId);
        if (data == null || data.BasePrefab == null) return null;

        GameObject enemyObject = Object.Instantiate(data.BasePrefab, arg_spawnPosition, arg_spawnRotation);
        EnemyController controller = enemyObject.GetComponent<EnemyController>();

        if (controller != null)
        {
            IEnemyMoveBehavior moveBehavior = GenerateMoveBehavior(data.MoveType);
            IEnemyAttackBehavior attackBehavior = GenerateAttackBehavior(data.AttackType, data.BulletPrefab);

            controller.InitializeEnemy(data, moveBehavior, attackBehavior);
        }
        else
        {
            Debug.LogError("生成したプレハブに EnemyController がアタッチされていません。");
            Object.Destroy(enemyObject);
        }

        return controller;
    }

    private IEnemyMoveBehavior GenerateMoveBehavior(EnemyMoveType arg_type)
    {
        switch (arg_type)
        {
            case EnemyMoveType.Chase: return new MoveBehaviorChase();
            case EnemyMoveType.Stationary: return new MoveBehaviorStationary();
            default: return new MoveBehaviorStationary();
        }
    }

    private IEnemyAttackBehavior GenerateAttackBehavior(EnemyAttackType arg_type, GameObject arg_bulletPrefab)
    {
        switch (arg_type)
        {
            case EnemyAttackType.SingleShot: return new AttackBehaviorSingle(arg_bulletPrefab, m_bulletContainer);
            default: return new AttackBehaviorSingle(arg_bulletPrefab, m_bulletContainer);
        }
    }
}