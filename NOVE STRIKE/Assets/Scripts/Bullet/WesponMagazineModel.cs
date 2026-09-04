using System;
using UnityEngine;

public class WeaponMagazineModel
{
    // 各種設定値（不変）
    private readonly int m_burstShotCount;      // 1セットの連射数（例: 10発）
    private readonly float m_burstCoolTime;     // セット間の短いCT（例: 1.0s）
    private readonly int m_maxBurstSets;        // リロードまでに撃てるセット数（例: 5回）
    private readonly float m_reloadTime;        // リロードにかかる時間
    private readonly float m_shotInterval;      // 1発ごとの発射間隔（連射速度）

    // 現在の状態管理変数
    private int m_currentShotsInBurst;
    private int m_currentBurstSets;
    private float m_nextActionTime;
    private bool m_isReloading;
    private bool m_isInBurstCoolTime;

    // 状態を外部（PresenterやUI）に伝えるイベント
    public event Action OnReloadStarted;
    public event Action OnReloadFinished;
    public event Action OnBurstCoolTimeStarted;

    public bool CanShoot => !m_isReloading && !m_isInBurstCoolTime && Time.time >= m_nextActionTime;
    public float ShotInterval => m_shotInterval;

    public WeaponMagazineModel(
        int arg_burstShotCount,
        float arg_burstCoolTime,
        int arg_maxBurstSets,
        float arg_reloadTime,
        float arg_shotInterval)
    {
        m_burstShotCount = arg_burstShotCount;
        m_burstCoolTime = arg_burstCoolTime;
        m_maxBurstSets = arg_maxBurstSets;
        m_reloadTime = arg_reloadTime;
        m_shotInterval = arg_shotInterval;

        ResetMagazine();
    }

    public void ResetMagazine()
    {
        m_currentShotsInBurst = 0;
        m_currentBurstSets = 0;
        m_isReloading = false;
        m_isInBurstCoolTime = false;
        m_nextActionTime = 0f;
    }

    /// <summary>
    /// 管理クラス等のUpdateから毎秒呼ばれ、リロードやバーストCTのタイマーを監視するメソッド
    /// </summary>
    public void TickUpdate()
    {
        if (Time.time < m_nextActionTime) return;

        if (m_isReloading)
        {
            m_isReloading = false;
            ResetMagazine();
            OnReloadFinished?.Invoke();
        }
        else if (m_isInBurstCoolTime)
        {
            m_isInBurstCoolTime = false;
            m_currentShotsInBurst = 0;
        }
    }

    /// <summary>
    /// 1発発射した際のカウント進捗ロジック
    /// </summary>
    public void RecordShot()
    {
        m_currentShotsInBurst++;
        m_nextActionTime = Time.time + m_shotInterval;

        // 1セットの連射（バースト）を撃ち切ったか
        if (m_currentShotsInBurst >= m_burstShotCount)
        {
            m_currentBurstSets++;

            // 全セットを撃ち切ったのでリロードへ
            if (m_currentBurstSets >= m_maxBurstSets)
            {
                TriggerReload();
            }
            else
            {
                // セット間の短いクールタイムへ
                TriggerBurstCoolTime();
            }
        }
    }

    private void TriggerBurstCoolTime()
    {
        m_isInBurstCoolTime = true;
        m_nextActionTime = Time.time + m_burstCoolTime;
        OnBurstCoolTimeStarted?.Invoke();
    }

    public void TriggerReload()
    {
        if (m_isReloading) return;

        m_isReloading = true;
        m_nextActionTime = Time.time + m_reloadTime;
        OnReloadStarted?.Invoke();
    }
}