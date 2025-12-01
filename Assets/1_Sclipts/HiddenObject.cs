using UnityEngine;

public class HiddenObject : MonoBehaviour
{
    private Collider myCollider;
    private Renderer myRenderer;

    void Awake()
    {
        myCollider = GetComponent<Collider>();
        myRenderer = GetComponent<Renderer>();

        // Å‰‚Í‰B‚·i“–‚½‚è”»’è‚È‚µAŒ©‚¦‚È‚¢j
        Hide();
    }

    public void Reveal()
    {
        if (myCollider) myCollider.enabled = true;
        if (myRenderer) myRenderer.enabled = true;
    }

    public void Hide()
    {
        if (myCollider) myCollider.enabled = false;
        if (myRenderer) myRenderer.enabled = false;
    }
}