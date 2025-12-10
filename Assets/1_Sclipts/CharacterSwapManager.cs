using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using StarterAssets; // ThirdPersonControllerへの参照用
using System.Collections; // コルーチン用

public class CharacterSwapManager : MonoBehaviour
{
    [Header("キャラクター一覧 (0:Lily, 1:Rose, 2:Tatiana)")]
    public GameObject[] characters;

    [Header("カメラ")]
    public CinemachineVirtualCamera virtualCamera;

    [Header("設定")]
    [Tooltip("交代後の入力無効化時間（秒）")]
    public float swapInputCooldown = 0.1f;

    private int currentIndex = 0;
    private bool isSwapping = false; // 交代処理中フラグ

    void Start()
    {
        InitializeCharacters();
    }

    void Update()
    {
        // 交代中なら入力させない
        if (isSwapping) return;

        // キー入力検知
        if (Keyboard.current.digit1Key.wasPressedThisFrame) SwitchCharacter(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) SwitchCharacter(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) SwitchCharacter(2);
    }

    private void InitializeCharacters()
    {
        for (int i = 0; i < characters.Length; i++)
        {
            characters[i].SetActive(i == currentIndex);
        }
        UpdateCameraTarget();
    }

    private void SwitchCharacter(int newIndex)
    {
        if (currentIndex == newIndex || newIndex >= characters.Length) return;

        StartCoroutine(SwapProcess(newIndex));
    }

    // 安全に交代するためのコルーチン
    private IEnumerator SwapProcess(int newIndex)
    {
        isSwapping = true; // 入力ロック開始

        // 1. 現在のキャラクター（交代元）の情報を取得
        GameObject oldChar = characters[currentIndex];
        GameObject newChar = characters[newIndex];

        Transform oldTransform = oldChar.transform;
        Vector3 oldPos = oldTransform.position;
        Quaternion oldRot = oldTransform.rotation;

        // コントローラーの内部情報を取得（回転角度など）
        ThirdPersonController oldController = oldChar.GetComponent<ThirdPersonController>();
        float oldYaw = oldController.CinemachineTargetYaw;
        float oldPitch = oldController.CinemachineTargetPitch;
        float oldRotVel = oldController.RotationVelocity;

        // 2. 交代元を無効化
        oldChar.SetActive(false);

        // 3. 新しいキャラクターを有効化
        newChar.SetActive(true);

        // 4. 位置と回転を同期
        CharacterController cc = newChar.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false; // ワープのためにCCを切る

        newChar.transform.position = oldPos;
        newChar.transform.rotation = oldRot;

        if (cc != null) cc.enabled = true;

        // 5. ★重要★ コントローラー内部の回転情報を同期（これで逆走を防ぐ）
        ThirdPersonController newController = newChar.GetComponent<ThirdPersonController>();
        if (newController != null)
        {
            newController.SyncState(oldYaw, oldPitch, oldRotVel);
        }

        // インデックス更新
        currentIndex = newIndex;

        // 6. カメラのターゲット更新
        Transform cameraRoot = newChar.transform.Find("PlayerCameraRoot");
        if (cameraRoot == null) cameraRoot = newChar.transform;

        virtualCamera.Follow = cameraRoot;
        virtualCamera.LookAt = cameraRoot;

        // 7. ★重要★ Cinemachineにワープを通知（これでカメラのガタつきを防ぐ）
        // 「ターゲットが瞬時にここへ移動した」と伝えることでDampingを無視させます
        virtualCamera.OnTargetObjectWarped(cameraRoot, cameraRoot.position - oldPos);

        // 8. 敵のターゲット更新
        UpdateEnemyTargets();

        // わずかに待機して物理演算を馴染ませる
        yield return new WaitForSeconds(swapInputCooldown);

        isSwapping = false; // 入力ロック解除
    }

    private void UpdateCameraTarget()
    {
        if (virtualCamera != null)
        {
            Transform cameraRoot = characters[currentIndex].transform.Find("PlayerCameraRoot");
            if (cameraRoot == null) cameraRoot = characters[currentIndex].transform;
            virtualCamera.Follow = cameraRoot;
            virtualCamera.LookAt = cameraRoot;
        }
    }

    private void UpdateEnemyTargets()
    {
        GameObject activePlayer = characters[currentIndex];
        EvilController[] enemies = FindObjectsOfType<EvilController>();
        foreach (var enemy in enemies)
        {
            enemy.player = activePlayer.transform;
        }
    }
}