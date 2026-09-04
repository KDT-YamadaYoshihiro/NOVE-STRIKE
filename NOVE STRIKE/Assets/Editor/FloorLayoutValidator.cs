#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// フロアの論理レイアウト生成を、オブジェクトを作らずに検証する
/// </summary>
/// <remarks>
/// 生成は乱数を使うため、1回動いただけでは正しさを確認できない。
/// 多数回まわして、成功率と結果の妥当性(重なり・接続・部屋数)を検証する。
/// </remarks>
public static class FloorLayoutValidator
{
    private const string DefaultFloorPath = "Assets/Data/Room/Floor01.asset";
    private const int TrialCount = 200;

    [MenuItem("Tools/NOVE STRIKE/フロア生成を検証する")]
    public static void Validate()
    {
        FloorData floor = AssetDatabase.LoadAssetAtPath<FloorData>(DefaultFloorPath);
        if (floor == null)
        {
            Debug.LogError($"FloorData が見つかりません: {DefaultFloorPath}");
            return;
        }

        int successCount = 0;
        int failureCount = 0;
        int minRooms = int.MaxValue;
        int maxRooms = 0;
        List<string> problems = new List<string>();

        for (int i = 0; i < TrialCount; i++)
        {
            FloorLayout layout = new FloorLayoutBuilder(floor).Build();
            if (layout == null)
            {
                failureCount++;
                continue;
            }

            successCount++;
            minRooms = Mathf.Min(minRooms, layout.Rooms.Count);
            maxRooms = Mathf.Max(maxRooms, layout.Rooms.Count);

            string problem = Inspect(floor, layout);
            if (!string.IsNullOrEmpty(problem)) { problems.Add($"試行{i}: {problem}"); }
        }

        Debug.Log($"[フロア生成検証] {TrialCount} 回 / 成功 {successCount} / 失敗 {failureCount} / 部屋数 {minRooms}～{maxRooms}");

        if (problems.Count > 0)
        {
            Debug.LogError($"[フロア生成検証] 不正な結果 {problems.Count} 件:\n" + string.Join("\n", problems.GetRange(0, Mathf.Min(10, problems.Count))));
            return;
        }

        if (failureCount > 0)
        {
            Debug.LogWarning($"[フロア生成検証] {failureCount} 回、構築に失敗しました。リトライ回数か部屋構成の見直しを検討してください。");
            return;
        }

        Debug.Log("[フロア生成検証] 問題は見つかりませんでした。");
    }

    /// <summary>
    /// 生成結果を検査する。問題があれば内容を返し、無ければ空文字
    /// </summary>
    private static string Inspect(FloorData arg_floor, FloorLayout arg_layout)
    {
        // 本道の部屋数が指定範囲に収まっているか
        if (arg_layout.MainPath.Count < arg_floor.MinRoomCount || arg_layout.MainPath.Count > arg_floor.MaxRoomCount)
        {
            return $"本道の部屋数が範囲外です ({arg_layout.MainPath.Count})";
        }

        // 末尾がボス部屋か
        if (arg_layout.BossRoom == null || arg_layout.BossRoom.Type != RoomType.Boss)
        {
            return "本道の末尾がボス部屋ではありません";
        }

        // 枝道の部屋がすべて配置されているか
        int expectedBranch = arg_floor.BranchRooms != null ? arg_floor.BranchRooms.Count : 0;
        int actualBranch = arg_layout.Rooms.Count - arg_layout.MainPath.Count;
        if (actualBranch != expectedBranch)
        {
            return $"枝道の部屋数が一致しません (期待 {expectedBranch} / 実際 {actualBranch})";
        }

        // セルの重なりが無いか
        HashSet<Vector2Int> used = new HashSet<Vector2Int>();
        foreach (RoomPlacement room in arg_layout.Rooms)
        {
            foreach (Vector2Int cell in room.EnumerateCells())
            {
                if (!used.Add(cell)) { return $"セル {cell} が重複しています"; }
            }
        }

        // 本道が順に接続されているか
        for (int i = 0; i < arg_layout.MainPath.Count - 1; i++)
        {
            if (!IsConnected(arg_layout.MainPath[i], arg_layout.MainPath[i + 1]))
            {
                return $"本道 {i} と {i + 1} が接続されていません";
            }
        }

        return string.Empty;
    }

    private static bool IsConnected(RoomPlacement arg_a, RoomPlacement arg_b)
    {
        foreach (KeyValuePair<RoomDirection, RoomPlacement> pair in arg_a.Connections)
        {
            if (pair.Value == arg_b) { return true; }
        }
        return false;
    }

    /// <summary>
    /// スポーンポイントの抽選を検証する
    /// </summary>
    /// <remarks>
    /// 実際の部屋プレハブを使い、必須の点が必ず含まれること、
    /// 同じ点が重複して選ばれないこと、要求数を超えないことを確認する。
    /// </remarks>
    [MenuItem("Tools/NOVE STRIKE/スポーン抽選を検証する")]
    public static void ValidateSpawnSelection()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Rooms/Room_Battle_1x1.prefab");
        if (prefab == null)
        {
            Debug.LogError("検証用の部屋プレハブが見つかりません。先に仮部屋を生成してください。");
            return;
        }

        RoomDefinition definition = prefab.GetComponent<RoomDefinition>();
        List<SpawnPoint> points = definition.GetSpawnPoints(SpawnPointType.Enemy);
        if (points.Count == 0)
        {
            Debug.LogError("敵のスポーンポイントがありません。");
            return;
        }

        List<string> problems = new List<string>();
        for (int trial = 0; trial < TrialCount; trial++)
        {
            // 候補数より多い要求も混ぜて、境界の挙動を確認する
            int request = Random.Range(0, points.Count + 3);
            List<SpawnPoint> selected = SpawnPointSelector.Select(points, request);

            if (selected.Count > Mathf.Max(request, CountRequired(points)))
            {
                problems.Add($"要求 {request} に対し {selected.Count} 点が選ばれました");
            }

            HashSet<SpawnPoint> unique = new HashSet<SpawnPoint>(selected);
            if (unique.Count != selected.Count)
            {
                problems.Add($"同じスポーンポイントが重複して選ばれました (要求 {request})");
            }

            foreach (SpawnPoint point in points)
            {
                if (point.IsRequired && !selected.Contains(point) && request > 0)
                {
                    problems.Add($"必須のスポーンポイントが選ばれませんでした (要求 {request})");
                }
            }
        }

        if (problems.Count > 0)
        {
            Debug.LogError($"[スポーン抽選検証] 問題 {problems.Count} 件:\n" + string.Join("\n", problems.GetRange(0, Mathf.Min(10, problems.Count))));
            return;
        }

        Debug.Log($"[スポーン抽選検証] {TrialCount} 回 / 候補 {points.Count} 点 / 問題は見つかりませんでした。");
    }

    private static int CountRequired(List<SpawnPoint> arg_points)
    {
        int count = 0;
        foreach (SpawnPoint point in arg_points)
        {
            if (point.IsRequired) { count++; }
        }
        return count;
    }

    /// <summary>
    /// バッチモードから呼ぶ検証入口。仮部屋の生成から検証までを通しで実行する
    /// </summary>
    public static void RunAll()
    {
        PlaceholderRoomGenerator.Generate();
        Validate();
        ValidateSpawnSelection();
    }
}
#endif
