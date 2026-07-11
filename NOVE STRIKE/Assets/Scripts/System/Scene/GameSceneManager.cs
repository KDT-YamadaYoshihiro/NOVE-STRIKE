using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager
{
    private readonly Stack<SceneBase> m_sceneStack = new Stack<SceneBase>();
    private readonly MonoBehaviour m_coroutineRunner;

    public SceneBase m_currentScene => m_sceneStack.Count > 0 ? m_sceneStack.Peek() : null;

    public GameSceneManager(MonoBehaviour coroutineRunner)
    {
        m_coroutineRunner = coroutineRunner;
    }

    /// <summary>
    /// 新しいシーンを型指定で開く
    /// </summary>
    /// <typeparam name="T">開きたいシーンのクラス</typeparam>
    /// <param name="initializer">生成されたシーンに足して、固有の初期化メソッドなどを呼び出すための処理</param>
    public void OpenScene<T>(Action<T> initializer = null) where T : SceneBase,new()
    {
        m_coroutineRunner.StartCoroutine(LoadSceneRoutine(initializer));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="initializer"></param>
    /// <returns></returns>
    private IEnumerator LoadSceneRoutine<T>(Action<T> initializer) where T : SceneBase
    {
        string sceneName = typeof(T).Name;

        // 現在のシーンがあれば一時停止
        if (m_sceneStack.Count > 0)
        {
            m_sceneStack.Peek().Suspend();
        }

        // シーンを加算ロード（既存のシーンの上に重ねる）
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        if (asyncLoad == null)
        {
            Debug.LogError($"[error]シーン　'{sceneName}'のロードに失敗しました。");
            yield break;
        }

        yield return asyncLoad;

        // ロードされたシーンを取得
        Scene loadedScene = SceneManager.GetSceneByName(sceneName);

        if(!loadedScene.IsValid())
        {
            Debug.Log($"[エラー] シーン '{sceneName}' が無効です。");
            yield break;
        }

        T newSceneComponent = null;

        // ロードしたシーンのルートオブジェクトから、目的のコンポーネント(T)を探す
        GameObject[] rootObjects = loadedScene.GetRootGameObjects();
        foreach (GameObject rootObj in loadedScene.GetRootGameObjects())
        {
            newSceneComponent = rootObj.GetComponentInChildren<T>();
            if (newSceneComponent != null) { break; }
        }

        if (newSceneComponent != null)
        {
            newSceneComponent.Setup(this);
            initializer?.Invoke(newSceneComponent);

            m_sceneStack.Push(newSceneComponent);
            newSceneComponent.Initialize();

            Debug.Log($"Scene Opened: {sceneName}");
        }
        else
        {
            Debug.LogError($"{sceneName}.unity の中に {typeof(T).Name} コンポーネントが見つかりません！");
        }
    }

    /// <summary>
    /// 一番上のシーンを閉じる
    /// </summary>
    public void CloseScene()
    {
        if (m_sceneStack.Count == 0) return;

        SceneBase poppedScene = m_sceneStack.Pop();
        poppedScene.Terminate();

        // 対象コンポーネントが所属している物理シーンを取得してアンロード
        Scene unityScene = poppedScene.gameObject.scene;
        SceneManager.UnloadSceneAsync(unityScene);

        Debug.Log($"Scene Closed: {unityScene.name}");

        // 下に隠れていたシーンを再開
        if (m_sceneStack.Count > 0)
        {
            m_sceneStack.Peek().Resume();
        }
    }
    /// <summary>
    /// エントリーポイントから毎フレーム呼び出される更新処理
    /// </summary>
    public void SceneUpdate()
    {
        // 最前面だけ
        m_currentScene?.OnSceneUpdate();
    }

    /// <summary>
    /// 物理演算用更新メソッド
    /// </summary>
    public void SceneFixedUpdate()
    {
        m_currentScene?.OnSceneFixedUpdate();
    }
    
    /// <summary>
    /// カメラ・事後処理用
    /// </summary>
    public void SceneLateUpdate()
    {
        m_currentScene?.OnSceneLateUpdate();
    }
}
