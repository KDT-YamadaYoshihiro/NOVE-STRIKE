using UnityEngine;

/// <summary>
/// 1種類のエネミーのステータスや行動パターンを定義するデータテーブル
/// </summary>
[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Basic Info (基本情報)")]
    public string EnemyID;
    public GameObject BasePrefab;

    [Header("Status (ステータス)")]
    [Tooltip("体力")] public float MaxHealth = 100f;
    [Tooltip("移動速度（0に設定すると移動しなくなります）")] public float MoveSpeed = 3f;
    [Tooltip("攻撃力")] public float AttackPower = 10f;
    [Tooltip("防御力")] public float DefensePower = 0f;
    [Tooltip("攻撃を行う距離（自爆の場合はコライダーの半径程度）")] public float AttackRange = 15f;
    [Tooltip("攻撃のクールダウン（間隔）")] public float AttackCooldown = 2f;

    [Header("Attack Behaviors (攻撃の設定)")]
    [Tooltip("チェックを入れると、近づいて自爆（接触ダメージを与えて自身は消滅）します")]
    public bool IsKamikaze = false;

    [Tooltip("弾のプレハブをセットすると、遠距離攻撃（弾の発射）を行います")]
    public GameObject BulletPrefab;
}