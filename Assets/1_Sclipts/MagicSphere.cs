using UnityEngine;
using DG.Tweening; // DOTween用

public class MagicSphere : MonoBehaviour
{
    [Header("設定")]
    [Tooltip("球体の有効時間（秒）")]
    public float lifeTime = 60f;
    [Tooltip("変化が始まる半径（実際の球体のScaleと合わせるか少し大きめに）")]
    public float effectRadius = 2.5f;

    [Header("消滅アニメーション")]
    public float shrinkDuration = 1.0f; // 消えるときにかかる時間
    public Ease shrinkEase = Ease.InBack; // 消えるときの動き（シュッと消える）

    // シェーダー側のプロパティ名と紐付けるID
    private static readonly int GlobalSphereCenterID = Shader.PropertyToID("_GlobalMagicSphereCenter");
    private static readonly int GlobalSphereRadiusID = Shader.PropertyToID("_GlobalMagicSphereRadius");

    void Start()
    {
        // 半径の情報をシェーダー全体に伝える
        Shader.SetGlobalFloat(GlobalSphereRadiusID, effectRadius);

        // --- 修正: いきなりDestroyせず、時間経過後に「縮小処理」を開始する ---
        // DOTweenの遅延実行機能を使います
        DOVirtual.DelayedCall(lifeTime, StartShrinkDisappear).SetLink(gameObject);
    }

    void Update()
    {
        // 毎フレーム、現在位置をシェーダーに伝える
        Shader.SetGlobalVector(GlobalSphereCenterID, transform.position);
    }

    // --- 追加: ゆっくり消える処理 ---
    private void StartShrinkDisappear()
    {
        // スケールを0にするアニメーション
        transform.DOScale(Vector3.zero, shrinkDuration)
            .SetEase(shrinkEase)
            .OnComplete(() =>
            {
                // アニメーションが終わったら削除
                Destroy(gameObject);
            });
    }

    // 球体のTriggerに入ったとき
    private void OnTriggerEnter(Collider other)
    {
        // デバッグ: そもそも何かに当たったか？
        //Debug.Log($"[Sphere Debug] スフィアが接触: {other.name} (Tag: {other.tag}, Layer: {LayerMask.LayerToName(other.gameObject.layer)})");

        // 1. 隠しオブジェクト処理
        HiddenObject hidden = other.GetComponent<HiddenObject>();
        if (hidden != null) hidden.Reveal();

        // 2. アイテム処理
        if (other.CompareTag("Item")) other.gameObject.SetActive(true);

        // 3. 敵出現処理
        RespawnPoint point = other.GetComponent<RespawnPoint>();

        if (point != null)
        {
            Debug.Log($"[Sphere Debug] RespawnPointコンポーネントを発見！ Type: {point.type}");

            if (point.type == SpawnType.Enemy)
            {
                CharacterCombatController playerController = GetComponentInParent<CharacterCombatController>();

                if (playerController != null)
                {
                    int level = Mathf.FloorToInt(playerController.characterStatus.lv); // ※本来はLv参照ですが、テスト用にHPなど確実に取れる値で試すのも手です
                                                                                    // 正しくは: int level = Mathf.FloorToInt(playerController.characterStatus.lv);

                    if (level < 1) level = 1;

                    Debug.Log($"[Sphere Debug] 敵生成命令を出します。Lv: {level}");
                    point.SpawnEnemies(level);
                }
                else
                {
                    Debug.LogWarning("[Sphere Debug] 親オブジェクトから CharacterCombatController が取得できませんでした！");
                    point.SpawnEnemies(1);
                }
            }
            else
            {
                Debug.Log("[Sphere Debug] TypeがEnemyではありません。");
            }
        }
        else
        {
            // RespawnPointスクリプトがついていないオブジェクトに当たった場合
            // Debug.Log("[Sphere Debug] これはRespawnPointではありません。");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        HiddenObject hidden = other.GetComponent<HiddenObject>();
        if (hidden != null)
        {
            // hidden.Hide();
        }
    }

    private void OnDestroy()
    {
        // シェーダーの効果を消す
        Shader.SetGlobalFloat(GlobalSphereRadiusID, -1f);
    }
}