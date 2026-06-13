using System;
using UnityEngine;

public abstract class CharaStatusModel
{
    /// <summary>
    /// フィールド
    /// </summary>
    public float MaxHealth { get; protected set; }
    public float CurrentHealth { get; protected set; }
    public float BaseMoveSpeed { get; protected set; }

    /// <summary>
    /// 共有イベント
    /// </summary>
    public event Action<float, float> OnHealthChanged; // (current, max)
    public event Action OnDeath;

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="arg_maxHealth"></param>
    /// <param name="arg_baseMoveSpeed"></param>
    protected CharaStatusModel(float arg_maxHealth, float arg_baseMoveSpeed)
    {
        MaxHealth = arg_maxHealth;
        CurrentHealth = arg_maxHealth;
        BaseMoveSpeed = arg_baseMoveSpeed;
    }

    /// <summary>
    /// ダメージ計算メソッド
    /// </summary>
    /// <param name="arg_damage"></param>
    public virtual void ApplyDamage(float arg_damage)
    {
        if (CurrentHealth <= 0) { return; }

        CurrentHealth -= arg_damage;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);

        // 変更をView（UI等）に通知
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

        if (CurrentHealth <= 0)
        {
            OnDeath?.Invoke();
        }
    }

    /// <summary>
    /// 回復メソッド
    /// </summary>
    /// <param name="arg_amount"></param>
    public virtual void Heal(float arg_amount)
    {
        if (CurrentHealth <= 0) { return; }

        CurrentHealth += arg_amount;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    /// <summary>
    /// 
    /// </summary>
    private void ExecuteDeath()
    {
        OnDeath?.Invoke();
    }
}