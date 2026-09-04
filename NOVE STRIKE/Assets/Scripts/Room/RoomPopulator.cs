using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 部屋のスポーンポイントに、敵・障害物・宝箱を抽選配置する
/// </summary>
/// <remarks>
/// 部屋プレハブは「ここに何かを置ける」という位置しか持たない。
/// 何をいくつ置くかはこのクラスが決める。
/// 全部屋を同時に配置する方式のため、配置はプレイヤーの入室時に行う。
/// </remarks>
public class RoomPopulator
{
    private readonly FloorData m_floorData;
    private readonly EnemyManager m_enemyManager;

    public RoomPopulator(FloorData arg_floorData, EnemyManager arg_enemyManager)
    {
        m_floorData = arg_floorData;
        m_enemyManager = arg_enemyManager;
    }

    /// <summary>
    /// 部屋の中身を配置する。配置済みの場合は何もしない
    /// </summary>
    /// <returns>配置を行ったら true</returns>
    public bool Populate(RoomRuntime arg_room)
    {
        if (arg_room == null || arg_room.IsPopulated) { return false; }

        RoomData data = arg_room.Placement.Data;
        RoomDefinition definition = arg_room.Definition;
        if (data == null || definition == null)
        {
            Debug.LogError("部屋データまたは部屋の実体がありません。");
            return false;
        }

        PlaceObstacles(definition, data);
        PlaceChests(definition);

        List<EnemyController> enemies = PlaceEnemies(definition, data);
        arg_room.MarkPopulated(enemies);
        return true;
    }

    // ------------------------------------------------------------
    // 敵
    // ------------------------------------------------------------

    /// <summary>
    /// 敵を抽選して配置する
    /// </summary>
    private List<EnemyController> PlaceEnemies(RoomDefinition arg_definition, RoomData arg_data)
    {
        List<EnemyController> spawned = new List<EnemyController>();

        if (m_enemyManager == null) { return spawned; }

        int count = RandomCount(arg_data.MinEnemyCount, arg_data.MaxEnemyCount);
        if (count <= 0) { return spawned; }

        List<SpawnPoint> points = arg_definition.GetSpawnPoints(SpawnPointType.Enemy);
        List<SpawnPoint> selected = SpawnPointSelector.Select(points, count);

        foreach (SpawnPoint point in selected)
        {
            string enemyId = PickEnemyId();
            if (string.IsNullOrEmpty(enemyId)) { continue; }

            EnemyController enemy = m_enemyManager.SpawnEnemy(enemyId, point.Position);
            if (enemy != null) { spawned.Add(enemy); }
        }

        return spawned;
    }

    /// <summary>
    /// フロアの出現テーブルから敵IDを重み付き抽選する
    /// </summary>
    private string PickEnemyId()
    {
        List<EnemySpawnEntry> table = m_floorData != null ? m_floorData.EnemyTable : null;
        if (table == null || table.Count == 0)
        {
            Debug.LogError($"[{(m_floorData != null ? m_floorData.name : "null")}] 敵の出現テーブルが空です。");
            return null;
        }

        int totalWeight = 0;
        foreach (EnemySpawnEntry entry in table)
        {
            if (entry == null) { continue; }
            totalWeight += Mathf.Max(1, entry.Weight);
        }

        if (totalWeight <= 0) { return null; }

        int value = Random.Range(0, totalWeight);
        foreach (EnemySpawnEntry entry in table)
        {
            if (entry == null) { continue; }

            value -= Mathf.Max(1, entry.Weight);
            if (value < 0) { return entry.EnemyID; }
        }

        // 重みの合計計算と抽選がずれた場合の保険
        return table[table.Count - 1].EnemyID;
    }

    // ------------------------------------------------------------
    // 障害物・宝箱
    // ------------------------------------------------------------

    /// <summary>
    /// 障害物を抽選して配置する
    /// </summary>
    private void PlaceObstacles(RoomDefinition arg_definition, RoomData arg_data)
    {
        List<GameObject> prefabs = m_floorData != null ? m_floorData.ObstaclePrefabs : null;
        if (prefabs == null || prefabs.Count == 0) { return; }

        int count = RandomCount(arg_data.MinObstacleCount, arg_data.MaxObstacleCount);
        if (count <= 0) { return; }

        List<SpawnPoint> points = arg_definition.GetSpawnPoints(SpawnPointType.Obstacle);
        foreach (SpawnPoint point in SpawnPointSelector.Select(points, count))
        {
            GameObject prefab = prefabs[Random.Range(0, prefabs.Count)];
            if (prefab == null) { continue; }

            Instantiate(prefab, point, arg_definition.transform);
        }
    }

    /// <summary>
    /// 宝箱を配置する
    /// </summary>
    /// <remarks>
    /// 宝箱は数を抽選せず、宝箱用のスポーンポイントすべてに置く。
    /// 宝箱部屋の価値を運に左右させないため。
    /// </remarks>
    private void PlaceChests(RoomDefinition arg_definition)
    {
        GameObject prefab = m_floorData != null ? m_floorData.ChestPrefab : null;
        if (prefab == null) { return; }

        foreach (SpawnPoint point in arg_definition.GetSpawnPoints(SpawnPointType.Chest))
        {
            Instantiate(prefab, point, arg_definition.transform);
        }
    }

    // ------------------------------------------------------------
    // 補助
    // ------------------------------------------------------------

    private static void Instantiate(GameObject arg_prefab, SpawnPoint arg_point, Transform arg_parent)
    {
        Object.Instantiate(arg_prefab, arg_point.Position, arg_point.Rotation, arg_parent);
    }

    /// <summary>
    /// 下限と上限から個数を決める。逆転していても破綻しないようにする
    /// </summary>
    private static int RandomCount(int arg_min, int arg_max)
    {
        int min = Mathf.Max(0, Mathf.Min(arg_min, arg_max));
        int max = Mathf.Max(0, Mathf.Max(arg_min, arg_max));
        return Random.Range(min, max + 1);
    }
}
