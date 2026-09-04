using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 論理レイアウトをもとに、実際の部屋オブジェクトをシーンへ配置する
/// </summary>
/// <remarks>
/// 全部屋を同時にグリッド上へ配置する。ドアをくぐると隣の部屋へ連続して移動できる。
/// レイアウトの決定は FloorLayoutBuilder が行い、ここでは配置だけを担当する。
/// </remarks>
public class FloorBuilder
{
    private readonly RoomGridSettings m_gridSettings;
    private readonly Transform m_parent;

    // 配置済みの部屋オブジェクト。破棄と参照解決に使う
    private readonly Dictionary<RoomPlacement, RoomDefinition> m_spawned = new Dictionary<RoomPlacement, RoomDefinition>();

    public FloorBuilder(RoomGridSettings arg_gridSettings, Transform arg_parent)
    {
        m_gridSettings = arg_gridSettings;
        m_parent = arg_parent;
    }

    /// <summary>
    /// 配置済みの部屋。key はレイアウト上の部屋
    /// </summary>
    public IReadOnlyDictionary<RoomPlacement, RoomDefinition> SpawnedRooms => m_spawned;

    /// <summary>
    /// レイアウトに従って全部屋を配置する
    /// </summary>
    /// <returns>成功したら true</returns>
    public bool Build(FloorLayout arg_layout)
    {
        if (arg_layout == null)
        {
            Debug.LogError("FloorLayout が null です。");
            return false;
        }

        if (m_gridSettings == null)
        {
            Debug.LogError("RoomGridSettings が指定されていません。");
            return false;
        }

        Clear();

        foreach (RoomPlacement placement in arg_layout.Rooms)
        {
            if (!SpawnRoom(placement)) { return false; }
        }

        return true;
    }

    /// <summary>
    /// 配置済みの部屋をすべて破棄する
    /// </summary>
    public void Clear()
    {
        foreach (KeyValuePair<RoomPlacement, RoomDefinition> pair in m_spawned)
        {
            if (pair.Value == null) { continue; }
            Object.Destroy(pair.Value.gameObject);
        }
        m_spawned.Clear();
    }

    /// <summary>
    /// 1部屋を配置する
    /// </summary>
    private bool SpawnRoom(RoomPlacement arg_placement)
    {
        GameObject prefab = arg_placement.Data != null ? arg_placement.Data.RoomPrefab : null;
        if (prefab == null)
        {
            Debug.LogError($"部屋プレハブが設定されていません。({arg_placement.Type})");
            return false;
        }

        Vector3 position = m_gridSettings.GridToWorld(arg_placement.OriginCell);
        GameObject instance = Object.Instantiate(prefab, position, Quaternion.identity, m_parent);
        instance.name = $"Room_{arg_placement.Type}_{arg_placement.OriginCell.x}_{arg_placement.OriginCell.y}";

        RoomDefinition definition = instance.GetComponent<RoomDefinition>();
        if (definition == null)
        {
            Debug.LogError($"[{instance.name}] RoomDefinition が付いていません。");
            Object.Destroy(instance);
            return false;
        }

        m_spawned[arg_placement] = definition;
        return true;
    }
}
