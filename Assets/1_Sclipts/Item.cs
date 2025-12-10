using UnityEngine;

public class Item : MonoBehaviour, IInteractable
{
    [Header("アイテムデータ")]
    public ItemData itemData; // 上昇量の候補が入ったデータ

    // 一度開けたかどうか
    private bool isOpened = false;

    // インターフェースの実装
    public void Interact(GameObject player)
    {
        if (isOpened) return;

        InteractItem(player);
    }

    private void InteractItem(GameObject player)
    {
        Debug.Log("Item Chest Opened & Destroyed!");
        isOpened = true;

        // 1. プレイヤーに「ランダムなステータス強化」を依頼する
        if (player != null && itemData != null)
        {
            var combatController = player.GetComponent<CharacterCombatController>();
            if (combatController != null)
            {
                // ★ここを新しいメソッド呼び出しに変更（後述）
                combatController.ApplyRandomStatBoost(itemData, 2);
            }
        }

        // 2. コライダーを無効化（二重取得防止）
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 3. 宝箱オブジェクト自体を削除
        // 必要ならパーティクルやSEを出してから消すためにDestroyの第二引数で遅らせてもOK
        Destroy(gameObject);
    }
}