using UnityEngine;

/// <summary>
/// 弾丸の発射元を識別するフラグ
/// </summary>
public enum BulletOwnerType
{
    Player,
    Enemy
}

public class BulletData
{
    // 弾丸のダメージを保持するプロパティ
    public float Damage { get; private set; }
    // 弾丸の速度を保持するプロパティ
    public float Speed { get; private set; }
    // 弾丸の寿命を保持するプロパティ
    public float LifeTime { get; private set; }
    // 弾丸の発射元を保持するプロパティ
    public BulletOwnerType OwnerType { get; private set; }
    // 発射方向を保持するプロパティ
    public Vector3 Direction { get; set; }

    // 初期化用のコンストラクタ
    public BulletData(float arg_damage, float arg_speed, float arg_lifeTime, BulletOwnerType arg_ownerType, Vector3 arg_direction)
    {
        Damage = arg_damage;
        Speed = arg_speed;
        LifeTime = arg_lifeTime;
        OwnerType = arg_ownerType;
        Direction = arg_direction;
    }
}