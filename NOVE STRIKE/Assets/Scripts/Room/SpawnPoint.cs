using UnityEngine;

/// <summary>
/// 部屋プレハブ内の配置位置を示すマーカー
/// </summary>
/// <remarks>
/// 部屋の見た目と生成ロジックを分離するため、何を配置するかは部屋側では決めず
/// 「ここに何かを置ける」という位置と種別だけを持つ。
/// 実際に何を置くかは生成側が抽選して決める。
/// </remarks>
public class SpawnPoint : MonoBehaviour
{
    [Header("Spawn Point Settings (スポーンポイント設定)")]
    [SerializeField] private SpawnPointType m_type = SpawnPointType.Enemy;

    [Tooltip("抽選の重み。大きいほど選ばれやすい")]
    [SerializeField] private int m_weight = 1;

    [Tooltip("チェックすると、抽選に関わらず必ず使用される")]
    [SerializeField] private bool m_isRequired = false;

    public SpawnPointType Type => m_type;
    public int Weight => Mathf.Max(1, m_weight);
    public bool IsRequired => m_isRequired;

    /// <summary>
    /// 配置に使うワールド座標
    /// </summary>
    public Vector3 Position => transform.position;

    /// <summary>
    /// 配置に使うワールド回転
    /// </summary>
    public Quaternion Rotation => transform.rotation;

#if UNITY_EDITOR
    /// <summary>
    /// 種別ごとに色分けしてシーンビューに表示する
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = GetGizmoColor(m_type);
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawRay(transform.position, transform.forward * 1.5f);
    }

    private static Color GetGizmoColor(SpawnPointType arg_type)
    {
        switch (arg_type)
        {
            case SpawnPointType.Enemy: return Color.red;
            case SpawnPointType.Obstacle: return Color.gray;
            case SpawnPointType.Chest: return Color.yellow;
            case SpawnPointType.PlayerStart: return Color.cyan;
            default: return Color.white;
        }
    }
#endif
}
