#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// エディタ上でPlayボタンを押した際、自動的にRootSceneからゲームを開始させるスクリプト
/// </summary>
[InitializeOnLoad]
public static class AutoLoadRootScene
{
    static AutoLoadRootScene()
    {
        // エディタの起動時や、スクリプトコンパイル時にフックを登録
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // Playボタンが押された瞬間（ゲームが実際に始まる直前）
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            // Build Settingsに登録されている「最初（インデックス0）のシーン」のパスを取得
            // ※RootSceneがBuild Settingsの最上部にある前提です
            if (EditorBuildSettings.scenes.Length > 0)
            {
                string firstScenePath = EditorBuildSettings.scenes[0].path;
                SceneAsset rootSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(firstScenePath);

                if (rootSceneAsset != null)
                {
                    // 再生時に強制的に読み込む「プレイ開始シーン」として設定する
                    EditorSceneManager.playModeStartScene = rootSceneAsset;
                    return;
                }
            }

            Debug.LogWarning("[AutoLoadRootScene] Build Settingsにシーンが登録されていないため、自動ロードを設定できませんでした。");
        }

        // 再生が終了してエディタモードに戻った時
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            // 設定を解除して、元々編集していたシーンの状態に戻す
            EditorSceneManager.playModeStartScene = null;
        }
    }
}
#endif