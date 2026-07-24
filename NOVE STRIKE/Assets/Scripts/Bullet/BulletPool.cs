using System.Collections.Generic;
using UnityEngine;

public class BulletPool
{
    private readonly GameObject m_prefab;
    private readonly Transform m_container;
    private readonly Queue<Bullet> m_pool;

    public BulletPool(GameObject arg_prefab, Transform arg_container, int arg_initialCapacity = 30)
    {
        m_prefab = arg_prefab;
        m_container = arg_container;
        m_pool = new Queue<Bullet>(arg_initialCapacity);

        for (int i = 0; i < arg_initialCapacity; i++)
        {
            CreateNewBulletAndEnqueue();
        }
    }

    private void CreateNewBulletAndEnqueue()
    {
        GameObject go = Object.Instantiate(m_prefab, m_container);
        go.SetActive(false);
        Bullet newBullet = new Bullet(go);
        m_pool.Enqueue(newBullet);
    }

    public Bullet Rent(BulletData arg_data, Vector3 arg_position, Quaternion arg_rotation)
    {
        if (m_pool.Count == 0)
        {
            CreateNewBulletAndEnqueue();
        }

        Bullet bullet = m_pool.Dequeue();
        bullet.GameObject.transform.SetPositionAndRotation(arg_position, arg_rotation);
        bullet.GameObject.SetActive(true);
        bullet.Setup(arg_data);

        return bullet;
    }

    public void ReturnToPool(Bullet arg_bullet)
    {
        arg_bullet.GameObject.SetActive(false);
        m_pool.Enqueue(arg_bullet);
    }
}