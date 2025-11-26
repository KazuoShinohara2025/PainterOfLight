using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    private Animator animator;
    private bool isOpen = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Interact()
    {
        ToggleDoor();
    }

    private void ToggleDoor()
    {
        isOpen = !isOpen;
        string animationName = isOpen ? "Open" : "Close";

        animator.Play(animationName);

        Debug.Log($"Door is now {(isOpen ? "open" : "closed")}");
    }
}
