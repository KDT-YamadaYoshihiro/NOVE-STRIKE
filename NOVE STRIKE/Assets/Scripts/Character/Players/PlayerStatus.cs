using System;

public class PlayerStatus : CharaStatusModel
{
    // 外部からは読み取り専用
    public int Level { get; private set; } = 1;
    public float CurrentExp { get; private set; } = 0f;
    public float NextLevelExp { get; private set; } = 100f;

    public event Action<int> OnLevelUp;
    public event Action<float, float> OnExpChanged; // (current, next)

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="arg_maxHealth"></param>
    /// <param name="arg_baseMoveSpeed"></param>
    public PlayerStatus(float arg_maxHealth, float arg_baseMoveSpeed) : base(arg_maxHealth, arg_baseMoveSpeed)
    {
    }

    /// <summary>
    /// 経験値獲得メソッド
    /// </summary>
    /// <param name="arg_amount"></param>
    public void AddExp(float arg_amount)
    {
        CurrentExp += arg_amount;
        if (CurrentExp >= NextLevelExp)
        {
            LevelUp();
        }
        OnExpChanged?.Invoke(CurrentExp, NextLevelExp);
    }

    /// <summary>
    /// レベルアップメソッド
    /// </summary>
    private void LevelUp()
    {
        CurrentExp -= NextLevelExp;
        Level++;
        NextLevelExp *= 1.2f;

        MaxHealth += 10f;
        Heal(MaxHealth);

        OnLevelUp?.Invoke(Level);
    }
}