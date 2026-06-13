using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private PlayerController m_playerController;

    // Unity標準ライフサイクルの使用箇所をこのクラスに限定化する

    private void Awake()
    {
        InitializeManager();
    }

    private void Update()
    {
        UpdateManager();
    }

    private void FixedUpdate()
    {
        FixedUpdateManager();
    }

    private void OnDestroy()
    {
        ReleaseManager();
    }

    private void InitializeManager()
    {
        if (m_playerController != null)
        {
            // プレイヤー全体の初期化処理を明示的に実行
            m_playerController.InitializeCharacter();
        }
        else
        {
            Debug.LogError("PlayerControllerがPlayerManagerにアタッチされていません。");
        }
    }

    private void UpdateManager()
    {
        if (m_playerController != null)
        {
            // プレイヤーのUpdateルーチンを毎フレーム手動呼び出し
            m_playerController.TickUpdate();
        }
    }

    private void FixedUpdateManager()
    {
        if (m_playerController != null)
        {
            // プレイヤーのFixedUpdate（物理演算）ルーチンを手動呼び出し
            m_playerController.TickFixedUpdate();
        }
    }

    private void ReleaseManager()
    {
        if (m_playerController != null)
        {
            // プレイヤーの解放・クリーンアップ処理を明示的に実行
            m_playerController.ReleaseCharacter();
        }
    }
}