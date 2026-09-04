using UnityEngine;

/// <summary>
/// 部屋の配置に用いるグリッドの規格を定義するデータ
/// </summary>
/// <remarks>
/// 部屋のサイズも接続位置も、すべてこのセルサイズを基準に決まる。
/// 値を変更すると既存の部屋プレハブと整合しなくなるため、変更は慎重に行うこと。
/// </remarks>
[CreateAssetMenu(fileName = "RoomGridSettings", menuName = "Game/Room Grid Settings")]
public class RoomGridSettings : ScriptableObject
{
    [Header("Grid (グリッド規格)")]
    [Tooltip("グリッド1セルの1辺の長さ(m)")] public float CellSize = 20f;

    [Header("Layout (配置)")]
    [Tooltip("部屋どうしの間隔(m)。0にすると部屋が密着する")] public float RoomSpacing = 0f;

    /// <summary>
    /// グリッド座標をワールド座標に変換する
    /// </summary>
    public Vector3 GridToWorld(Vector2Int arg_gridPosition)
    {
        float pitch = CellSize + RoomSpacing;
        return new Vector3(arg_gridPosition.x * pitch, 0f, arg_gridPosition.y * pitch);
    }

    /// <summary>
    /// サイズ規格をワールド上の寸法(X,Z)に変換する
    /// </summary>
    public Vector2 GetRoomExtent(RoomSize arg_size)
    {
        Vector2Int cells = RoomGeometry.GetCellCount(arg_size);
        return new Vector2(cells.x * CellSize, cells.y * CellSize);
    }
}
