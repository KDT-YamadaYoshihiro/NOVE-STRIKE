using UnityEngine;

public abstract class SceneBase : MonoBehaviour
{
    // シーンマネージャーへの参照を各シーンが持てるようにする
    protected GameSceneManager Manager { get; private set; }

    public void Setup(GameSceneManager manager)
    {
        Manager = manager;
    }

    public abstract void Initialize();
    public abstract void Suspend();
    public abstract void Resume();
    public abstract void Terminate();

    // 毎フレームの更新処理
    public abstract void OnSceneUpdate();
    public abstract void OnSceneFixedUpdate();
    public abstract void OnSceneLateUpdate();

}