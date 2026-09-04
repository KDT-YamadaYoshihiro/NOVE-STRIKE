using UnityEngine;

/// <summary>
/// フロア生成の動作確認用コンポーネント
/// </summary>
/// <remarks>
/// バトルシーンへの組み込み(フェーズ1-4)までの暫定的な確認手段。
/// シーンに置いて再生すると、指定した FloorData でフロアを1つ生成する。
/// </remarks>
public class FloorGeneratorTester : MonoBehaviour
{
    [Header("Floor (フロア設定)")]
    [SerializeField] private FloorData m_floorData;

    [Tooltip("敵の生成を担当するマネージャー。未設定の場合、敵は配置されない")]
    [SerializeField] private EnemyManager m_enemyManager;

    [Tooltip("チェックすると、再生開始時に自動でフロアを生成する")]
    [SerializeField] private bool m_buildOnStart = true;

    [Tooltip("チェックすると、生成直後に開始部屋の中身を配置する")]
    [SerializeField] private bool m_populateStartRoom = true;

    private FloorBuilder m_builder;
    private RoomPopulator m_populator;
    private FloorLayout m_layout;

    /// <summary>生成されたレイアウト。未生成の場合は null</summary>
    public FloorLayout Layout => m_layout;

    private void Start()
    {
        if (m_buildOnStart) { BuildFloor(); }
    }

    /// <summary>
    /// フロアを生成する
    /// </summary>
    [ContextMenu("フロアを生成する")]
    public void BuildFloor()
    {
        if (m_floorData == null)
        {
            Debug.LogError($"[{name}] FloorData が設定されていません。", this);
            return;
        }

        if (m_floorData.GridSettings == null)
        {
            Debug.LogError($"[{name}] FloorData に RoomGridSettings が設定されていません。", this);
            return;
        }

        m_layout = new FloorLayoutBuilder(m_floorData).Build();
        if (m_layout == null) { return; }

        m_builder = m_builder ?? new FloorBuilder(m_floorData.GridSettings, transform);
        if (!m_builder.Build(m_layout))
        {
            Debug.LogError($"[{name}] フロアの配置に失敗しました。", this);
            return;
        }

        if (m_enemyManager != null) { m_enemyManager.InitializeManager(); }
        m_populator = new RoomPopulator(m_floorData, m_enemyManager);

        Debug.Log($"[{name}] フロアを生成しました。本道 {m_layout.MainPath.Count} 部屋 / 全 {m_layout.Rooms.Count} 部屋");

        if (m_populateStartRoom) { EnterRoom(m_layout.StartRoom); }
    }

    /// <summary>
    /// 部屋に入ったときの処理。まだ中身が無ければ配置する
    /// </summary>
    /// <remarks>
    /// 入室の検知そのものはフェーズ1-4で実装する。
    /// ここは配置を呼び出す入口だけを用意している。
    /// </remarks>
    public void EnterRoom(RoomPlacement arg_placement)
    {
        if (arg_placement == null || m_builder == null || m_populator == null) { return; }

        if (!m_builder.SpawnedRooms.TryGetValue(arg_placement, out RoomRuntime room))
        {
            Debug.LogError($"[{name}] 配置されていない部屋に入室しようとしました。", this);
            return;
        }

        if (!m_populator.Populate(room)) { return; }

        Debug.Log($"[{name}] 部屋の中身を配置しました。({room.Placement.Type} / 敵 {room.Enemies.Count} 体)");
    }

    /// <summary>
    /// すべての部屋の中身を配置する。検証用
    /// </summary>
    [ContextMenu("全部屋の中身を配置する")]
    public void PopulateAllRooms()
    {
        if (m_layout == null || m_populator == null)
        {
            Debug.LogError($"[{name}] 先にフロアを生成してください。", this);
            return;
        }

        int enemyTotal = 0;
        foreach (RoomPlacement placement in m_layout.Rooms)
        {
            if (!m_builder.SpawnedRooms.TryGetValue(placement, out RoomRuntime room)) { continue; }

            m_populator.Populate(room);
            enemyTotal += room.Enemies.Count;
        }

        Debug.Log($"[{name}] 全 {m_layout.Rooms.Count} 部屋に配置しました。敵の合計 {enemyTotal} 体");
    }

    private void OnDestroy()
    {
        m_builder?.Clear();
    }
}
