using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;

public class InteractionManager : MonoBehaviour
{
    public float interactionRange = 5f;
    public Vector3 detectionCenter;  // 検出の中心点（プレイヤーの位置など）
    public float detectionRadius = 2;    // 検出範囲の半径
    RaycastHit hit;
    public LayerMask interactableLayers;

    public void HandleInteraction(Vector3 rayOrigin, Vector3 rayDirection)
    {
        Debug.Log($"HandleInteraction called. Range: {interactionRange}, Layers: {interactableLayers.value}");
        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hitInfo, interactionRange, interactableLayers) ||
            Physics.SphereCast(transform.position, detectionRadius, transform.forward, out hit, 0f, interactableLayers))
        {
            
            Debug.Log($"Raycast hit: {hitInfo.collider.gameObject.name}, Tag: {hitInfo.collider.tag}");
            switch (hitInfo.collider.tag)
            {
                case "Door":
                    InteractWithDoor(hitInfo.collider.gameObject);
                    break;
                case "Item":
                    InteractWithItem(hitInfo.collider.gameObject);
                    break;
                case "NPC":
                    InteractWithTalk(hitInfo.collider.gameObject);
                    break;
                default:
                    Debug.Log($"Interacting with unknown object: {hitInfo.collider.tag}");
                    break;
            }
        }
        else
        {
            Debug.Log("Raycast did not hit any object");
        }
    }

    private void InteractWithDoor(GameObject door)
    {
        Debug.Log($"Attempting to interact with door: {door.name}");
        Door doorComponent = door.GetComponent<Door>();
        if (doorComponent != null)
        {
            doorComponent.Interact();
        }
        else
        {
            Debug.LogError($"Door object {door.name} does not have a Door component");
        }
    }

    private void InteractWithItem(GameObject item)
    {
        Item itemComponent = item.GetComponent<Item>();
        if (itemComponent != null)
        {
            itemComponent.Interact();
        }
        else
        {
            Debug.Log("Item object does not have an Item component");
        }
    }

    private void InteractWithTalk(GameObject talk)
    {
        Item talkComponent = talk.GetComponent<Item>();
        if (talkComponent != null)
        {
            talkComponent.Interact();
        }
        else
        {
            Debug.Log("Talk object does not have an Talk component");
        }
    }
}
