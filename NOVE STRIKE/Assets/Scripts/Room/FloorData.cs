using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 1フロアの構成を定義するデータ
/// </summary>
/// <remarks>
/// 部屋数や各種部屋の出現数はここでのみ決める。生成側に数値を持たせない。
/// </remarks>
[CreateAssetMenu(fileName = "NewFloorData", menuName = "Game/Floor Data")]
public class FloorData : ScriptableObject
{
    [Header("Basic Info (基本情報)")]
    public string FloorID;

    [Header("Room Count (部屋数)")]
    [Tooltip("1ランで通過する部屋数の下限(ボス部屋を含む)")] public int MinRoomCount = 8;
    [Tooltip("1ランで通過する部屋数の上限(ボス部屋を含む)")] public int MaxRoomCount = 12;
    [Tooltip("宝箱部屋の出現数")] public int TreasureRoomCount = 1;
    [Tooltip("休憩部屋の出現数")] public int RestRoomCount = 1;

    [Header("Room Pools (部屋の抽選プール)")]
    public List<RoomData> BattleRooms = new List<RoomData>();
    public List<RoomData> TreasureRooms = new List<RoomData>();
    public List<RoomData> RestRooms = new List<RoomData>();
    [Tooltip("ボス部屋。抽選せず固定で使用する")] public RoomData BossRoom;

    [Header("Grid (グリッド規格)")]
    public RoomGridSettings GridSettings;

    /// <summary>
    /// 種別に対応する抽選プールを返す
    /// </summary>
    public List<RoomData> GetPool(RoomType arg_type)
    {
        switch (arg_type)
        {
            case RoomType.Battle: return BattleRooms;
            case RoomType.Treasure: return TreasureRooms;
            case RoomType.Rest: return RestRooms;
            default:
                Debug.LogError($"種別 {arg_type} に対応する抽選プールはありません。");
                return null;
        }
    }

    /// <summary>
    /// 重み付き抽選で部屋を1つ選ぶ。選べない場合は null
    /// </summary>
    public RoomData PickRoom(RoomType arg_type)
    {
        // ボス部屋は抽選せず固定
        if (arg_type == RoomType.Boss) { return BossRoom; }

        List<RoomData> pool = GetPool(arg_type);
        if (pool == null || pool.Count == 0)
        {
            Debug.LogError($"[{name}] 種別 {arg_type} の抽選プールが空です。");
            return null;
        }

        int totalWeight = 0;
        foreach (RoomData room in pool)
        {
            if (room == null) { continue; }
            totalWeight += Mathf.Max(1, room.Weight);
        }

        if (totalWeight <= 0)
        {
            Debug.LogError($"[{name}] 種別 {arg_type} の抽選プールに有効な部屋がありません。");
            return null;
        }

        int value = Random.Range(0, totalWeight);
        foreach (RoomData room in pool)
        {
            if (room == null) { continue; }

            value -= Mathf.Max(1, room.Weight);
            if (value < 0) { return room; }
        }

        // 重みの合計計算と抽選がずれた場合の保険
        return pool[pool.Count - 1];
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (MinRoomCount > MaxRoomCount)
        {
            Debug.LogWarning($"[{name}] MinRoomCount が MaxRoomCount を超えています。", this);
        }

        // ボス部屋と、種別ごとの必要数を差し引いても戦闘部屋が残るかを確認する
        int reserved = 1 + TreasureRoomCount + RestRoomCount;
        if (reserved > MinRoomCount)
        {
            Debug.LogWarning($"[{name}] ボス・宝箱・休憩の合計 {reserved} 部屋が MinRoomCount({MinRoomCount}) を超えています。戦闘部屋が配置されません。", this);
        }
    }
#endif
}
