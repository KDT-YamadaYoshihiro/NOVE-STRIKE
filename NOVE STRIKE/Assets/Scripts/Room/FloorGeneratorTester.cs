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

    [Tooltip("チェックすると、再生開始時に自動でフロアを生成する")]
    [SerializeField] private bool m_buildOnStart = true;

    private FloorBuilder m_builder;
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

        Debug.Log($"[{name}] フロアを生成しました。本道 {m_layout.MainPath.Count} 部屋 / 全 {m_layout.Rooms.Count} 部屋");
    }

    private void OnDestroy()
    {
        m_builder?.Clear();
    }
}
