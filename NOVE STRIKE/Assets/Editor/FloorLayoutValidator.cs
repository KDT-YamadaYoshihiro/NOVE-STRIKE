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
    /// バッチモードから呼ぶ検証入口。仮部屋の生成から検証までを通しで実行する
    /// </summary>
    public static void RunAll()
    {
        PlaceholderRoomGenerator.Generate();
        Validate();
    }
}
#endif
