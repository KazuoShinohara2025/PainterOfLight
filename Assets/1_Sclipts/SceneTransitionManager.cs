using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening; // DOTween
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager instance;

    [Header("UI References")]
    [SerializeField] private RectTransform overlayRect; // 黒い幕のRectTransform
    [SerializeField] private CanvasGroup canvasGroup;   // Raycastブロック用

    [Header("Settings")]
    [SerializeField] private float duration = 2.0f; // 2秒かけて移動

    private float screenWidth;

    private void Awake()
    {
        // シングルトン化（シーン遷移しても破壊されないようにする）
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 画面の幅を取得（移動量計算用）
        // CanvasScalerの影響を受けるため、RectTransformのサイズを取得するのが確実
        screenWidth = overlayRect.rect.width;
        if (screenWidth == 0) screenWidth = 1920f; // 安全策

        // 初期状態：黒幕を「画面の右外」に配置しておく
        overlayRect.anchoredPosition = new Vector2(screenWidth, 0);

        // 操作可能にしておく
        if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// 外部からこのメソッドを呼ぶと遷移が始まる
    /// </summary>
    public void LoadScene(string sceneName)
    {
        StartCoroutine(TransitionSequence(sceneName));
    }

    private IEnumerator TransitionSequence(string sceneName)
    {
        // 1. 操作をブロックする
        if (canvasGroup != null) canvasGroup.blocksRaycasts = true;

        // 画面幅を再取得（解像度変更対策）
        float width = overlayRect.rect.width;

        // --- 暗転アニメーション (右 -> 中央) ---
        // まず右外に配置
        overlayRect.anchoredPosition = new Vector2(width, 0);

        // 中央(0)へ移動
        yield return overlayRect.DOAnchorPosX(0, duration)
            .SetEase(Ease.OutExpo) // ゆっくり止まるような動き（好みでLinearなどに変更可）
            .WaitForCompletion();

        // --- シーンロード ---
        // 非同期でロードを開始
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false; // 読み込み完了まで待機

        // ロードが90%進むまで待つ
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        // ロード完了を許可
        asyncLoad.allowSceneActivation = true;

        // シーンが完全に切り替わるのを少し待つ
        yield return new WaitForSeconds(0.1f);

        // --- 明転アニメーション (中央 -> 左) ---
        // 左外(-width)へ移動
        yield return overlayRect.DOAnchorPosX(-width, duration)
            .SetEase(Ease.OutExpo)
            .WaitForCompletion();

        // --- 終了処理 ---
        // 次回のために右外に戻しておく
        overlayRect.anchoredPosition = new Vector2(width, 0);

        // 操作ブロック解除
        if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
    }
}