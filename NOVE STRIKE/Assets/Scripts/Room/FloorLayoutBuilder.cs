using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// フロアの論理レイアウトを構築する
/// </summary>
/// <remarks>
/// GameObject を一切生成せず、グリッド座標と接続だけを決める。
/// 実際の配置は FloorBuilder が行う。判断と配置を分けることで、
/// 生成結果の妥当性をオブジェクトを作らずに検証できる。
/// </remarks>
public class FloorLayoutBuilder
{
    private readonly FloorData m_floorData;

    // グリッドセルの占有状況。key はセル座標
    private readonly Dictionary<Vector2Int, RoomPlacement> m_occupied = new Dictionary<Vector2Int, RoomPlacement>();

    public FloorLayoutBuilder(FloorData arg_floorData)
    {
        m_floorData = arg_floorData;
    }

    /// <summary>
    /// レイアウトを構築する。失敗した場合は null
    /// </summary>
    /// <remarks>
    /// ランダムウォークは行き止まりに入り込むことがあるため、
    /// 失敗したら最初からやり直す。試行回数は FloorData で指定する。
    /// </remarks>
    public FloorLayout Build()
    {
        if (m_floorData == null)
        {
            Debug.LogError("FloorData が指定されていません。");
            return null;
        }

        int retryLimit = Mathf.Max(1, m_floorData.MaxBuildRetryCount);
        for (int attempt = 0; attempt < retryLimit; attempt++)
        {
            m_occupied.Clear();

            FloorLayout layout = TryBuild();
            if (layout != null) { return layout; }
        }

        Debug.LogError($"[{m_floorData.name}] {retryLimit} 回試行しましたがフロアを構築できませんでした。部屋数や部屋プレハブのドア構成を見直してください。");
        return null;
    }

    /// <summary>
    /// 1回分の構築を試みる。失敗したら null
    /// </summary>
    private FloorLayout TryBuild()
    {
        FloorLayout layout = new FloorLayout();

        int mainPathCount = Random.Range(m_floorData.MinRoomCount, m_floorData.MaxRoomCount + 1);
        if (!BuildMainPath(layout, mainPathCount)) { return null; }
        if (!BuildBranchRooms(layout)) { return null; }

        return layout;
    }

    // ------------------------------------------------------------
    // 本道
    // ------------------------------------------------------------

    /// <summary>
    /// 本道を構築する。末尾がボス部屋になる
    /// </summary>
    private bool BuildMainPath(FloorLayout arg_layout, int arg_roomCount)
    {
        // 開始部屋。次の部屋へ向かう方向は配置後に決まるため、ここでは方向を要求しない
        RoomPlacement previous = CreateRoom(RoomType.Battle, Vector2Int.zero, 0, null);
        if (previous == null) { return false; }

        Occupy(previous);
        arg_layout.AddRoom(previous, true);

        // 中間の戦闘部屋。最後の1部屋はボス部屋にするため除く
        int battleRoomCount = arg_roomCount - 1;
        for (int i = 1; i < battleRoomCount; i++)
        {
            RoomPlacement next = ExtendPath(previous, RoomType.Battle, i);
            if (next == null) { return false; }

            arg_layout.AddRoom(next, true);
            previous = next;
        }

        // ボス部屋
        RoomPlacement boss = ExtendPath(previous, RoomType.Boss, arg_roomCount - 1);
        if (boss == null) { return false; }

        arg_layout.AddRoom(boss, true);
        return true;
    }

    /// <summary>
    /// 指定部屋から1部屋伸ばす。置ける場所が無ければ null
    /// </summary>
    private RoomPlacement ExtendPath(RoomPlacement arg_from, RoomType arg_type, int arg_mainPathIndex)
    {
        List<RoomDirection> directions = GetShuffledDirections();

        foreach (RoomDirection direction in directions)
        {
            // 伸ばす方向のドアが元の部屋に無ければ繋げない
            if (!CanExtendTowards(arg_from, direction)) { continue; }

            RoomPlacement placed = TryPlaceNeighbor(arg_from, direction, arg_type, arg_mainPathIndex);
            if (placed != null) { return placed; }
        }

        // すべての方向が塞がっている(行き止まり)
        return null;
    }

    // ------------------------------------------------------------
    // 枝道
    // ------------------------------------------------------------

    /// <summary>
    /// 本道から行き止まりの部屋を生やす
    /// </summary>
    private bool BuildBranchRooms(FloorLayout arg_layout)
    {
        if (m_floorData.BranchRooms == null) { return true; }

        foreach (BranchRoomPlacement branch in m_floorData.BranchRooms)
        {
            if (branch == null) { continue; }

            RoomPlacement from = ResolveBranchOrigin(arg_layout, branch.MainPathIndex);
            if (from == null) { return false; }

            RoomPlacement placed = ExtendPath(from, branch.Type, -1);
            if (placed == null) { return false; }

            arg_layout.AddRoom(placed, false);
        }

        return true;
    }

    /// <summary>
    /// 枝道の分岐元となる本道の部屋を決める
    /// </summary>
    /// <remarks>
    /// 指定が本道の範囲を超える場合は末尾側へ丸める。
    /// ボス部屋からは枝を生やさない。終端でありドアが1つしかないため。
    /// </remarks>
    private RoomPlacement ResolveBranchOrigin(FloorLayout arg_layout, int arg_mainPathIndex)
    {
        // ボス部屋(末尾)を除いた範囲に丸める
        int lastSelectable = arg_layout.MainPath.Count - 2;
        if (lastSelectable < 0)
        {
            Debug.LogError($"[{m_floorData.name}] 枝道を生やせる本道の部屋がありません。");
            return null;
        }

        int index = Mathf.Clamp(arg_mainPathIndex, 0, lastSelectable);
        return arg_layout.MainPath[index];
    }

    // ------------------------------------------------------------
    // 配置
    // ------------------------------------------------------------

    /// <summary>
    /// 指定方向の隣に部屋を置く。置けなければ null
    /// </summary>
    private RoomPlacement TryPlaceNeighbor(RoomPlacement arg_from, RoomDirection arg_direction, RoomType arg_type, int arg_mainPathIndex)
    {
        RoomDirection entryDirection = RoomGeometry.GetOpposite(arg_direction);

        // 新しい部屋には、来た方向へのドアが必ず必要になる
        List<RoomDirection> required = new List<RoomDirection> { entryDirection };
        RoomData data = m_floorData.PickRoom(arg_type, required);
        if (data == null) { return null; }

        RoomDefinition definition = data.GetDefinition();
        if (definition == null) { return null; }

        RoomDoor entryDoor = definition.GetDoor(entryDirection);
        if (entryDoor == null) { return null; }

        // 接続元のドアが属するセルの隣が、接続先のドアが属するセルになる。
        // そこから、接続先の部屋の原点セルを逆算する。
        Vector2Int fromDoorCell = GetDoorCell(arg_from, arg_direction);
        Vector2Int targetDoorCell = fromDoorCell + RoomGeometry.ToGridOffset(arg_direction);
        Vector2Int origin = targetDoorCell - entryDoor.CellOffset;

        RoomPlacement placement = new RoomPlacement(data, arg_type, definition.RoomSize, origin, arg_mainPathIndex);
        if (!CanPlace(placement)) { return null; }

        Occupy(placement);
        arg_from.Connect(arg_direction, placement);
        return placement;
    }

    /// <summary>
    /// 部屋を新規に作る。接続を伴わない開始部屋用
    /// </summary>
    private RoomPlacement CreateRoom(RoomType arg_type, Vector2Int arg_origin, int arg_mainPathIndex, IReadOnlyList<RoomDirection> arg_required)
    {
        RoomData data = m_floorData.PickRoom(arg_type, arg_required);
        if (data == null) { return null; }

        RoomDefinition definition = data.GetDefinition();
        if (definition == null) { return null; }

        return new RoomPlacement(data, arg_type, definition.RoomSize, arg_origin, arg_mainPathIndex);
    }

    /// <summary>
    /// 指定方向のドアが属するセルのグリッド座標を返す
    /// </summary>
    private static Vector2Int GetDoorCell(RoomPlacement arg_room, RoomDirection arg_direction)
    {
        RoomDefinition definition = arg_room.Data.GetDefinition();
        RoomDoor door = definition != null ? definition.GetDoor(arg_direction) : null;

        Vector2Int offset = door != null ? door.CellOffset : Vector2Int.zero;
        return arg_room.OriginCell + offset;
    }

    /// <summary>
    /// その方向へ部屋を伸ばせるかを返す
    /// </summary>
    private static bool CanExtendTowards(RoomPlacement arg_room, RoomDirection arg_direction)
    {
        // 既に使っている方向へは伸ばせない
        if (arg_room.Connections.ContainsKey(arg_direction)) { return false; }

        RoomDefinition definition = arg_room.Data.GetDefinition();
        return definition != null && definition.HasDoor(arg_direction);
    }

    /// <summary>
    /// 占有セルが他の部屋と重ならないかを判定する
    /// </summary>
    private bool CanPlace(RoomPlacement arg_room)
    {
        foreach (Vector2Int cell in arg_room.EnumerateCells())
        {
            if (m_occupied.ContainsKey(cell)) { return false; }
        }
        return true;
    }

    /// <summary>
    /// 占有セルを登録する
    /// </summary>
    private void Occupy(RoomPlacement arg_room)
    {
        foreach (Vector2Int cell in arg_room.EnumerateCells())
        {
            m_occupied[cell] = arg_room;
        }
    }

    /// <summary>
    /// 4方向をシャッフルして返す
    /// </summary>
    private static List<RoomDirection> GetShuffledDirections()
    {
        List<RoomDirection> directions = new List<RoomDirection>
        {
            RoomDirection.North,
            RoomDirection.East,
            RoomDirection.South,
            RoomDirection.West,
        };

        for (int i = directions.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            RoomDirection temp = directions[i];
            directions[i] = directions[j];
            directions[j] = temp;
        }
        return directions;
    }
}
