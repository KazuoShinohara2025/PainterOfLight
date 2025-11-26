using UnityEngine;

public class NPC : MonoBehaviour, IInteractable
{
    private Animator animator;
    private bool isTalk = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Interact()
    {
        ToggleTalk();
    }

    private void ToggleTalk()
    {
        isTalk = !isTalk;
        string animationName = isTalk ? "Talk" : "Ignore";

        animator.Play(animationName);

        Debug.Log($"Talk is now {(isTalk ? "talk" : "ignore")}");
    }
}
