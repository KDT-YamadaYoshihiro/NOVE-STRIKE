public class BulletData
{
    public float Damage { get; private set; }
    public float Speed { get; private set; }
    public float LifeTime { get; private set; }

    public BulletData(float arg_damage, float arg_speed, float arg_lifeTime)
    {
        Damage = arg_damage;
        Speed = arg_speed;
        LifeTime = arg_lifeTime;
    }
}