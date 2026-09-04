using UnityEngine;

/// <summary>
/// 部屋の種別
/// </summary>
public enum RoomType
{
    Battle,     // 戦闘部屋。敵を全滅させるとクリア
    Treasure,   // 宝箱部屋。戦闘なし
    Rest,       // 休憩部屋。戦闘なし、HP回復
    Boss,       // ボス部屋。フロアの終端
}

/// <summary>
/// 部屋のサイズ規格。グリッドセル単位で表す
/// </summary>
public enum RoomSize
{
    Size1x1,    // 1セル×1セル
    Size2x1,    // 2セル×1セル
    Size2x2,    // 2セル×2セル。ボス部屋専用
}

/// <summary>
/// 部屋の接続方向
/// </summary>
public enum RoomDirection
{
    North,
    East,
    South,
    West,
}

/// <summary>
/// スポーンポイントの種別
/// </summary>
public enum SpawnPointType
{
    Enemy,          // 敵の湧き位置
    Obstacle,       // 障害物・遮蔽物の配置位置
    Chest,          // 宝箱の配置位置
    PlayerStart,    // プレイヤーの入場位置
}

/// <summary>
/// 部屋のサイズと方向にまつわる計算をまとめたユーティリティ
/// </summary>
public static class RoomGeometry
{
    /// <summary>
    /// サイズ規格をグリッドセル数(X,Z)に変換する
    /// </summary>
    public static Vector2Int GetCellCount(RoomSize arg_size)
    {
        switch (arg_size)
        {
            case RoomSize.Size1x1: return new Vector2Int(1, 1);
            case RoomSize.Size2x1: return new Vector2Int(2, 1);
            case RoomSize.Size2x2: return new Vector2Int(2, 2);
            default:
                Debug.LogError($"未定義のRoomSizeです: {arg_size}");
                return new Vector2Int(1, 1);
        }
    }

    /// <summary>
    /// 指定方向の辺が何セル分の長さを持つかを返す
    /// </summary>
    /// <remarks>
    /// 南北の辺はX方向のセル数、東西の辺はZ方向のセル数だけの長さを持つ。
    /// </remarks>
    public static int GetEdgeCellLength(RoomSize arg_size, RoomDirection arg_direction)
    {
        Vector2Int cells = GetCellCount(arg_size);
        return IsAlongX(arg_direction) ? cells.x : cells.y;
    }

    /// <summary>
    /// その方向の辺にドアを置けるかを判定する
    /// </summary>
    /// <remarks>
    /// ドアは常に「1セル幅の辺の中央」に置く規約とする。
    /// 辺が2セル以上の長さを持つ場合、ドア位置がグリッドに整合せず
    /// 他サイズの部屋と噛み合わなくなるため接続を許可しない。
    /// ボス部屋のみこの規約の対象外とする(RoomDefinition側で例外扱い)。
    /// </remarks>
    public static bool IsDoorAllowed(RoomSize arg_size, RoomDirection arg_direction)
    {
        return GetEdgeCellLength(arg_size, arg_direction) == 1;
    }

    /// <summary>
    /// 南北方向の辺かどうか(X方向に伸びる辺かどうか)を返す
    /// </summary>
    public static bool IsAlongX(RoomDirection arg_direction)
    {
        return arg_direction == RoomDirection.North || arg_direction == RoomDirection.South;
    }

    /// <summary>
    /// 反対方向を返す。部屋どうしの接続判定に使う
    /// </summary>
    public static RoomDirection GetOpposite(RoomDirection arg_direction)
    {
        switch (arg_direction)
        {
            case RoomDirection.North: return RoomDirection.South;
            case RoomDirection.East: return RoomDirection.West;
            case RoomDirection.South: return RoomDirection.North;
            case RoomDirection.West: return RoomDirection.East;
            default:
                Debug.LogError($"未定義のRoomDirectionです: {arg_direction}");
                return RoomDirection.North;
        }
    }

    /// <summary>
    /// 方向をグリッド座標の increment に変換する
    /// </summary>
    public static Vector2Int ToGridOffset(RoomDirection arg_direction)
    {
        switch (arg_direction)
        {
            case RoomDirection.North: return new Vector2Int(0, 1);
            case RoomDirection.East: return new Vector2Int(1, 0);
            case RoomDirection.South: return new Vector2Int(0, -1);
            case RoomDirection.West: return new Vector2Int(-1, 0);
            default:
                Debug.LogError($"未定義のRoomDirectionです: {arg_direction}");
                return Vector2Int.zero;
        }
    }
}
