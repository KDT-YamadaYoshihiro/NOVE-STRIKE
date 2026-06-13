using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BulletMover : MonoBehaviour
{
    private Rigidbody m_rigidbody;
    private float m_speed;
    private Vector3 m_direction; 

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="arg_speed"></param>
    /// <param name="arg_direction"></param>
    public void InitializeMover(float arg_speed, Vector3 arg_direction)
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_rigidbody.useGravity = false; // 弾に重力は適用しない

        m_speed = arg_speed;
        m_direction = arg_direction.normalized;
    }

    /// <summary>
    /// 移動処理
    /// </summary>
    public void TickFixedUpdate()
    {
        // 水平方向（3D空間）への移動直線処理
        m_rigidbody.linearVelocity = m_direction * m_speed;
    }
}