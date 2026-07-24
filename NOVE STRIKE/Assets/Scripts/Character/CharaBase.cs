// CharacterBase.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class CharaBase : MonoBehaviour, IDamageable
{
    [Header("Base Character Settings")]
    // インスペクター（GUI）でのみ設定可能、外部スクリプトからは完全非公開（安全）
    [SerializeField] private float m_defaultMaxHealth = 100f;
    [SerializeField] private float m_defaultBaseMoveSpeed = 5f;
    [SerializeField] private float m_defaultAttackPower = 10f;
    [SerializeField] private float m_defaultDefensePower = 0f;

    [SerializeField] private Transform m_firePoint;

    // 外部のシステム（UIやバフ管理など）がこのキャラクターの状態を知るための、読み取り専用プロパティ
    public CharaStatusModel Status { get; protected set; }
    public Transform FirePoint => m_firePoint;
    protected Rigidbody CachedRigidbody { get; private set; }

    // 子クラスが初期値を利用できるようにするためのプロパティ（カプセル化の維持）
    protected float DefaultMaxHealth => m_defaultMaxHealth;
    protected float DefaultBaseMoveSpeed => m_defaultBaseMoveSpeed;
    protected float DefaultAttackPower => m_defaultAttackPower;
    protected float DefaultDefensePower => m_defaultDefensePower;
    public virtual void InitializeCharacter()
    {
        CachedRigidbody = GetComponent<Rigidbody>();

        // 3Dトップダウン特有の設定（勝手に転倒するのを防ぎ、重力を切るか制御する）
        CachedRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        CachedRigidbody.useGravity = false;

        // 具体的なModelの生成を子クラスに行わせる
        SetupStatusModel();

        if (Status != null)
        {
            Status.OnDeath += HandleDeath;
        }
    }

    public virtual void ReleaseCharacter()
    {
        if (Status != null)
        {
            Status.OnDeath -= HandleDeath;
        }
    }

    // 抽象メソッド（子クラスにStatusModelの構築を強制）
    protected abstract void SetupStatusModel();

    /// <summary>
    /// 物理移動の共通処理
    /// </summary>
    public virtual void ExecuteMove(Vector3 arg_velocity)
    {
        if (CachedRigidbody != null)
        {
            CachedRigidbody.linearVelocity = arg_velocity;
        }
    }

    /// <summary>
    /// 物理回転の共通処理
    /// </summary>
    public virtual void ExecuteRotation(Quaternion arg_rotation)
    {
        if (CachedRigidbody != null && arg_rotation != Quaternion.identity)
        {
            CachedRigidbody.MoveRotation(arg_rotation);
        }
    }

    // IDamageable の実装
    public virtual void TakeDamage(float arg_damage)
    {
        Status?.ApplyDamage(arg_damage);
    }

    /// <summary>
    /// 回復処理
    /// </summary>
    public virtual void Heal(float arg_amount)
    {
        Status?.Heal(arg_amount);
    }

    // 死亡時の処理
    protected virtual void HandleDeath()
    {
        ReleaseCharacter();
        Destroy(gameObject);
    }
}