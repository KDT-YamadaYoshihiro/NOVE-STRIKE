using UnityEngine;

public class BattleScene : SceneBase
{
    public override void Initialize()
    {
        Debug.Log("BattleScene: ‰Šú‰»");
    }

    public override void OnSceneUpdate()
    {
    }

    public override void Suspend()
    {
    }

    public override void Resume()
    {
    }

    public override void Terminate()
    {
        Debug.Log("BattleScene: I—¹B");
    }
}
