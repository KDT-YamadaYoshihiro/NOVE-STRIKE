using UnityEngine;

public class BattleScene : SceneBase
{
    public override void Initialize()
    {
        Debug.Log("BattleScene: 初期化");
    }

    public override void OnSceneUpdate()
    {
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
    }
}
