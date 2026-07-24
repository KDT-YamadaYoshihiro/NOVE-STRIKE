using UnityEngine;

/// <summary>
///  インターフェース
/// </summary>
public interface IEnemyMoveBehavior
{
    void ExecuteMove(CharaBase arg_selfController, Transform arg_targetTransform, float arg_moveSpeed);
}

public interface IEnemyAttackBehavior
{
    void ExecuteAttack(CharaBase arg_selfController, Transform arg_targetTransform, EnemyStatus arg_status, BulletOwnerType arg_ownerType);
}

/// <summary>
/// 移動アルゴリズム
/// </summary>
public class MoveBehaviorStationary : IEnemyMoveBehavior
{
    public void ExecuteMove(CharaBase arg_selfController, Transform arg_targetTransform,float arg_moveSpeed)
    {
        arg_selfController.ExecuteMove(Vector3.zero);

        Vector3 direction = (arg_targetTransform.position - arg_selfController.transform.position).normalized;
        direction.y = 0;
        if(direction.sqrMagnitude > 0.01f)
        {
            arg_selfController.ExecuteRotation(Quaternion.LookRotation(direction));
        }
    }
}

/// <summary>
/// 追尾アルゴリズム
/// </summary>
public class MoveBehaviorChase : IEnemyMoveBehavior
{

    public void ExecuteMove(CharaBase arg_selfController, Transform arg_targetTransform, float arg_moveSpeed)
    {
        Vector3 direction = (arg_targetTransform.position - arg_selfController.transform.position).normalized;
        direction.y = 0;

        if(direction.sqrMagnitude > 0.01f)
        {
            arg_selfController.ExecuteRotation(Quaternion.LookRotation(direction));
            arg_selfController.ExecuteMove(direction * arg_moveSpeed);
        }

    }
}

/// <summary>
/// 単発攻撃アルゴリズム
/// </summary>
public class AttackBehaviorSingle : IEnemyAttackBehavior
{
    private readonly GameObject m_bulletPrefab;
    private readonly Transform m_bulletContainer;

    public AttackBehaviorSingle(GameObject arg_bulletPrefab, Transform arg_bulletContainer)
    {
        m_bulletPrefab = arg_bulletPrefab;
        m_bulletContainer = arg_bulletContainer;
    }

    public void ExecuteAttack(CharaBase arg_selfController, Transform arg_targetTransform, EnemyStatus arg_status, BulletOwnerType arg_ownerType)
    {
        Transform firePoint = arg_selfController.FirePoint;
        if (m_bulletPrefab == null || firePoint == null) { return; }

        Vector3 shootDirection = (arg_targetTransform.position - firePoint.position).normalized;
        shootDirection.y = 0f;

        GameObject bulletObject = m_bulletContainer != null
            ? Object.Instantiate(m_bulletPrefab, firePoint.position, firePoint.rotation, m_bulletContainer)
            : Object.Instantiate(m_bulletPrefab, firePoint.position, firePoint.rotation);

        BulletData data = new BulletData(
                        arg_status.AttackPower, // ステータスの攻撃力を付与！
                        15f,
                        5f,
                        arg_ownerType,
                        shootDirection
                    );

        // 純粋C#クラスとして new で生み出し、GameObjectとデータを紐づける
        Bullet bullet = new Bullet(bulletObject, data);
    }
}