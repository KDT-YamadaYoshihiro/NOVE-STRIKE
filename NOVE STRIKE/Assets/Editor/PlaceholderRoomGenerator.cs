#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 生成ロジックの検証用に、仮モデルの部屋プレハブと関連データを自動生成する
/// </summary>
/// <remarks>
/// 床・壁・ドア・スポーンポイントだけの簡素な部屋を作る。
/// 見た目の作り込み(フェーズ8)ではなく、部屋の規約どおりのプレハブを
/// 確実に量産して生成ロジックを検証することが目的。
/// </remarks>
public static class PlaceholderRoomGenerator
{
    private const string PrefabFolder = "Assets/Prefab/Rooms";
    private const string DataFolder = "Assets/Data/Room";

    private const float WallHeight = 4f;
    private const float WallThickness = 1f;
    private const float DoorWidth = 6f;
    private const float FloorThickness = 0.5f;

    [MenuItem("Tools/NOVE STRIKE/検証用の仮部屋を生成する")]
    public static void Generate()
    {
        EnsureFolder(PrefabFolder);
        EnsureFolder(DataFolder);

        RoomGridSettings gridSettings = CreateOrLoadGridSettings();

        // 戦闘・宝箱・休憩は4方向すべてにドアを持たせる。
        // どの方向から接続されても選べるようにして、生成の失敗を減らすため。
        RoomData battle = CreateRoom(gridSettings, "Room_Battle_1x1", RoomType.Battle, RoomSize.Size1x1, AllDirections(), arg_enemyCount: 6, arg_obstacleCount: 4, arg_chestCount: 0);
        RoomData treasure = CreateRoom(gridSettings, "Room_Treasure_1x1", RoomType.Treasure, RoomSize.Size1x1, AllDirections(), arg_enemyCount: 0, arg_obstacleCount: 2, arg_chestCount: 1);
        RoomData rest = CreateRoom(gridSettings, "Room_Rest_1x1", RoomType.Rest, RoomSize.Size1x1, AllDirections(), arg_enemyCount: 0, arg_obstacleCount: 1, arg_chestCount: 0);

        // ボス部屋は2x2で入口1つ。南から入る想定で固定する
        RoomData boss = CreateRoom(gridSettings, "Room_Boss_2x2", RoomType.Boss, RoomSize.Size2x2,
            new List<RoomDirection> { RoomDirection.South }, arg_enemyCount: 1, arg_obstacleCount: 0, arg_chestCount: 0);

        CreateFloorData(gridSettings, battle, treasure, rest, boss);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"検証用の仮部屋を生成しました。プレハブ: {PrefabFolder} / データ: {DataFolder}");
    }

    // ------------------------------------------------------------
    // データ資産
    // ------------------------------------------------------------

    private static RoomGridSettings CreateOrLoadGridSettings()
    {
        string path = $"{DataFolder}/RoomGridSettings.asset";
        RoomGridSettings settings = AssetDatabase.LoadAssetAtPath<RoomGridSettings>(path);
        if (settings != null) { return settings; }

        settings = ScriptableObject.CreateInstance<RoomGridSettings>();
        AssetDatabase.CreateAsset(settings, path);
        return settings;
    }

    private static void CreateFloorData(RoomGridSettings arg_gridSettings, RoomData arg_battle, RoomData arg_treasure, RoomData arg_rest, RoomData arg_boss)
    {
        string path = $"{DataFolder}/Floor01.asset";
        FloorData floor = AssetDatabase.LoadAssetAtPath<FloorData>(path);
        bool isNew = floor == null;
        if (isNew) { floor = ScriptableObject.CreateInstance<FloorData>(); }

        floor.FloorID = "Floor01";
        floor.MinRoomCount = 8;
        floor.MaxRoomCount = 12;
        floor.GridSettings = arg_gridSettings;
        floor.BattleRooms = new List<RoomData> { arg_battle };
        floor.TreasureRooms = new List<RoomData> { arg_treasure };
        floor.RestRooms = new List<RoomData> { arg_rest };
        floor.BossRoom = arg_boss;

        // 宝箱は中盤、休憩はボス手前に固定する
        floor.BranchRooms = new List<BranchRoomPlacement>
        {
            new BranchRoomPlacement { Type = RoomType.Treasure, MainPathIndex = 3 },
            new BranchRoomPlacement { Type = RoomType.Rest, MainPathIndex = 99 },
        };

        if (isNew) { AssetDatabase.CreateAsset(floor, path); }
        else { EditorUtility.SetDirty(floor); }
    }

    // ------------------------------------------------------------
    // 部屋プレハブ
    // ------------------------------------------------------------

    private static RoomData CreateRoom(RoomGridSettings arg_gridSettings, string arg_name, RoomType arg_type, RoomSize arg_size,
        List<RoomDirection> arg_doorDirections, int arg_enemyCount, int arg_obstacleCount, int arg_chestCount)
    {
        float cellSize = arg_gridSettings.CellSize;
        Vector2Int cells = RoomGeometry.GetCellCount(arg_size);

        GameObject root = new GameObject(arg_name);
        RoomDefinition definition = root.AddComponent<RoomDefinition>();
        SetPrivateField(definition, "m_roomType", arg_type);
        SetPrivateField(definition, "m_roomSize", arg_size);

        // ルートは原点セルの中心に置く。セル(i,j)の中心は root + (i,0,j) * cellSize
        Vector3 extent = new Vector3(cells.x * cellSize, 0f, cells.y * cellSize);
        Vector3 center = new Vector3((cells.x - 1) * cellSize * 0.5f, 0f, (cells.y - 1) * cellSize * 0.5f);

        CreateFloor(root.transform, center, extent);
        CreateWalls(root.transform, center, extent);

        foreach (RoomDirection direction in arg_doorDirections)
        {
            CreateDoor(root.transform, arg_size, direction, cellSize);
        }

        CreateSpawnPoints(root.transform, center, extent, SpawnPointType.Enemy, arg_enemyCount);
        CreateSpawnPoints(root.transform, center, extent, SpawnPointType.Obstacle, arg_obstacleCount);
        CreateSpawnPoints(root.transform, center, extent, SpawnPointType.Chest, arg_chestCount);
        CreateSpawnPoints(root.transform, center, extent, SpawnPointType.PlayerStart, 1);

        string prefabPath = $"{PrefabFolder}/{arg_name}.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);

        return CreateRoomData(arg_name, prefab);
    }

    private static RoomData CreateRoomData(string arg_name, GameObject arg_prefab)
    {
        string path = $"{DataFolder}/{arg_name}_Data.asset";
        RoomData data = AssetDatabase.LoadAssetAtPath<RoomData>(path);
        bool isNew = data == null;
        if (isNew) { data = ScriptableObject.CreateInstance<RoomData>(); }

        data.RoomID = arg_name;
        data.RoomPrefab = arg_prefab;
        data.Weight = 1;

        if (isNew) { AssetDatabase.CreateAsset(data, path); }
        else { EditorUtility.SetDirty(data); }

        return data;
    }

    // ------------------------------------------------------------
    // 部屋の構成要素
    // ------------------------------------------------------------

    private static void CreateFloor(Transform arg_parent, Vector3 arg_center, Vector3 arg_extent)
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.SetParent(arg_parent, false);
        floor.transform.localPosition = arg_center + new Vector3(0f, -FloorThickness * 0.5f, 0f);
        floor.transform.localScale = new Vector3(arg_extent.x, FloorThickness, arg_extent.z);
    }

    /// <summary>
    /// 四方の壁を作る。中央にドア用の隙間を空ける
    /// </summary>
    private static void CreateWalls(Transform arg_parent, Vector3 arg_center, Vector3 arg_extent)
    {
        CreateWallPair(arg_parent, "Wall_North", arg_center + new Vector3(0f, 0f, arg_extent.z * 0.5f), arg_extent.x, true);
        CreateWallPair(arg_parent, "Wall_South", arg_center + new Vector3(0f, 0f, -arg_extent.z * 0.5f), arg_extent.x, true);
        CreateWallPair(arg_parent, "Wall_East", arg_center + new Vector3(arg_extent.x * 0.5f, 0f, 0f), arg_extent.z, false);
        CreateWallPair(arg_parent, "Wall_West", arg_center + new Vector3(-arg_extent.x * 0.5f, 0f, 0f), arg_extent.z, false);
    }

    private static void CreateWallPair(Transform arg_parent, string arg_name, Vector3 arg_center, float arg_length, bool arg_alongX)
    {
        float segmentLength = (arg_length - DoorWidth) * 0.5f;
        if (segmentLength <= 0f) { return; }

        float offset = (DoorWidth + segmentLength) * 0.5f;
        for (int i = 0; i < 2; i++)
        {
            float sign = i == 0 ? -1f : 1f;
            Vector3 localOffset = arg_alongX
                ? new Vector3(sign * offset, 0f, 0f)
                : new Vector3(0f, 0f, sign * offset);

            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = $"{arg_name}_{i}";
            wall.transform.SetParent(arg_parent, false);
            wall.transform.localPosition = arg_center + localOffset + new Vector3(0f, WallHeight * 0.5f, 0f);
            wall.transform.localScale = arg_alongX
                ? new Vector3(segmentLength, WallHeight, WallThickness)
                : new Vector3(WallThickness, WallHeight, segmentLength);
        }
    }

    /// <summary>
    /// ドアのマーカーを、接続に使うセルの辺中央へ置く
    /// </summary>
    private static void CreateDoor(Transform arg_parent, RoomSize arg_size, RoomDirection arg_direction, float arg_cellSize)
    {
        Vector2Int cellOffset = GetDoorCellOffset(arg_size, arg_direction);

        // セル中心 + 方向 * セル半分 が、そのセルの辺の中央になる
        Vector2Int gridOffset = RoomGeometry.ToGridOffset(arg_direction);
        Vector3 cellCenter = new Vector3(cellOffset.x * arg_cellSize, 0f, cellOffset.y * arg_cellSize);
        Vector3 position = cellCenter + new Vector3(gridOffset.x, 0f, gridOffset.y) * (arg_cellSize * 0.5f);

        GameObject door = new GameObject($"Door_{arg_direction}");
        door.transform.SetParent(arg_parent, false);
        door.transform.localPosition = position;

        RoomDoor component = door.AddComponent<RoomDoor>();
        SetPrivateField(component, "m_direction", arg_direction);
        SetPrivateField(component, "m_cellOffset", cellOffset);
    }

    /// <summary>
    /// ドアを置くセルを決める
    /// </summary>
    /// <remarks>
    /// 1セル幅の辺なら候補は1つに定まる。
    /// ボス部屋(2x2)は規約の対象外で、原点セル側に固定する。
    /// </remarks>
    private static Vector2Int GetDoorCellOffset(RoomSize arg_size, RoomDirection arg_direction)
    {
        Vector2Int cells = RoomGeometry.GetCellCount(arg_size);

        int x = 0;
        int y = 0;
        if (arg_direction == RoomDirection.North) { y = cells.y - 1; }
        if (arg_direction == RoomDirection.East) { x = cells.x - 1; }

        return new Vector2Int(x, y);
    }

    private static void CreateSpawnPoints(Transform arg_parent, Vector3 arg_center, Vector3 arg_extent, SpawnPointType arg_type, int arg_count)
    {
        // 部屋の内側に均等に散らす。壁際は避ける
        float marginX = arg_extent.x * 0.3f;
        float marginZ = arg_extent.z * 0.3f;

        for (int i = 0; i < arg_count; i++)
        {
            float t = arg_count == 1 ? 0.5f : (float)i / (arg_count - 1);
            float angle = t * Mathf.PI * 2f;

            Vector3 offset = arg_type == SpawnPointType.PlayerStart
                ? Vector3.zero
                : new Vector3(Mathf.Cos(angle) * marginX, 0f, Mathf.Sin(angle) * marginZ);

            GameObject point = new GameObject($"Spawn_{arg_type}_{i}");
            point.transform.SetParent(arg_parent, false);
            point.transform.localPosition = arg_center + offset;

            SpawnPoint component = point.AddComponent<SpawnPoint>();
            SetPrivateField(component, "m_type", arg_type);
        }
    }

    // ------------------------------------------------------------
    // 補助
    // ------------------------------------------------------------

    private static List<RoomDirection> AllDirections()
    {
        return new List<RoomDirection>
        {
            RoomDirection.North,
            RoomDirection.East,
            RoomDirection.South,
            RoomDirection.West,
        };
    }

    /// <summary>
    /// SerializeField な private フィールドへ値を設定する
    /// </summary>
    /// <remarks>
    /// カプセル化を保つためコンポーネント側に setter を作らず、
    /// エディタ専用のこの生成器からのみ SerializedObject 経由で書き込む。
    /// </remarks>
    private static void SetPrivateField(Object arg_target, string arg_fieldName, object arg_value)
    {
        SerializedObject so = new SerializedObject(arg_target);
        SerializedProperty property = so.FindProperty(arg_fieldName);
        if (property == null)
        {
            Debug.LogError($"フィールド {arg_fieldName} が見つかりません。({arg_target.GetType().Name})");
            return;
        }

        // enum は基になる整数値へ変換して書き込む。本プロジェクトの enum は
        // すべて 0 からの連番のため、enumValueIndex と値が一致する
        if (arg_value is System.Enum) { property.enumValueIndex = System.Convert.ToInt32(arg_value); }
        else if (arg_value is Vector2Int vector) { property.vector2IntValue = vector; }
        else if (arg_value is int number) { property.intValue = number; }
        else { Debug.LogError($"未対応の型です: {arg_value.GetType().Name}"); }

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void EnsureFolder(string arg_path)
    {
        if (AssetDatabase.IsValidFolder(arg_path)) { return; }

        string parent = Path.GetDirectoryName(arg_path).Replace('\\', '/');
        string leaf = Path.GetFileName(arg_path);

        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, leaf);
    }
}
#endif
