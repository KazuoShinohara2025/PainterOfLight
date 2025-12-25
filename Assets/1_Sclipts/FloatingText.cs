using UnityEngine;
using TMPro;
using DG.Tweening;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMesh;
    [SerializeField] private float moveDistance = 2.0f; // 上昇する距離
    [SerializeField] private float duration = 1.5f;     // 消えるまでの時間

    public void Setup(string text, Color color)
    {
        // もしInspectorでの設定を忘れていても、自動で探す
        if (textMesh == null) textMesh = GetComponent<TextMeshPro>();

        // それでも見つからない場合の安全策
        if (textMesh == null)
        {
            Debug.LogError("FloatingText: TextMeshProコンポーネントが見つかりません！プレハブを確認してください。");
            Destroy(gameObject); // エラー回避のため自分を消す
            return;
        }

        // テキストと色をセット
        textMesh.text = text;
        textMesh.color = color;

        // --- アニメーション ---

        // 1. 上に移動 (Relativeで現在位置から移動)
        transform.DOMoveY(moveDistance, duration).SetRelative().SetEase(Ease.OutCirc);

        // 2. フェードアウトして消滅
        textMesh.DOFade(0, duration)
            .SetEase(Ease.InQuart)
            .OnComplete(() => Destroy(gameObject)); // 終わったら自分を削除
    }

    private void LateUpdate()
    {
        // 常にカメラの方を向く（ビルボード処理）
        if (Camera.main != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        }
    }
}