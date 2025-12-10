using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    [Header("Settings")]
    public float interactionRange = 5f;
    public float detectionRadius = 0.5f;
    public LayerMask interactableLayers;

    [Header("Ray設定")]
    [Tooltip("足元からどれくらい高い位置から判定を出すか（1.3~1.6推奨）")]
    public float rayHeightOffset = 1.5f;

    // デバッグ表示用
    private Vector3 lastRayOrigin;
    private Vector3 lastRayDirection;
    private bool lastHitSuccess;

    // Updateで常時監視する場合（Input Systemの入力判定は別途行ってください）
    private void Update()
    {
        // 常に正面をチェックする（入力があった時だけ呼ぶ形でもOK）
        CheckInteraction();
    }

    public void CheckInteraction()
    {
        // 1. 発射位置を「足元」から「胸/目の高さ」に上げる
        Vector3 rayOrigin = transform.position + (Vector3.up * rayHeightOffset);
        Vector3 rayDirection = transform.forward; // キャラクターの正面

        lastRayOrigin = rayOrigin;
        lastRayDirection = rayDirection;

        // 2. QueryTriggerInteraction.Collide を追加して、Trigger（ドアなど）も検知させる
        if (Physics.SphereCast(
            rayOrigin,
            detectionRadius,
            rayDirection,
            out RaycastHit hitInfo,
            interactionRange,
            interactableLayers,
            QueryTriggerInteraction.Collide // ★ここが重要：Triggerもヒットさせる
        ))
        {
            lastHitSuccess = true;

            // Fキーが押されたらインタラクト実行 (Input Systemの記述に合わせてください)
            if (UnityEngine.InputSystem.Keyboard.current.fKey.wasPressedThisFrame)
            {
                HandleHitObject(hitInfo.collider.gameObject);
            }
        }
        else
        {
            lastHitSuccess = false;
        }
    }

    private void HandleHitObject(GameObject target)
    {
        Debug.Log($"Interact Action: {target.name} [{target.tag}]");

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
                Debug.Log($"Unknown tag: {target.tag}");
                break;
        }
    }

    // --- 各インタラクト処理 ---

    private void InteractWithDoor(GameObject door)
    {
        // 親オブジェクトにスクリプトがある場合も考慮して InParent で探す
        Door doorComponent = door.GetComponentInParent<Door>();
        if (doorComponent != null)
        {
            doorComponent.Interact(this.gameObject);
        }
        else
        {
            Debug.LogWarning("Door component not found!");
        }
    }

    private void InteractWithItem(GameObject item)
    {
        Item itemComponent = item.GetComponentInParent<Item>();
        if (itemComponent != null)
        {
            itemComponent.Interact(this.gameObject);
        }
    }

    private void InteractWithTalk(GameObject npc)
    {
        // 仮：Itemコンポーネントで会話する場合
        Item talkComponent = npc.GetComponentInParent<Item>();
        if (talkComponent != null)
        {
            talkComponent.Interact(this.gameObject);
        }
    }

    // --- Gizmos（可視化） ---
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = lastHitSuccess ? Color.green : Color.red;

            // 視線の高さを考慮して描画
            Vector3 startPos = lastRayOrigin;
            Vector3 endPos = lastRayOrigin + (lastRayDirection * interactionRange);

            Gizmos.DrawLine(startPos, endPos);
            Gizmos.DrawWireSphere(startPos, detectionRadius);
            Gizmos.DrawWireSphere(endPos, detectionRadius);
        }
        else
        {
            // プレイ前でも目安を表示
            Gizmos.color = Color.cyan;
            Vector3 start = transform.position + (Vector3.up * rayHeightOffset);
            Gizmos.DrawLine(start, start + transform.forward * interactionRange);
        }
    }
}