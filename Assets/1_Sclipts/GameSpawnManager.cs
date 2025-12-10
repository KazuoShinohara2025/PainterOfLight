using UnityEngine;
using System.Collections.Generic;
using System.Linq; // リスト操作用

public class GameSpawnManager : MonoBehaviour
{
    [Header("設定")]
    public int chestCount = 3; // 配置する宝箱の数

    void Start()
    {
        SpawnRandomChests();
    }

    void SpawnRandomChests()
    {
        // 1. シーン上のすべての RespawnPoint を探す
        RespawnPoint[] allPoints = FindObjectsOfType<RespawnPoint>();

        // 2. その中から「宝箱用 (Chest)」かつ「プレハブが設定されているもの」だけをリストアップ
        List<RespawnPoint> chestPoints = allPoints
            .Where(p => p.type == SpawnType.Chest && p.prefabToSpawn != null)
            .ToList();

        // 3. ポイントが足りない場合のエラー回避
        if (chestPoints.Count < chestCount)
        {
            Debug.LogWarning("宝箱用のリスポーン地点が足りません！");
            chestCount = chestPoints.Count;
        }

        // 4. リストをシャッフル（ランダムに並び替え）
        // GUIDを使った簡易シャッフル
        chestPoints = chestPoints.OrderBy(x => System.Guid.NewGuid()).ToList();

        // 5. 先頭から指定個数分だけSpawnChestを実行
        for (int i = 0; i < chestCount; i++)
        {
            chestPoints[i].SpawnChest();
        }

        Debug.Log($"{chestCount} 個の宝箱を配置しました。");
    }
}