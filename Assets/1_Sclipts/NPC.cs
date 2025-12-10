using UnityEngine;

public class NPC : MonoBehaviour, IInteractable
{
    [Header("UI参照")]
    [Tooltip("このNPCに対応するショップUI")]
    public NPCShopUI shopUI;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // インターフェース実装
    public void Interact(GameObject player)
    {
        // UIが開いていないときだけ実行
        if (shopUI != null && !shopUI.IsShopOpen)
        {
            // プレイヤーの方を向く（オプション）
            Vector3 lookPos = player.transform.position;
            lookPos.y = transform.position.y;
            transform.LookAt(lookPos);

            // 会話アニメーション
            if (animator != null) animator.Play("Talk");

            // UIを開く
            shopUI.StartConversation(player);
        }
    }
}