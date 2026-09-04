using UnityEngine;

/// <summary>
/// 部屋プレハブ内のドア(接続点)を示すマーカー
/// </summary>
/// <remarks>
/// ドアは「1セル幅の辺の中央」に置く規約とする。
/// この規約により、サイズの異なる部屋どうしでも接続位置が必ず一致する。
/// </remarks>
public class RoomDoor : MonoBehaviour
{
    [Header("Door Settings (ドア設定)")]
    [Tooltip("このドアが面している方向")]
    [SerializeField] private RoomDirection m_direction = RoomDirection.North;

    [Tooltip("部屋の左下セルを(0,0)としたときの、このドアが属するセルの位置")]
    [SerializeField] private Vector2Int m_cellOffset = Vector2Int.zero;

    public RoomDirection Direction => m_direction;
    public Vector2Int CellOffset => m_cellOffset;

    /// <summary>
    /// 通過位置のワールド座標
    /// </summary>
    public Vector3 Position => transform.position;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(2f, 0.2f, 2f));
        Gizmos.DrawRay(transform.position, DirectionToVector(m_direction) * 3f);
    }

    private static Vector3 DirectionToVector(RoomDirection arg_direction)
    {
        Vector2Int offset = RoomGeometry.ToGridOffset(arg_direction);
        return new Vector3(offset.x, 0f, offset.y);
    }
#endif
}
