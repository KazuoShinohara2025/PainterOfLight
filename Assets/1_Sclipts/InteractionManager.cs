using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    // ... (前半の変数はそのまま) ...
    [Header("Settings")]
    public float interactionRange = 5f;
    public float detectionRadius = 0.5f;
    public LayerMask interactableLayers;

    [Header("Ray設定")]
    [Tooltip("足元からどれくらい高い位置から判定を出すか（1.3~1.6推奨）")]
    public float rayHeightOffset = 1.5f;

    private Vector3 lastRayOrigin;
    private Vector3 lastRayDirection;
    private bool lastHitSuccess;

    private void Update()
    {
        CheckInteraction();
    }

    public void CheckInteraction()
    {
        // ... (SphereCastの処理はそのまま) ...
        Vector3 rayOrigin = transform.position + (Vector3.up * rayHeightOffset);
        Vector3 rayDirection = transform.forward;

        lastRayOrigin = rayOrigin;
        lastRayDirection = rayDirection;

        if (Physics.SphereCast(
            rayOrigin,
            detectionRadius,
            rayDirection,
            out RaycastHit hitInfo,
            interactionRange,
            interactableLayers,
            QueryTriggerInteraction.Collide
        ))
        {
            lastHitSuccess = true;

            // Fキーでインタラクト
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
            case "NPC": // NPCタグの場合
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
        Door doorComponent = door.GetComponentInParent<Door>();
        if (doorComponent != null) doorComponent.Interact(this.gameObject);
    }

    private void InteractWithItem(GameObject item)
    {
        Item itemComponent = item.GetComponentInParent<Item>();
        if (itemComponent != null) itemComponent.Interact(this.gameObject);
    }

    private void InteractWithTalk(GameObject npc)
    {
        // 【修正】NPCコンポーネントを取得してInteractを呼ぶ
        NPC npcComponent = npc.GetComponentInParent<NPC>();
        if (npcComponent != null)
        {
            npcComponent.Interact(this.gameObject);
        }
        else
        {
            Debug.LogWarning("NPC component not found on object tagged NPC");
        }
    }

    // ... (Gizmosはそのまま) ...
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = lastHitSuccess ? Color.green : Color.red;
            Vector3 startPos = lastRayOrigin;
            Vector3 endPos = lastRayOrigin + (lastRayDirection * interactionRange);
            Gizmos.DrawLine(startPos, endPos);
            Gizmos.DrawWireSphere(startPos, detectionRadius);
            Gizmos.DrawWireSphere(endPos, detectionRadius);
        }
    }
}