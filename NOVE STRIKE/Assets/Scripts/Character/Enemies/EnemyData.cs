using UnityEngine;

public enum EnemyMoveType { Stationary, Chase, Kamikaze }
public enum EnemyAttackType { SingleShot, BurstShot, SpreadShot, Laser, Explode, BossCombo }

/// <summary>
/// 1種類のエネミーのステータスや行動パターンを定義するデータテーブル
/// </summary>
[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Basic Info")]
    public string EnemyID;           // 生成時に指定する一意のID
    public GameObject BasePrefab;    // 3Dモデルやコライダーを含むベースプレハブ

    [Header("Status")]
    public float MaxHealth = 100f;
    public float MoveSpeed = 3f;
    public float AttackPower = 10f;
    public float AttackRange = 15f;
    public float AttackCooldown = 2f;

    [Header("AI Behaviors")]
    public EnemyMoveType MoveType;     // どう動くか
    public EnemyAttackType AttackType; // どう攻撃するか

    [Header("Weapon References")]
    public GameObject BulletPrefab;    // 攻撃に使う弾のプレハブ
}