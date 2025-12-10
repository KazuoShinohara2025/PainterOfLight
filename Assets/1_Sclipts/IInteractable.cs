using UnityEngine;

public interface IInteractable
{
    // 引数にプレイヤー自身（GameObject）を受け取るように変更
    void Interact(GameObject player);
}