using UnityEngine;
using UnityEngine.SceneManagement; // シーン移動に必要
using UnityEngine.UI;

public class Door : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    [Tooltip("移動先のシーン名をここに入力してください (例: StandByScene)")]
    [SerializeField] private string sceneToLoad;

    [Header("UI Reference")]
    [SerializeField] private GameObject uiPanel; // 「移動しますか？」UIパネル

    private Animator animator;
    private bool isOpen = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (uiPanel != null)
        {
            uiPanel.SetActive(false); // 初期状態では非表示
        }
    }

    // インターフェース実装
    public void Interact(GameObject player)
    {
        // 既に開いている、またはUIが出ている場合は何もしない
        if (uiPanel.activeSelf) return;

        ShowConfirmationUI();
    }

    private void ShowConfirmationUI()
    {
        if (uiPanel != null)
        {
            uiPanel.SetActive(true); // UIを表示

            // 【重要】ゲーム時間を止める（これでキャラクター操作も止まります）
            Time.timeScale = 0f;

            // マウスカーソルを表示・ロック解除してボタンを押せるようにする
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Debug.LogError("Ui Panelが設定されていません！Inspectorを確認してください。");
        }
    }

    // Yesボタン
    public void OnYesButton()
    {
        // 1. ゲーム時間を再開（これをしないとアニメーションも再生されず、コルーチンも進みません）
        Time.timeScale = 1f;

        // 2. UIを消す
        if (uiPanel != null) uiPanel.SetActive(false);

        // 3. カーソルを戻す（ロード中も操作させたくない場合は表示したままでもOKですが、今回は戻します）
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // 4. ドアを開けて移動処理開始
        ToggleDoor();
        StartCoroutine(WaitForAnimationAndLoadScene());
    }

    // Noボタン
    public void OnNoButton()
    {
        // 1. ゲーム時間を再開
        Time.timeScale = 1f;

        // 2. UIを消す
        if (uiPanel != null) uiPanel.SetActive(false);

        // 3. カーソルを戻してゲーム再開
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void ToggleDoor()
    {
        isOpen = !isOpen;
        string animationName = isOpen ? "Open" : "Close";
        if (animator != null) animator.Play(animationName);
    }

    private System.Collections.IEnumerator WaitForAnimationAndLoadScene()
    {
        // アニメーション開始を1フレーム待つ
        yield return null;

        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            // アニメーションの長さ分待機
            yield return new WaitForSeconds(stateInfo.length);
        }

        // シーン移動処理
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.Log($"シーン '{sceneToLoad}' へ移動します...");
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogError("移動先のシーン名が設定されていません！InspectorでScene To Loadに入力してください。");
        }
    }
}