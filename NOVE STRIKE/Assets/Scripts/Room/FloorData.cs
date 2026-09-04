using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// フロアに出現する敵と、その出やすさ
/// </summary>
[System.Serializable]
public class EnemySpawnEntry
{
    [Tooltip("EnemyDatabase に登録されている敵ID")]
    public string EnemyID;

    [Tooltip("抽選の重み。大きいほど出やすい")]
    public int Weight = 1;
}

/// <summary>
/// 本道から枝分かれさせる部屋の配置指定
/// </summary>
/// <remarks>
/// 宝箱部屋・休憩部屋は本道から1部屋だけ生える行き止まりとして配置する。
/// 位置を固定することで、回復や補給のタイミングが運に左右されないようにする。
/// </remarks>
[System.Serializable]
public class BranchRoomPlacement
{
    [Tooltip("枝道に置く部屋の種別")]
    public RoomType Type = RoomType.Treasure;

    [Tooltip("分岐元となる本道の部屋番号(0始まり)。本道の部屋数を超える場合は末尾側へ丸める")]
    public int MainPathIndex = 3;
}

/// <summary>
/// 1フロアの構成を定義するデータ
/// </summary>
/// <remarks>
/// 部屋数や枝道の位置はここでのみ決める。生成側に数値を持たせない。
/// </remarks>
[CreateAssetMenu(fileName = "NewFloorData", menuName = "Game/Floor Data")]
public class FloorData : ScriptableObject
{
    [Header("Basic Info (基本情報)")]
    public string FloorID;

    [Header("Main Path (本道)")]
    [Tooltip("本道の部屋数の下限(ボス部屋を含む)")] public int MinRoomCount = 8;
    [Tooltip("本道の部屋数の上限(ボス部屋を含む)")] public int MaxRoomCount = 12;

    [Header("Branch Rooms (枝道の部屋)")]
    [Tooltip("本道から生やす行き止まりの部屋。位置は固定する")]
    public List<BranchRoomPlacement> BranchRooms = new List<BranchRoomPlacement>();

    [Header("Enemy Table (敵の出現テーブル)")]
    [Tooltip("このフロアに出現する敵と、その出やすさ。部屋ごとの敵はここから抽選する")]
    public List<EnemySpawnEntry> EnemyTable = new List<EnemySpawnEntry>();

    [Header("Props (小物)")]
    [Tooltip("障害物として配置するプレハブの候補")]
    public List<GameObject> ObstaclePrefabs = new List<GameObject>();
    [Tooltip("宝箱として配置するプレハブ")]
    public GameObject ChestPrefab;

    [Header("Room Pools (部屋の抽選プール)")]
    public List<RoomData> BattleRooms = new List<RoomData>();
    public List<RoomData> TreasureRooms = new List<RoomData>();
    public List<RoomData> RestRooms = new List<RoomData>();
    [Tooltip("ボス部屋。抽選せず固定で使用する")] public RoomData BossRoom;

    [Header("Grid (グリッド規格)")]
    public RoomGridSettings GridSettings;

    [Header("Generation (生成)")]
    [Tooltip("配置に失敗したときに最初からやり直す最大回数")] public int MaxBuildRetryCount = 20;

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
    /// 指定方向すべてにドアを持つ部屋を、重み付き抽選で1つ選ぶ
    /// </summary>
    /// <param name="arg_type">部屋の種別</param>
    /// <param name="arg_requiredDirections">必ずドアが必要な方向</param>
    /// <returns>条件を満たす部屋。見つからない場合は null</returns>
    public RoomData PickRoom(RoomType arg_type, IReadOnlyList<RoomDirection> arg_requiredDirections)
    {
        // ボス部屋は抽選せず固定
        if (arg_type == RoomType.Boss) { return BossRoom; }

        List<RoomData> pool = GetPool(arg_type);
        if (pool == null || pool.Count == 0)
        {
            Debug.LogError($"[{name}] 種別 {arg_type} の抽選プールが空です。");
            return null;
        }

        // 必要な方向のドアを持たない部屋を候補から外す
        List<RoomData> candidates = new List<RoomData>();
        foreach (RoomData room in pool)
        {
            if (room == null) { continue; }
            if (HasRequiredDoors(room, arg_requiredDirections)) { candidates.Add(room); }
        }

        if (candidates.Count == 0)
        {
            Debug.LogError($"[{name}] 種別 {arg_type} に、必要な方向のドアを持つ部屋がありません。部屋プレハブのドア構成を見直してください。");
            return null;
        }

        return PickByWeight(candidates);
    }

    /// <summary>
    /// 必要な方向すべてにドアがあるかを判定する
    /// </summary>
    private static bool HasRequiredDoors(RoomData arg_room, IReadOnlyList<RoomDirection> arg_requiredDirections)
    {
        if (arg_requiredDirections == null || arg_requiredDirections.Count == 0) { return true; }

        RoomDefinition definition = arg_room.GetDefinition();
        if (definition == null)
        {
            Debug.LogError($"[{arg_room.name}] プレハブに RoomDefinition が付いていません。");
            return false;
        }

        foreach (RoomDirection direction in arg_requiredDirections)
        {
            if (!definition.HasDoor(direction)) { return false; }
        }
        return true;
    }

    /// <summary>
    /// 重み付き抽選で1つ選ぶ
    /// </summary>
    private static RoomData PickByWeight(List<RoomData> arg_candidates)
    {
        int totalWeight = 0;
        foreach (RoomData room in arg_candidates)
        {
            totalWeight += Mathf.Max(1, room.Weight);
        }

        int value = Random.Range(0, totalWeight);
        foreach (RoomData room in arg_candidates)
        {
            value -= Mathf.Max(1, room.Weight);
            if (value < 0) { return room; }
        }

        // 重みの合計計算と抽選がずれた場合の保険
        return arg_candidates[arg_candidates.Count - 1];
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (MinRoomCount > MaxRoomCount)
        {
            Debug.LogWarning($"[{name}] MinRoomCount が MaxRoomCount を超えています。", this);
        }

        // 本道はボス部屋と開始部屋を含むため、最低2部屋は必要
        if (MinRoomCount < 2)
        {
            Debug.LogWarning($"[{name}] 本道は開始部屋とボス部屋で最低2部屋必要です。", this);
        }

        if (EnemyTable == null || EnemyTable.Count == 0)
        {
            Debug.LogWarning($"[{name}] 敵の出現テーブルが空です。戦闘部屋に敵が配置されません。", this);
        }

        foreach (BranchRoomPlacement branch in BranchRooms)
        {
            if (branch == null) { continue; }

            // ボス部屋は本道の終端に固定されるため、枝道には置けない
            if (branch.Type == RoomType.Boss || branch.Type == RoomType.Battle)
            {
                Debug.LogWarning($"[{name}] 枝道に置けるのは宝箱部屋と休憩部屋のみです。({branch.Type})", this);
            }

            if (branch.MainPathIndex < 0)
            {
                Debug.LogWarning($"[{name}] MainPathIndex に負の値が指定されています。", this);
            }
        }
    }
#endif
}
