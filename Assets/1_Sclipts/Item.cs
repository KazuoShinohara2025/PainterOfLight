using UnityEngine;

public class Item : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        InteractItem();
    }

    public void InteractItem()
    {
        GameObject child = this.transform.Find("Base").gameObject;
        Debug.Log("Item is picked up.");
        child.gameObject.SetActive(true);
        // アイテム取得処理（ここではオブジェクトを非アクティブにします）
        //gameObject.SetActive(false);
        // 他の処理をここに追加（例：インベントリにアイテムを追加する等）
    }
}

