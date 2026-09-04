using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// シーンに配置された部屋の実行時状態
/// </summary>
/// <remarks>
/// 全部屋を同時に配置するため、部屋ごとに「まだ中身を配置していない」状態を持つ。
/// 中身の配置はプレイヤーが入室したときに行う。
/// </remarks>
public class RoomRuntime
{
    /// <summary>レイアウト上の情報</summary>
    public RoomPlacement Placement { get; private set; }

    /// <summary>シーン上の部屋</summary>
    public RoomDefinition Definition { get; private set; }

    /// <summary>中身の配置が済んでいるか</summary>
    public bool IsPopulated { get; private set; }

    /// <summary>この部屋に配置された敵</summary>
    public IReadOnlyList<EnemyController> Enemies => m_enemies;

    private readonly List<EnemyController> m_enemies = new List<EnemyController>();

    public RoomRuntime(RoomPlacement arg_placement, RoomDefinition arg_definition)
    {
        Placement = arg_placement;
        Definition = arg_definition;
    }

    /// <summary>
    /// 配置済みとして記録する
    /// </summary>
    public void MarkPopulated(IEnumerable<EnemyController> arg_enemies)
    {
        IsPopulated = true;

        m_enemies.Clear();
        if (arg_enemies == null) { return; }

        foreach (EnemyController enemy in arg_enemies)
        {
            if (enemy != null) { m_enemies.Add(enemy); }
        }
    }

    /// <summary>
    /// この部屋の敵がすべて倒されたかを返す
    /// </summary>
    /// <remarks>
    /// 部屋のクリア判定(フェーズ1-4)から使う。
    /// 倒された敵は Destroy されるため、参照が null になったものを撃破済みとみなす。
    /// </remarks>
    public bool IsCleared()
    {
        if (!IsPopulated) { return false; }

        foreach (EnemyController enemy in m_enemies)
        {
            if (enemy != null) { return false; }
        }
        return true;
    }
}
