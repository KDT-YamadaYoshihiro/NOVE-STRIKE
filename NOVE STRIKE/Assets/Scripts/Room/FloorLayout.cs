using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// フロア上に配置される1部屋分の情報
/// </summary>
/// <remarks>
/// GameObject を持たない論理データ。実際の配置は FloorBuilder が行う。
/// 生成の判断と実配置を分けることで、配置結果を検証しやすくする。
/// </remarks>
public class RoomPlacement
{
    /// <summary>使用する部屋データ</summary>
    public RoomData Data { get; private set; }

    /// <summary>部屋の種別</summary>
    public RoomType Type { get; private set; }

    /// <summary>部屋のサイズ規格</summary>
    public RoomSize Size { get; private set; }

    /// <summary>部屋の左下セルのグリッド座標</summary>
    public Vector2Int OriginCell { get; private set; }

    /// <summary>本道上の順番。枝道の部屋は -1</summary>
    public int MainPathIndex { get; private set; }

    /// <summary>接続先。key は自分から見た方向</summary>
    public Dictionary<RoomDirection, RoomPlacement> Connections { get; private set; }
        = new Dictionary<RoomDirection, RoomPlacement>();

    public RoomPlacement(RoomData arg_data, RoomType arg_type, RoomSize arg_size, Vector2Int arg_originCell, int arg_mainPathIndex)
    {
        Data = arg_data;
        Type = arg_type;
        Size = arg_size;
        OriginCell = arg_originCell;
        MainPathIndex = arg_mainPathIndex;
    }

    /// <summary>
    /// この部屋が占有するグリッドセルを列挙する
    /// </summary>
    public IEnumerable<Vector2Int> EnumerateCells()
    {
        Vector2Int count = RoomGeometry.GetCellCount(Size);
        for (int x = 0; x < count.x; x++)
        {
            for (int y = 0; y < count.y; y++)
            {
                yield return OriginCell + new Vector2Int(x, y);
            }
        }
    }

    /// <summary>
    /// 双方向に接続を登録する
    /// </summary>
    public void Connect(RoomDirection arg_direction, RoomPlacement arg_other)
    {
        Connections[arg_direction] = arg_other;
        arg_other.Connections[RoomGeometry.GetOpposite(arg_direction)] = this;
    }
}

/// <summary>
/// 1フロア分の配置結果
/// </summary>
public class FloorLayout
{
    /// <summary>配置された全部屋</summary>
    public List<RoomPlacement> Rooms { get; private set; } = new List<RoomPlacement>();

    /// <summary>本道の部屋。先頭が開始部屋、末尾がボス部屋</summary>
    public List<RoomPlacement> MainPath { get; private set; } = new List<RoomPlacement>();

    /// <summary>開始部屋</summary>
    public RoomPlacement StartRoom => MainPath.Count > 0 ? MainPath[0] : null;

    /// <summary>ボス部屋</summary>
    public RoomPlacement BossRoom => MainPath.Count > 0 ? MainPath[MainPath.Count - 1] : null;

    /// <summary>
    /// 部屋を登録する。本道の部屋は MainPath にも積む
    /// </summary>
    public void AddRoom(RoomPlacement arg_room, bool arg_isMainPath)
    {
        Rooms.Add(arg_room);
        if (arg_isMainPath) { MainPath.Add(arg_room); }
    }
}
