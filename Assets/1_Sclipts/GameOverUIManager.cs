using UnityEngine;
using DG.Tweening; // DOTween
using UnityEngine.UI;

public class GameOverUIManager : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("GameOverPanelにつけたCanvasGroup")]
    public CanvasGroup gameOverCanvasGroup;

    [Header("設定")]
    public float fadeDuration = 3.0f; // 3秒かけて表示

    void Start()
    {
        // 初期化：透明にして非表示
        if (gameOverCanvasGroup != null)
        {
            gameOverCanvasGroup.alpha = 0f;
            gameOverCanvasGroup.gameObject.SetActive(false);
        }
    }

    public void ShowGameOver()
    {
        if (gameOverCanvasGroup == null) return;

        // 1. オブジェクトを有効化
        gameOverCanvasGroup.gameObject.SetActive(true);

        // 2. DOTweenでAlphaを0から1にする
        gameOverCanvasGroup.DOFade(1.0f, fadeDuration)
            .SetEase(Ease.Linear) // 一定の速度で
            .OnComplete(() =>
            {
                // フェード完了後の処理（例：クリックでタイトルへ戻る入力を許可するなど）
                Debug.Log("Game Over表示完了");

                // 必要であればここでカーソルを表示する
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            });
    }
}