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

    private bool hasSpawned = false; // 二重出現防止フラグ


    // --- 宝箱用: 1つだけ生成 ---
    public void SpawnChest()
    {
        if (hasSpawned || prefabToSpawn == null) return;

        Instantiate(prefabToSpawn, transform.position, transform.rotation);
        hasSpawned = true;
    }

    // --- 敵用: レベルに応じた数を生成 ---
    public void SpawnEnemies(int count)
    {
        if (hasSpawned || prefabToSpawn == null) return;

        Debug.Log($"リスポーン地点 {name}: 敵を {count} 体出現させます。");

        for (int i = 0; i < count; i++)
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
        Debug.Log($"[RespawnPoint Debug] '{gameObject.name}' に '{other.gameObject.name}' が接触しました！");
    }
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲


    // Editor上で場所がわかりやすいようにGizmosを表示
    private void OnDrawGizmos()
    {
        Gizmos.color = type == SpawnType.Enemy ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}