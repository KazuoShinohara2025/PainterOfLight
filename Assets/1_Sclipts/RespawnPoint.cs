using UnityEngine;

public enum SpawnType
{
    Enemy, // 敵用
    Chest  // 宝箱用
}

public class RespawnPoint : MonoBehaviour
{

    [Header("設定")]
    public SpawnType type; // 敵用か宝箱用か
    public GameObject prefabToSpawn; // 出現させるプレハブ（敵 or 宝箱）

    [Header("敵出現時の設定")]
    public float spawnRadius = 2.0f; // 複数体出す場合の散らばり具合
    [Tooltip("敵の最大出現数")]
    public int maxEnemyCount = 3; // 上限設定（デフォルト3）

    private bool hasSpawned = false; // 二重出現防止フラグ


    // --- 宝箱用: 1つだけ生成 ---
    public void SpawnChest()
    {
        if (hasSpawned || prefabToSpawn == null) return;

        Instantiate(prefabToSpawn, transform.position, transform.rotation);
        hasSpawned = true;
    }

    // --- 敵用: 抽選を行い、上限付きで生成 ---
    // ★修正: 確率計算のために playerLevel を引数に追加しました
    public void SpawnEnemies(int count, int playerLevel)
    {
        if (hasSpawned || prefabToSpawn == null) return;

        // 1. 出現抽選 (Playerレベル * 10 %)
        int spawnChance = playerLevel * 10;
        int randomValue = Random.Range(0, 100); // 0～99のランダムな値

        // デバッグ用ログ（確率確認用）
        // Debug.Log($"[{name}] 抽選: Lv{playerLevel} -> 確率{spawnChance}% vs 結果{randomValue}");

        // 抽選に外れた場合（確率より大きい値が出た場合）
        if (randomValue >= spawnChance)
        {
            Debug.Log($"リスポーン地点 {name}: 敵の出現抽選に外れました。(確率: {spawnChance}%)");
            hasSpawned = true; // 出現済みとして処理し、もう呼ばれないようにする
            return;
        }

        // 2. 個数制限 (最大3体まで)
        // 要求された数(count) と 上限(maxEnemyCount) のうち、小さい方を採用する
        int finalCount = Mathf.Min(count, maxEnemyCount);

        Debug.Log($"リスポーン地点 {name}: 抽選当選！ 敵を {finalCount} 体出現させます。(元要求: {count})");

        // 3. 生成ループ
        for (int i = 0; i < finalCount; i++)
        {
            // 同じ位置に重なると物理演算で吹き飛ぶので、少しランダムにずらす
            Vector3 randomOffset = new Vector3(
                Random.Range(-spawnRadius, spawnRadius),
                0,
                Random.Range(-spawnRadius, spawnRadius)
            );

            Vector3 spawnPos = transform.position + randomOffset;

            // 敵を生成
            Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        }

        hasSpawned = true; // 一度出たらもう出ないようにする
    }

    private void OnTriggerEnter(Collider other)
    {
        // 何かが触れたら名前を表示
        // Debug.Log($"[RespawnPoint Debug] '{gameObject.name}' に '{other.gameObject.name}' が接触しました！");
    }
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲


    // Editor上で場所がわかりやすいようにGizmosを表示
    private void OnDrawGizmos()
    {
        Gizmos.color = type == SpawnType.Enemy ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}