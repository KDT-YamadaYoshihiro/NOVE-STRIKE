using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class BulletMover : MonoBehaviour
{
    private Rigidbody m_rigidbody;
    private float m_speed;
    private Vector3 m_direction;

    public event Action<Collider> OnTriggerEntered;

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="arg_speed"></param>
    /// <param name="arg_direction"></param>
    public void InitializeMover(float arg_speed, Vector3 arg_direction)
    {
        m_rigidbody = GetComponent<Rigidbody>();
        if(m_rigidbody != null)
        {
            // 弾に重力は適用しない
            m_rigidbody.useGravity = false; 
        }

        m_speed = arg_speed;
        m_direction = arg_direction;
    }

    /// <summary>
    /// 移動処理
    /// </summary>
    public void TickFixedUpdate()
    {
        if (m_rigidbody != null)
        {
            // 物理演算がスリープ状態（停止）になっていたら強制的に起こす
            if (m_rigidbody.IsSleeping())
            {
                m_rigidbody.WakeUp();
            }

            // 速度を代入して移動させる
            m_rigidbody.linearVelocity = m_direction * m_speed;
        }
    }

    /// <summary>
    /// 衝突判定イベント
    /// </summary>
    /// <param name="arg_other"></param>
    private void OnTriggerEnter(Collider arg_other)
    {
        OnTriggerEntered?.Invoke(arg_other);
    }
}