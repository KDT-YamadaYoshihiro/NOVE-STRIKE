using UnityEngine;

/// <summary>
/// 1種類の部屋プレハブを抽選対象として定義するデータ
/// </summary>
[CreateAssetMenu(fileName = "NewRoomData", menuName = "Game/Room Data")]
public class RoomData : ScriptableObject
{
    [Header("Basic Info (基本情報)")]
    public string RoomID;
    public GameObject RoomPrefab;

    [Header("Selection (抽選)")]
    [Tooltip("抽選の重み。大きいほど選ばれやすい")] public int Weight = 1;

    [Header("Contents (内容量)")]
    [Tooltip("配置する敵の数の下限")] public int MinEnemyCount = 3;
    [Tooltip("配置する敵の数の上限")] public int MaxEnemyCount = 6;
    [Tooltip("配置する障害物の数の下限")] public int MinObstacleCount = 0;
    [Tooltip("配置する障害物の数の上限")] public int MaxObstacleCount = 3;

    /// <summary>
    /// プレハブから部屋の規格を取得する。取得できない場合は null
    /// </summary>
    public RoomDefinition GetDefinition()
    {
        if (RoomPrefab == null) { return null; }
        return RoomPrefab.GetComponent<RoomDefinition>();
    }
}
