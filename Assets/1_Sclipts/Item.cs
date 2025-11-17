using UnityEngine;

public class Item : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        InteractItem();
    }

    public void InteractItem()
    {
        Debug.Log("Item is picked up.");
        // アイテム取得処理（ここではオブジェクトを非アクティブにします）
        gameObject.SetActive(false);
        // 他の処理をここに追加（例：インベントリにアイテムを追加する等）
    }
}

