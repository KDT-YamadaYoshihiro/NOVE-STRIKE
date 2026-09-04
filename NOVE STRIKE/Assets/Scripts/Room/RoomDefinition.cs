using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 部屋プレハブのルートに付け、その部屋の規格と内部のマーカーを提供する
/// </summary>
/// <remarks>
/// 生成側はこのコンポーネント越しにドアとスポーンポイントを取得する。
/// 部屋プレハブの構造(階層やオブジェクト名)を生成側に知らせないための境界となる。
/// </remarks>
public class RoomDefinition : MonoBehaviour
{
    [Header("Room Settings (部屋の規格)")]
    [SerializeField] private RoomType m_roomType = RoomType.Battle;
    [SerializeField] private RoomSize m_roomSize = RoomSize.Size1x1;

    private RoomDoor[] m_doors;
    private SpawnPoint[] m_spawnPoints;

    public RoomType RoomType => m_roomType;
    public RoomSize RoomSize => m_roomSize;

    /// <summary>
    /// 占有するグリッドセル数(X,Z)
    /// </summary>
    public Vector2Int CellCount => RoomGeometry.GetCellCount(m_roomSize);

    private void Awake()
    {
        CacheMarkers();
    }

    /// <summary>
    /// 子オブジェクトのマーカーを収集する
    /// </summary>
    /// <remarks>
    /// 生成直後の1回だけ収集する。実行中に部屋の構造は変化しない前提。
    /// </remarks>
    public void CacheMarkers()
    {
        if (m_doors != null && m_spawnPoints != null) { return; }

        m_doors = GetComponentsInChildren<RoomDoor>(true);
        m_spawnPoints = GetComponentsInChildren<SpawnPoint>(true);
    }

    /// <summary>
    /// この部屋が持つ全ドアを返す
    /// </summary>
    public IReadOnlyList<RoomDoor> GetDoors()
    {
        CacheMarkers();
        return m_doors;
    }

    /// <summary>
    /// 指定方向のドアを返す。無ければ null
    /// </summary>
    public RoomDoor GetDoor(RoomDirection arg_direction)
    {
        CacheMarkers();
        foreach (RoomDoor door in m_doors)
        {
            if (door.Direction == arg_direction) { return door; }
        }
        return null;
    }

    /// <summary>
    /// 指定方向に接続できるかを返す
    /// </summary>
    public bool HasDoor(RoomDirection arg_direction)
    {
        return GetDoor(arg_direction) != null;
    }

    /// <summary>
    /// 指定種別のスポーンポイントを収集して返す
    /// </summary>
    public List<SpawnPoint> GetSpawnPoints(SpawnPointType arg_type)
    {
        CacheMarkers();

        List<SpawnPoint> result = new List<SpawnPoint>();
        foreach (SpawnPoint point in m_spawnPoints)
        {
            if (point.Type == arg_type) { result.Add(point); }
        }
        return result;
    }

#if UNITY_EDITOR
    /// <summary>
    /// インスペクターでの編集時に、部屋の規約違反を検出して警告する
    /// </summary>
    /// <remarks>
    /// 規約違反のまま部屋を量産すると、生成時に繋がらない部屋が混ざり原因の特定が難しくなる。
    /// 作成時点で気づけるよう、ここで検証する。
    /// </remarks>
    private void OnValidate()
    {
        ValidateRoom();
    }

    private void ValidateRoom()
    {
        // ボス部屋は2x2、それ以外は2x2以外という対応を強制する
        bool isBossType = m_roomType == RoomType.Boss;
        bool isBossSize = m_roomSize == RoomSize.Size2x2;
        if (isBossType != isBossSize)
        {
            Debug.LogWarning($"[{name}] 2x2はボス部屋専用です。RoomTypeとRoomSizeの組み合わせを見直してください。(Type={m_roomType}, Size={m_roomSize})", this);
        }

        RoomDoor[] doors = GetComponentsInChildren<RoomDoor>(true);

        // ボス部屋は終端のため入口1つのみとする
        if (isBossType)
        {
            if (doors.Length != 1)
            {
                Debug.LogWarning($"[{name}] ボス部屋のドアは1つにしてください。現在 {doors.Length} 個あります。", this);
            }
            return;
        }

        if (doors.Length == 0)
        {
            Debug.LogWarning($"[{name}] ドアが1つもありません。この部屋はどこにも接続できません。", this);
        }

        HashSet<RoomDirection> used = new HashSet<RoomDirection>();
        foreach (RoomDoor door in doors)
        {
            // 2セル以上の長さを持つ辺はグリッドに整合しないため接続を許可しない
            if (!RoomGeometry.IsDoorAllowed(m_roomSize, door.Direction))
            {
                Debug.LogWarning($"[{name}] {door.Direction} の辺は2セル以上の長さがあるためドアを置けません。({door.name})", door);
            }

            // 同じ方向に複数のドアがあると接続先が一意に定まらない
            if (!used.Add(door.Direction))
            {
                Debug.LogWarning($"[{name}] {door.Direction} にドアが複数あります。1方向につき1つにしてください。({door.name})", door);
            }
        }
    }
#endif
}
