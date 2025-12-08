using StarterAssets;
using UnityEngine;
using DG.Tweening; // DOTweenを使うために必要

public class PlayerManaManager : MonoBehaviour
{
    [Header("ScriptableObject Data")]
    public PlayerData characterStatus;

    [Header("ステータス")]
    public float maxMana100 = 100;
    public float currentMana;

    [Header("魔法設定")]
    public GameObject magicSpherePrefab; // 球体のプレハブ

    [Header("アニメーション設定")]
    public float targetScale = 50.0f;    // 最終的な大きさ
    public float expandDuration = 2.0f;  // 何秒かけて広がるか
    public Ease expandEase = Ease.OutQuart; // 広がり方の緩急（ふわっと広がる）

    private StarterAssetsInputs _input;
    private GameObject currentSphere; // 現在展開中のスフィアを管理

    void Start()
    {
        // 同じオブジェクトにアタッチされているStarterAssetsInputsを取得
        _input = GetComponent<StarterAssetsInputs>();

        // マナの初期化（nullチェックを入れています）
        if (characterStatus != null)
        {
            currentMana = characterStatus.maxMana;
        }
        else
        {
            currentMana = maxMana100; // データがない場合のフォールバック
        }
    }

    void Update()
    {
        // New Input System経由のフラグをチェック
        if (_input.lighting)
        {
            // 入力を「消費」する (これをしないと、押しっぱなし判定で毎フレーム呼ばれてしまう)
            _input.lighting = false;

            TryCastMagic();
        }
    }

    void TryCastMagic()
    {
        // データがない場合のエラー回避
        if (characterStatus == null) return;

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

        // すでにスフィアが出ているなら、古いものを削除して作り直す（重複防止）
        if (currentSphere != null)
        {
            // DOTweenが動いている途中の可能性もあるのでKillして削除
            currentSphere.transform.DOKill();
            Destroy(currentSphere);
        }

        Vector3 spawnPos = transform.position;

        // 生成して、Playerの子オブジェクトにする
        currentSphere = Instantiate(magicSpherePrefab, spawnPos, Quaternion.identity, transform);

        // --- DOTweenによるアニメーション処理 ---

        // 1. 初期サイズを0にする（見えなくする）
        currentSphere.transform.localScale = Vector3.zero;

        // 2. 指定した時間（expandDuration）かけて targetScale まで拡大する
        // SetEaseを入れることで、機械的な動きではなく「魔法っぽく」ふわっと広げます
        currentSphere.transform.DOScale(Vector3.one * targetScale, expandDuration)
            .SetEase(expandEase);
    }
}