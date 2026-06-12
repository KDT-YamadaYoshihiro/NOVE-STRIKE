using UnityEngine;

public class AppMain : MonoBehaviour
{
    public static GameSceneManager SceneManager { get; private set; }

    private void Awake()
    {
        // 二重生成防止
        if (SceneManager != null)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        SceneManager = new GameSceneManager(this);
    }

    private void Start()
    {
        // 起動時、最初にタイトルシーンを開く
        SceneManager.OpenScene<TitleScene>();
    }

    private void Update()
    {
        SceneManager?.Update();
    }
}
