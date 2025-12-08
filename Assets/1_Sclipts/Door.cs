using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Door : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    [SerializeField] private string sceneToLoad; // インスペクターからシーン名を指定
    [SerializeField] private GameObject uiPanel; // 「移動しますか？」UIパネル

    private Animator animator;
    private bool isOpen = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (uiPanel != null)
            uiPanel.SetActive(false); // 初期状態では非表示
    }

    public void Interact()
    {
        if (uiPanel != null)
            uiPanel.SetActive(true); // UIを表示
    }

    // Yesボタンに紐付けるメソッド
    public void OnYesButton()
    {
        ToggleDoor();
        StartCoroutine(WaitForAnimationAndLoadScene());
    }

    // Noボタンに紐付けるメソッド
    public void OnNoButton()
    {
        if (uiPanel != null)
            uiPanel.SetActive(false); // UIを閉じる
    }

    private void ToggleDoor()
    {
        isOpen = !isOpen;
        string animationName = isOpen ? "Open" : "Close";
        animator.Play(animationName);
        Debug.Log($"Door is now {(isOpen ? "open" : "closed")}");
    }

    private System.Collections.IEnumerator WaitForAnimationAndLoadScene()
    {
        // アニメーションの長さを取得
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length);

        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("Scene name not set in Inspector!");
        }
    }
}
