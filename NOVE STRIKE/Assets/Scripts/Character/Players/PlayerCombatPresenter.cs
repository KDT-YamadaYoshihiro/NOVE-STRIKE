using UnityEngine;

public class PlayerCombatPresenter : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private GameObject m_bulletPrefab;
    [SerializeField] private Transform m_firePoint;

    private WeaponMagazineModel m_magazineModel;
    private bool m_isReloadTriggered;

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
        if (m_magazineModel == null) return;

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
        if (m_magazineModel == null) return;

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
        if (m_bulletPrefab == null || m_firePoint == null) return;

        // 3D空間上に弾丸をインスタンス化
        GameObject bulletObject = Instantiate(m_bulletPrefab, m_firePoint.position, m_firePoint.rotation);

        // 弾丸からPlayerBulletコンポーネントを取得
        PlayerBullet playerBullet = bulletObject.GetComponent<PlayerBullet>();
        if (playerBullet != null)
        {
            // 将来的にPlayerStatusの攻撃力などを反映できるように設計
            // ここでは仮の値（威力10, 速度20, 寿命5秒）でデータを作成
            BulletData bulletData = new BulletData(10f, 20f, 5f);

            // 3D空間の「前方向（firePoint.forward）」をベクトルとして弾丸に渡して初期化
            playerBullet.InitializeBullet(bulletData, m_firePoint.forward);
        }
    }

    /// <summary>
    /// リロード開始
    /// </summary>
    private void HandleReloadStarted()
    {
        Debug.Log("リロード開始（Presenter経由でアニメーションやUIへ通知可能）");
        // 将来的に、ここでプレイヤーの移動速度を「リロード減速（デバフ）」させるロジックなどを、
        // PlayerStatusを介して安全に行う拡張が可能です。
    }

    /// <summary>
    /// リロード完了
    /// </summary>
    private void HandleReloadFinished()
    {
        Debug.Log("リロード完了");
    }
}
