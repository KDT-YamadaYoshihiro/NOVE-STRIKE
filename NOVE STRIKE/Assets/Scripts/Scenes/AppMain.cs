using UnityEngine;

public class AppMain : MonoBehaviour
{
    public static GameSceneManager m_sceneManager { get; private set; }

    private void Awake()
    {
        // 二重生成防止
        if (m_sceneManager != null)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        m_sceneManager = new GameSceneManager(this);
    }

    private void Start()
    {
        // 起動時、最初にタイトルシーンを開く
        m_sceneManager.OpenScene<TitleScene>();
    }

    private void Update()
    {
        m_sceneManager?.SceneUpdate();
    }

    private void FixedUpdate()
    {
        m_sceneManager?.SceneFixedUpdate();
    }

    private void LateUpdate()
    {
        m_sceneManager?.SceneLateUpdate();
    }
}
