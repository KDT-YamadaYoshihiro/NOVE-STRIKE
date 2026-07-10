using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ゲーム内の全エネミーデータを一括管理するデータベース
/// </summary>
[CreateAssetMenu(fileName = "EnemyDatabase", menuName = "Game/Enemy Database")]
public class EnemyDatabase : ScriptableObject
{
    public List<EnemyData> EnemyList = new List<EnemyData>();

    /// <summary>
    /// IDに一致するエネミーデータを検索して返す
    /// </summary>
    public EnemyData GetEnemyData(string arg_id)
    {
        EnemyData data = EnemyList.Find(x => x.EnemyID == arg_id);
        if (data == null)
        {
            Debug.LogError($"Enemy ID '{arg_id}' がデータベースに見つかりません。");
        }
        return data;
    }
}