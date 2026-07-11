using UnityEngine;

public class BattleScene : SceneBase
{

    [Header("Systems")]
    [Tooltip("このシーンのメインシステム")]
    [SerializeField] private BattleSystem m_battleSystem;

    public override void Initialize()
    {
        Debug.Log("BattleScene: 初期化");

        if (m_battleSystem != null)
        {
            m_battleSystem.InitializeSystem();
        }
    }

    public override void OnSceneUpdate()
    {
        // ポーズ中などでなければSystemを駆動
        if (m_battleSystem != null)
        {
            m_battleSystem.TickUpdate(Time.deltaTime);
        }
    }

    public override void OnSceneFixedUpdate()
    {
        if (m_battleSystem != null)
        {
            m_battleSystem.TickFixedUpdate();
        }
    }

    public override void OnSceneLateUpdate()
    {
        if (m_battleSystem != null)
        {
            m_battleSystem.TickLateUpdate(Time.deltaTime);
        }
    }
    public override void Suspend()
    {
        Debug.Log("BattleScene: 一時停止（ポーズ画面などが乗った時）");
    }

    public override void Resume()
    {
        Debug.Log("BattleScene: 再開");
    }

    public override void Terminate()
    {
        Debug.Log("BattleScene: 終了。");

        if (m_battleSystem != null)
        {
            m_battleSystem.TerminateSystem();
        }
    }
}
