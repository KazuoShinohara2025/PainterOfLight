using UnityEngine;

public class MagicSphere : MonoBehaviour
{
    [Header("設定")]
    [Tooltip("球体の有効時間（秒）")]
    public float lifeTime = 60f;

    void Start()
    {
        // 指定時間後にこの球体を破壊する
        Destroy(gameObject, lifeTime);
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
    private void OnDestroy()
    {
        // 必要に応じて、範囲内にいたオブジェクトを全て隠す処理などを追加可能
    }
}