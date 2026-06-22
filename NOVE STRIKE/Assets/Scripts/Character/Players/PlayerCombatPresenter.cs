using System;
using UnityEngine;

public class PlayerCombatPresenter : MonoBehaviour
{
    [Header("Weapon Settings")]
    // 弾プレハブ
    [SerializeField] private GameObject m_bulletPrefab;
    // 発射位置
    [SerializeField] private Transform m_firePoint;
    // 弾の親オブジェクト
    [SerializeField] private Transform m_bulletContainer;

    private WeaponMagazineModel m_magazineModel;
    private bool m_isReloadTriggered;

    public event Action<PlayerBullet> OnBulletSpawned;

    public WeaponMagazineModel Magazin => m_magazineModel;

    /// <summary>
    /// 初期化
    /// </summary>
    public void InitializeCombat()
    {
        // 引数: 連射数10, バーストCT1.0s, 最大5セット, リロード3.0s, 発射間隔0.1s
        m_magazineModel = new WeaponMagazineModel(10, 1.0f, 5, 3.0f, 0.1f);
        // Modelからのイベントを購読（Viewの制御やSE・エフェクト再生への拡張用）
        m_magazineModel.OnReloadStarted += HandleReloadStarted;
        m_magazineModel.OnReloadFinished += HandleReloadFinished;
    }

    /// <summary>
    /// 解放メソッド
    /// </summary>
    public void ReleaseCombat()
    {
        if (m_magazineModel != null)
        {
            m_magazineModel.OnReloadStarted -= HandleReloadStarted;
            m_magazineModel.OnReloadFinished -= HandleReloadFinished;
        }
    }

    /// <summary>
    /// 更新ルーチン
    /// </summary>
    public void TickUpdate()
    {
        if (m_magazineModel == null) { return; }

        // Model内部のタイマー更新（リロード時間やバーストCTの計測）
        m_magazineModel.TickUpdate();

        // 手動リロード入力があった場合の処理
        if (m_isReloadTriggered)
        {
            m_magazineModel.TriggerReload();
            m_isReloadTriggered = false; // フラグの消費
        }
    }

    /// <summary>
    /// 手動リロード要求受付
    /// </summary>
    public void RequestReload()
    {
        m_isReloadTriggered = true;
    }

    /// <summary>
    /// 射撃入力
    /// </summary>
    public void OnShootInputPressed()
    {
        if (m_magazineModel == null) { return; }

        // Modelが「現在撃てる状態か（リロード中やCT中でないか）」を判断
        if (m_magazineModel.CanShoot)
        {
            // 弾丸の物理的な生成処理の実行
            ExecuteSpawnBullet();
            // 1発撃ったことをModelに記録（Model側でカウントや状態遷移が行われる）
            m_magazineModel.RecordShot();
        }
    }

    /// <summary>
    /// 弾丸の生成
    /// </summary>
    private void ExecuteSpawnBullet()
    {
        if (m_bulletPrefab == null || m_firePoint == null) { return; }

        // 生成する弾丸オブジェクト
        GameObject bulletObject;
        // コンテナ（親オブジェクト）が指定されている場合は、その子として生成する
        if (m_bulletContainer != null)
        {
            // 第4引数に m_bulletContainer を指定することで、自動的にBattleScene側へ生成されます
            bulletObject = Instantiate(m_bulletPrefab, m_firePoint.position, m_firePoint.rotation, m_bulletContainer);
        }
        else
        {
            // 万が一インスペクターで未設定だった場合のフォールバック（バックアップ処理）
            Debug.LogWarning("m_bulletContainer が未設定です。プレイヤーと同じシーンに直接生成します。");
            bulletObject = Instantiate(m_bulletPrefab, m_firePoint.position, m_firePoint.rotation);
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(bulletObject, gameObject.scene);
        }

        PlayerBullet playerBullet = bulletObject.GetComponent<PlayerBullet>();
        if (playerBullet != null)
        {
            // 発射瞬間の前方方向ベクトルを取得
            Vector3 shootDirection = m_firePoint.forward;
            // 水平方向のみで発射する場合はY軸を0にする
            shootDirection.y = 0f;
            // 正規化して単位ベクトルにする
            shootDirection.Normalize();

            BulletData bulletData = new BulletData(10f, 20f, 5f, BulletOwnerType.Player, shootDirection);
            // データを安全に注入
            playerBullet.InitializeBullet(bulletData);
            // 管理クラスに通知
            OnBulletSpawned?.Invoke(playerBullet);
        }
        else
        {
            Destroy(bulletObject);
        }
    }

    /// <summary>
    /// リロード開始
    /// </summary>
    private void HandleReloadStarted()
    {
        Debug.Log("リロード開始（Presenter経由でアニメーションやUIへ通知可能）");
    }

    /// <summary>
    /// リロード完了
    /// </summary>
    private void HandleReloadFinished()
    {
        Debug.Log("リロード完了");
    }
}
