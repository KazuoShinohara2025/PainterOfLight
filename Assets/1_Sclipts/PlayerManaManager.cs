using UnityEngine;

public class PlayerManaManager : MonoBehaviour
{
    [Header("ScriptableObject Data")]
    public PlayerData characterStatus;

    [Header("ステータス")]
    public float maxMana100 = 100;
    public float currentMana;
    public float manaCost5 = 5;

    [Header("魔法設定")]
    public GameObject magicSpherePrefab; // 球体のプレハブ
    public float spawnDistance = 2.0f;   // プレイヤーの前方どれくらいに出すか

    void Start()
    {
        currentMana = characterStatus.maxMana;
    }

    void Update()
    {
        // 右クリック (0=左, 1=右, 2=中)
        if (Input.GetMouseButtonDown(1))
        {
            TryCastMagic();
        }
    }

    void TryCastMagic()
    {
        if (currentMana >= characterStatus.lightingCost)
        {
            SpawnSphere();
            currentMana -= characterStatus.lightingCost;
            Debug.Log($"魔法発動！ 残りマナ: {currentMana}");
        }
        else
        {
            Debug.Log("マナが足りません！");
        }
    }

    void SpawnSphere()
    {
        if (magicSpherePrefab == null) return;

        // プレイヤーの位置（あるいは前方）に球体を生成
        // マウスの位置に出したい場合は Raycast を使用しますが、今回はシンプルにプレイヤー位置とします
        Vector3 spawnPos = transform.position;

        // プレハブを生成
        Instantiate(magicSpherePrefab, spawnPos, Quaternion.identity);
    }
}