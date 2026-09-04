using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// スポーンポイントの中から、使用する点を選ぶ
/// </summary>
/// <remarks>
/// 抽選ロジックだけを切り出した静的クラス。
/// GameObject の生成を伴わないため、生成結果を単体で検証できる。
/// </remarks>
public static class SpawnPointSelector
{
    /// <summary>
    /// 指定数のスポーンポイントを重複なく選ぶ
    /// </summary>
    /// <param name="arg_candidates">候補となるスポーンポイント</param>
    /// <param name="arg_count">選びたい数</param>
    /// <returns>選ばれた点。候補が足りない場合は候補すべて</returns>
    /// <remarks>
    /// IsRequired が立っている点は抽選に関わらず必ず含める。
    /// 残りの枠を重み付き抽選で埋める。
    /// </remarks>
    public static List<SpawnPoint> Select(IReadOnlyList<SpawnPoint> arg_candidates, int arg_count)
    {
        List<SpawnPoint> result = new List<SpawnPoint>();
        if (arg_candidates == null || arg_candidates.Count == 0 || arg_count <= 0) { return result; }

        // 必須の点を先に確保する
        List<SpawnPoint> remaining = new List<SpawnPoint>();
        foreach (SpawnPoint point in arg_candidates)
        {
            if (point == null) { continue; }

            if (point.IsRequired) { result.Add(point); }
            else { remaining.Add(point); }
        }

        // 必須だけで枠を超える場合は、必須をすべて使う
        if (result.Count >= arg_count) { return result; }

        int pickCount = Mathf.Min(arg_count - result.Count, remaining.Count);
        for (int i = 0; i < pickCount; i++)
        {
            int index = PickIndexByWeight(remaining);
            result.Add(remaining[index]);

            // 選んだ点を候補から外し、重複配置を防ぐ
            remaining.RemoveAt(index);
        }

        return result;
    }

    /// <summary>
    /// 重み付き抽選で添字を1つ選ぶ
    /// </summary>
    private static int PickIndexByWeight(List<SpawnPoint> arg_points)
    {
        int totalWeight = 0;
        foreach (SpawnPoint point in arg_points)
        {
            totalWeight += point.Weight;
        }

        int value = Random.Range(0, totalWeight);
        for (int i = 0; i < arg_points.Count; i++)
        {
            value -= arg_points[i].Weight;
            if (value < 0) { return i; }
        }

        // 重みの合計計算と抽選がずれた場合の保険
        return arg_points.Count - 1;
    }
}
