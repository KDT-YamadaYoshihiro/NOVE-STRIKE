using UnityEngine;

public class BattleScene : SceneBase
{

    private BattleSystem m_battleSystem;

    [Header("Managers")]
    [SerializeField] private PlayerManager m_playerManager;
    [SerializeField] private EnemyManager m_enemyManager;


    public override void Initialize()
    {
        Debug.Log("BattleScene: 初期化");
        m_battleSystem = new BattleSystem();
        if (m_battleSystem != null)
        {
            m_battleSystem.InitializeSystem(m_playerManager, m_enemyManager);
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
