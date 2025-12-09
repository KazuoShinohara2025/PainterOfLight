using UnityEngine;
using DG.Tweening;

public class PlayerManaManager : MonoBehaviour
{
    // ★マナ管理や入力処理は削除しました

    [Header("魔法設定")]
    public GameObject magicSpherePrefab;

    [Header("アニメーション設定")]
    public float targetScale = 50.0f;
    public float expandDuration = 2.0f;
    public Ease expandEase = Ease.OutQuart;

    private GameObject currentSphere;

    // CharacterCombatController から呼ばれる関数
    public void SpawnSphereVisual()
    {
        if (magicSpherePrefab == null) return;

        // すでにスフィアが出ているなら削除して作り直す
        if (currentSphere != null)
        {
            currentSphere.transform.DOKill();
            Destroy(currentSphere);
        }

        Vector3 spawnPos = transform.position;

        // 生成して、Playerの子オブジェクトにする
        currentSphere = Instantiate(magicSpherePrefab, spawnPos, Quaternion.identity, transform);

        // DOTween処理
        currentSphere.transform.localScale = Vector3.zero;
        currentSphere.transform.DOScale(Vector3.one * targetScale, expandDuration)
            .SetEase(expandEase);

        // 演出が終わったら自動で消えるようにする（必要であれば）
        // Destroy(currentSphere, expandDuration + 1.0f);
    }
}