using UnityEngine;

[RequireComponent(typeof(BulletMover), typeof(Collider))]
public class PlayerBullet : MonoBehaviour
{
    private BulletMover m_bulletMover;
    private BulletData m_bulletData;
    private float m_currentLifeTime;
    private bool m_isDead;

    public bool IsDead => m_isDead;

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="arg_data"></param>
    /// <param name="arg_direction"></param>
    public void InitializeBullet(BulletData arg_data, Vector3 arg_direction)
    {
        m_bulletMover = GetComponent<BulletMover>();
        m_bulletData = arg_data;
        m_currentLifeTime = 0f;
        m_isDead = false;

        m_bulletMover.InitializeMover(m_bulletData.Speed, arg_direction);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="arg_deltaTime"></param>
    public void TickUpdate(float arg_deltaTime)
    {
        if (m_isDead) return;

        m_currentLifeTime += arg_deltaTime;
        if (m_currentLifeTime >= m_bulletData.LifeTime)
        {
            DestroyBullet();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void TickFixedUpdate()
    {
        if (m_isDead) return;

        m_bulletMover.TickFixedUpdate();
    }

    /// <summary>
    /// 3Dの衝突判定（OnTriggerEnter）に変更
    /// </summary>
    /// <param name="arg_other"></param>
    private void OnTriggerEnter(Collider arg_other)
    {
        if (m_isDead) return;

        IDamageable target = arg_other.GetComponent<IDamageable>();
        if (target != null)
        {
            target.TakeDamage(m_bulletData.Damage);
            DestroyBullet();
        }
    }

    /// <summary>
    /// 消滅メソッド
    /// </summary>
    private void DestroyBullet()
    {
        m_isDead = true;
        Destroy(gameObject);
    }
}