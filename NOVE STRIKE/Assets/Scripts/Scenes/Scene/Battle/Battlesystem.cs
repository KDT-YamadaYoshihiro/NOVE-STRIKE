/// <summary>
/// バトルシーン全体の進行と実行順序を管理する最上位クラス
/// </summary>
public class BattleSystem
{
    private PlayerManager m_playerManager;
    private EnemyManager m_enemyManager;

    /// <summary>
    /// 初期化
    /// </summary>
    public void InitializeSystem(PlayerManager arg_playerManager, EnemyManager arg_enemyManager)
    {

        m_playerManager = arg_playerManager;
        m_enemyManager = arg_enemyManager;


        // 初期化
        if (m_playerManager != null) 
        {
            m_playerManager.InitializeManager(); 
        
        }
        // エネミーを初期化
        if (m_enemyManager != null) 
        { 
            m_enemyManager.InitializeManager();
        }
    }

    /// <summary>
    /// 基本更新処理
    /// </summary>
    /// <param name="arg_deltaTime"></param>
    public void TickUpdate(float arg_deltaTime)
    {
        // 実行順序の完全制御：プレイヤーの入力処理のあとに、エネミーのAI処理を行う
        if (m_playerManager != null) { m_playerManager.TickUpdate(arg_deltaTime); }
        if (m_enemyManager != null) { m_enemyManager.TickUpdate(arg_deltaTime); }
    }

    /// <summary>
    /// 物理演算用
    /// </summary>
    public void TickFixedUpdate()
    {
        if (m_playerManager != null) { m_playerManager.TickFixedUpdate(); }
        if (m_enemyManager != null) { m_enemyManager.TickFixedUpdate(); }
    }

    /// <summary>
    /// カメラ、事後処理用
    /// </summary>
    /// <param name="arg_deltaTime"></param>
    public void TickLateUpdate(float arg_deltaTime)
    {
        // カメラの追従などはキャラクターの移動が終わったLateUpdateで行う
        if (m_playerManager != null) { m_playerManager.TickLateUpdate(arg_deltaTime); }
    }

    /// <summary>
    /// 
    /// </summary>
    public void TerminateSystem()
    {
        if (m_playerManager != null) { m_playerManager.ReleaseManager(); }
    }
}