using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class TitleScene : SceneBase
{
    private GameObject m_uiInstance;
    private InputAction m_changeScene;


    public override void Initialize()
    {
        Debug.Log("TitleScene: 初期化");

        m_changeScene = InputSystem.actions.FindAction("Next");

        // Resourcesフォルダからプレハブをロードして生成
        // 名前はエディタで作ったプレハブ名に合わせてください
        GameObject prefab = Resources. Load<GameObject>("TitleUI");
        if (prefab != null)
        {
            m_uiInstance = Object.Instantiate(prefab);
        }
        else
        {
            Debug.LogError("TitleUIプレハブがResourcesフォルダに見つかりません。");
        }
    }

    public override void OnSceneUpdate()
    {

        // デバッグ用：スペースキーが押されたらBattleSceneへ遷移する
        if(Keyboard.current.spaceKey.isPressed)
        {
            Debug.Log("スペースキーが押されました。BattleSceneへ遷移します。");

            // タイトルシーンを終了して閉じる
            Manager.CloseScene();

            // 新しくバトルシーンを開く
            Manager.OpenScene<BattleScene>();

        }
    }

    public override void OnSceneFixedUpdate()
    { 
    }

    public override void OnSceneLateUpdate()
    { 
    }

    public override void Suspend()
    {
        // タイトルの上に何か重なる場合、UIを非活性にするなどの処理
        if (m_uiInstance != null) { m_uiInstance.SetActive(false); }
    }

    public override void Resume()
    {
        if (m_uiInstance != null) { m_uiInstance.SetActive(true); }
    }

    public override void Terminate()
    {
        Debug.Log("TitleScene: 終了。オブジェクトを破棄します。");

        // 生成したUIをシーンから削除
        if (m_uiInstance != null)
        {
            Object.Destroy(m_uiInstance);
        }
    }
}
