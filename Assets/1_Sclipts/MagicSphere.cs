using UnityEngine;

public class MagicSphere : MonoBehaviour
{
    [Header("設定")]
    [Tooltip("球体の有効時間（秒）")]
    public float lifeTime = 60f;
    [Tooltip("変化が始まる半径（実際の球体のScaleと合わせるか少し大きめに）")]
    public float effectRadius = 2.5f; // Scaleが5なら半径は2.5

    // シェーダー側のプロパティ名と紐付けるID
    private static readonly int GlobalSphereCenterID = Shader.PropertyToID("_GlobalMagicSphereCenter");
    private static readonly int GlobalSphereRadiusID = Shader.PropertyToID("_GlobalMagicSphereRadius");

    void Start()
    {
        // 指定時間後にこの球体を破壊する
        Destroy(gameObject, lifeTime);

        // 半径の情報をシェーダー全体に伝える
        Shader.SetGlobalFloat(GlobalSphereRadiusID, effectRadius);
    }

    // 球体のTriggerに入ったとき
    private void OnTriggerEnter(Collider other)
    {
        // 隠されたオブジェクトなら表示させる
        HiddenObject hidden = other.GetComponent<HiddenObject>();
        if (hidden != null)
        {
            hidden.Reveal();
        }

        // 触れたオブジェクトが「Target」タグを持っているか確認する
        if (other.CompareTag("Item"))
        {
            // 触れたオブジェクトをアクティブにする
            other.gameObject.SetActive(true);
        }
    }

    // 球体のTriggerから出たとき（オプション：球が消えたり移動したら隠す場合）
    private void OnTriggerExit(Collider other)
    {
        HiddenObject hidden = other.GetComponent<HiddenObject>();
        if (hidden != null)
        {
            // ここをコメントアウトすれば、一度見つけたお宝は出しっぱなしにできます
            //hidden.Hide();
        }
    }

    // 球体が消滅するとき（Destroyされたとき）の処理
    void Update()
    {
        // 毎フレーム、この球体の現在位置をシェーダー全体に伝える
        // これにより、球体が動いてもテクスチャの変化が追従します
        Shader.SetGlobalVector(GlobalSphereCenterID, transform.position);
    }

    private void OnDestroy()
    {
        // 球体が消えたら、影響範囲を0にして元に戻す（または遠くへ飛ばす）
        // ここでは半径をマイナスにして効果を打ち消す例です
        Shader.SetGlobalFloat(GlobalSphereRadiusID, -1f);
    }
}