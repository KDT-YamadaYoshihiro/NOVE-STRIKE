using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyManager : MonoBehaviour
{
    [Header("Database Reference")]
    [Tooltip("作成したEnemyDatabaseをアタッチします")]
    [SerializeField] private EnemyDatabase m_enemyDatabase;

    [Header("Container References")]
    [Tooltip("エネミーが撃った弾をまとめる空オブジェクト")]
    [SerializeField] private Transform m_enemyBulletContainer;
    [Tooltip("生成したエネミー本体をまとめる空オブジェクト（ヒエラルキー整理用）")]
    [SerializeField] private Transform m_enemyContainer;

    [Header("Target Reference")]
    [Tooltip("エネミーが狙うターゲット（プレイヤーのTransform）")]
    [SerializeField] private Transform m_playerTarget;

    private EnemyFactory m_factory;
    private List<EnemyController> m_activeEnemies = new List<EnemyController>();


    /// <summary>
    /// マネージャーとファクトリーの初期化
    /// </summary>
    public void InitializeManager()
    {
        if(m_enemyDatabase == null)
        {
            Debug.LogError("EnemyDatabase がアタッチされていません");
            return;
        }

        m_factory = new EnemyFactory(m_enemyDatabase, m_enemyBulletContainer);
    }

    /// <summary>
    /// 生きている全エネミーのUpdateを一括駆動
    /// </summary>
    public void TickUpdate(float arg_deltaTime)
    {
        // 【デバッグ用】必要に応じて残すか削除
        if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            SpawnEnemy("mob", new Vector3(5f, 0f, 5f));
        }

        // リストを後ろからループ
        for (int i = m_activeEnemies.Count -1; i >= 0; i--)
        {
            EnemyController enemy = m_activeEnemies[i];

            // エネミーが存在しない
            if(enemy == null)
            {
                m_activeEnemies.RemoveAt(i);
                continue;
            }
            enemy.TickUpdate(arg_deltaTime);
        }
    }

    /// <summary>
    /// 生きている全エネミーのfixeUpdateを一括駆動
    /// </summary>
    public void TickFixedUpdate()
    {
        for (int i = m_activeEnemies.Count - 1; i >= 0; i--)
        {
            EnemyController enemy = m_activeEnemies[i];
            if(enemy != null)
            {
                enemy.TickFixedUpdate();
            }
        }
    }

    /// <summary>
    /// 外部から呼ばれるエネミー生成メソッド
    /// </summary>
    /// <param name="arg_enemyId"></param>
    /// <param name="arg_spawnPosition"></param>
    /// <returns></returns>
    public EnemyController SpawnEnemy(string arg_enemyId, Vector3 arg_spawnPosition)
    {
        if (m_factory == null) { return null; }

        // ファクトリーに依頼
        EnemyController newEnemy = m_factory.CreateEnemy(arg_enemyId, arg_spawnPosition, Quaternion.identity);

        if (newEnemy != null)
        {
            // 指令があればコンテナの子オブジェクトにする
            if (m_enemyContainer != null)
            {
                newEnemy.transform.SetParent(m_enemyContainer);
            }

            // プレイヤーをターゲット
            if(m_playerTarget != null)
            {
                newEnemy.SetTarget(m_playerTarget);
            }

            // マネジャーの稼働リストに追加
            m_activeEnemies.Add(newEnemy);
        }

        return newEnemy;
    }

    /// <summary>
    /// ターゲットを動的に設定・変更メソッド
    /// </summary>
    /// <param name="arg_target"></param>
    public void SetPlayerTarget(Transform arg_target)
    {
        m_playerTarget = arg_target;

        foreach(var enemy in m_activeEnemies)
        {
            if(enemy != null)
            {
                enemy.SetTarget(m_playerTarget);
            }
        }
    }
}
