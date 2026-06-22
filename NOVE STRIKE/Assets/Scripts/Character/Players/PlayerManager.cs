using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private PlayerController m_playerController;

    [Header("Camera Reference")]
    [SerializeField] private TopDownCameraController m_cameraController;

    private List<PlayerBullet> m_activeBullets = new List<PlayerBullet>();
    private PlayerCombatPresenter m_combatPresenter;

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

    private void LateUpdate()
    {
        LateUpdateManager();
    }

    private void OnDestroy()
    {
        ReleaseManager();
    }

    /// <summary>
    /// プレイヤーとカメラの初期化を行う
    /// </summary>
    private void InitializeManager()
    {
        if (m_playerController != null)
        {
            // プレイヤー全体の初期化処理を明示的に実行
            m_playerController.InitializeCharacter();
            m_combatPresenter = m_playerController.GetComponent<PlayerCombatPresenter>();
            // カメラの初期化処理を明示的に実行
            if (m_cameraController != null)
            {
                m_cameraController.InitializeCamera(m_playerController.transform);
            }
        }
        else
        {
            Debug.LogError("PlayerControllerがPlayerManagerにアタッチされていません。");
        }

        if (m_combatPresenter != null)
        {
            m_combatPresenter.InitializeCombat();
            m_combatPresenter.OnBulletSpawned += HandleBulletSpawned;
        }
    }

    private void UpdateManager()
    {

        float deltaTime = Time.deltaTime;

        if (m_playerController != null)
        {
            // プレイヤーのUpdateルーチンを毎フレーム手動呼び出し
            m_playerController.TickUpdate();
        }

        // プレイヤーの戦闘処理のUpdateルーチンを毎フレーム手動呼び出し
        if (m_combatPresenter != null)
        {
            m_combatPresenter.TickUpdate();
        }

        // 弾丸の更新を一括駆動
        for(int i = m_activeBullets.Count - 1; i >= 0; i--)
        {
            PlayerBullet bullet = m_activeBullets[i];
            if (bullet.IsDead)
            {
                m_activeBullets.RemoveAt(i);
                continue;
            }
            
            bullet.TickUpdate(deltaTime);
            
        }
    }

    private void FixedUpdateManager()
    {
        // プレイヤーの物理移動
        if (m_playerController != null)
        {
            m_playerController.TickFixedUpdate();
        }

        // 弾丸のFixedUpdateを一括駆動
        for (int i = m_activeBullets.Count - 1; i >= 0; i--)
        {
            PlayerBullet bullet = m_activeBullets[i];
            if (bullet != null && !bullet.IsDead)
            {
                bullet.TickFixedUpdate();
            }
        }
    }

    private void LateUpdateManager()
    {
        if (m_cameraController != null)
        {
            m_cameraController.LateTickUpdate(Time.deltaTime);
        }
    }

    private void ReleaseManager()
    {
        if (m_playerController != null)
        {
            m_playerController.ReleaseCharacter();
        }

        if (m_combatPresenter != null)
        {
            m_combatPresenter.ReleaseCombat();
            m_combatPresenter.OnBulletSpawned -= HandleBulletSpawned;
        }

        m_activeBullets.Clear();
    }

    /// <summary>
    /// 弾が生成された時に呼び出され、一括管理リストに登録するメソッド
    /// </summary>
    private void HandleBulletSpawned(PlayerBullet arg_newBullet)
    {
        if (arg_newBullet != null)
        {
            m_activeBullets.Add(arg_newBullet);
        }
    }
}