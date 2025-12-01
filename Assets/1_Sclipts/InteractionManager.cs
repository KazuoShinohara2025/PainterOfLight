using UnityEngine;
using static UnityEditor.Progress;
// 不要なusingを削除しました

public class InteractionManager : MonoBehaviour
{
    [Header("Settings")]
    public float interactionRange = 5f;
    public float detectionRadius = 0.5f; // 半径は0.5f程度が推奨（2だと大きすぎて床に当たります）
    public LayerMask interactableLayers;

    // デバッグ表示用に保持する変数
    private Vector3 lastRayOrigin;
    private Vector3 lastRayDirection;
    private bool lastHitSuccess;

    public void HandleInteraction(Vector3 rayOrigin, Vector3 rayDirection)
    {
        // デバッグ用に値を保存
        lastRayOrigin = rayOrigin;
        lastRayDirection = rayDirection;

        // 【修正点】RaycastとSphereCastを混ぜず、SphereCastのみを使用します。
        // SphereCastは「太いRaycast」なので、Raycastの代わりになります。
        if (Physics.SphereCast(rayOrigin, detectionRadius, rayDirection, out RaycastHit hitInfo, interactionRange, interactableLayers))
        {
            lastHitSuccess = true;
            Debug.Log($"Hit Object: {hitInfo.collider.gameObject.name}, Tag: {hitInfo.collider.tag}");

            // 取得したオブジェクトに対して処理を行う
            GameObject target = hitInfo.collider.gameObject;

            switch (target.tag)
            {
                case "Door":
                    InteractWithDoor(target);
                    break;
                case "Item":
                    InteractWithItem(target);
                    break;
                case "NPC":
                    InteractWithTalk(target);
                    break;
                default:
                    Debug.Log($"Interacting with unknown object tag: {target.tag}");
                    break;
            }
        }
        else
        {
            lastHitSuccess = false;
            // Debug.Log("SphereCast did not hit any object"); // 頻繁に出るのでコメントアウト推奨
        }
    }

    private void InteractWithDoor(GameObject door)
    {
        // Debug.Log($"Attempting to interact with door: {door.name}");
        // 相手がDoorコンポーネントを持っているか確認
        // ※DoorクラスがIPublicインターフェースなどを継承しているとなお良いです
        if (door.TryGetComponent<Door>(out Door doorComponent))
        {
            doorComponent.Interact();
        }
        else
        {
            Debug.LogError($"Object tagged 'Door' ({door.name}) is missing 'Door' component!");
        }
    }

    private void InteractWithItem(GameObject item)
    {
        if (item.TryGetComponent<Item>(out Item itemComponent))
        {
            itemComponent.Interact();
        }
        else
        {
            Debug.LogWarning($"Object tagged 'Item' ({item.name}) is missing 'Item' component!");
        }
    }

    private void InteractWithTalk(GameObject npc)
    {
        // 【修正点】元のコードではここでItemコンポーネントを取得していましたが、
        // 会話なので NPC コンポーネント（またはそれに準ずるもの）を取得すべきです。
        // ここでは仮に 'NPC' というクラスがあると仮定して修正しています。
        // もし会話も 'Item' クラスで管理しているなら元のままで構いません。

        // 例: NPCクラスを取得する場合
        
        if (npc.TryGetComponent<NPC>(out NPC npcComponent))
        {
            npcComponent.Interact();
        }
        else
        {
            Debug.LogWarning($"Object tagged 'npc' ({npc.name}) is missing 'npc' component!");
        }
    }

    // エディタ上で判定範囲（SphereCast）を可視化する機能
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = lastHitSuccess ? Color.green : Color.red;

            // 線の描画
            Gizmos.DrawRay(lastRayOrigin, lastRayDirection * interactionRange);

            // 当たり判定の球を描画（位置の目安）
            Vector3 endPosition = lastRayOrigin + (lastRayDirection * interactionRange);
            Gizmos.DrawWireSphere(lastRayOrigin, detectionRadius); // 開始地点
            Gizmos.DrawWireSphere(endPosition, detectionRadius);   // 終了地点
        }
    }
}