// TopDownCameraController.cs
using UnityEngine;

public class TopDownCameraController : MonoBehaviour
{
    [Header("Camera Target Settings")]
    [SerializeField] private Transform m_targetPlayer;

    [Header("Position Settings")]
    // プレイヤーからどれくらい離れた上空にカメラを配置するか（デフォルト値: 真上近くから斜めに見下ろす設定）
    [SerializeField] private Vector3 m_cameraOffset = new Vector3(0f, 15f, -10f);

    // カメラの追従の滑らかさ（値が小さいほどキビキビ動き、大きいほど遅れて滑らかに追従します）
    [SerializeField] private float m_smoothTime = 0.2f;

    private Vector3 m_currentVelocity;

    /// <summary>
    /// カメラの初期設定（PlayerManagerから明示的に呼び出される）
    /// </summary>
    /// <param name="arg_playerTransform">追従対象のプレイヤーのTransform</param>
    public void InitializeCamera(Transform arg_playerTransform)
    {
        m_targetPlayer = arg_playerTransform;

        if (m_targetPlayer != null)
        {
            // ゲーム開始時にカメラを理想の俯瞰位置へ瞬時に配置
            transform.position = m_targetPlayer.position + m_cameraOffset;

            // プレイヤーのいる座標をカチッと見下ろす角度に回転をロック
            transform.LookAt(m_targetPlayer.position);
        }
    }

    /// <summary>
    /// カメラの手動更新ルーチン（PlayerManagerのLateUpdateManagerから駆動）
    /// </summary>
    /// <param name="arg_deltaTime">ゲームの経過時間</param>
    public void LateTickUpdate(float arg_deltaTime)
    {
        if (m_targetPlayer == null) return;

        // プレイヤーの現在地から逆算したカメラの目標座標を計算
        Vector3 targetPosition = m_targetPlayer.position + m_cameraOffset;

        // 滑らかに目標座標へ補間移動（キャラクターの移動によるガタつきを吸収）
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref m_currentVelocity,
            m_smoothTime,
            Mathf.Infinity,
            arg_deltaTime
        );
    }
}