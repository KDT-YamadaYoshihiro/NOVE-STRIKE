// CharacterBase.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class CharaBase : MonoBehaviour, IDamageable
{
    [Header("Base Character Settings")]
    // インスペクター（GUI）でのみ設定可能、外部スクリプトからは完全非公開（安全）
    [SerializeField] private float m_defaultMaxHealth = 100f;
    [SerializeField] private float m_defaultBaseMoveSpeed = 5f;
    // 外部のシステム（UIやバフ管理など）がこのキャラクターの状態を知るための、読み取り専用プロパティ
    public CharaStatusModel Status { get; protected set; }

    private Rigidbody m_rigidbody;

    // 子クラスが初期値を利用できるようにするためのプロパティ（カプセル化の維持）
    protected float DefaultMaxHealth => m_defaultMaxHealth;
    protected float DefaultBaseMoveSpeed => m_defaultBaseMoveSpeed;
    protected Rigidbody CachedRigidbody => m_rigidbody;

    public virtual void InitializeCharacter()
    {
        m_rigidbody = GetComponent<Rigidbody>();

        // 3Dトップダウン特有の設定（勝手に転倒するのを防ぎ、重力を切るか制御する）
        m_rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        m_rigidbody.useGravity = false;

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

    // IDamageable の実装
    public virtual void TakeDamage(float arg_damage)
    {
        Status?.ApplyDamage(arg_damage);
    }

    // 死亡時の処理
    protected virtual void HandleDeath()
    {
        ReleaseCharacter();
        Destroy(gameObject);
    }
}