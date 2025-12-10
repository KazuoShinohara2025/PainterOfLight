using UnityEngine;
using UnityEngine.InputSystem; // Input System用
using Cinemachine; // Cinemachine用（カメラ制御）

public class CharacterSwapManager : MonoBehaviour
{
    [Header("キャラクター一覧 (0:Lily, 1:Rose, 2:Tatiana)")]
    public GameObject[] characters;

    [Header("カメラ")]
    public CinemachineVirtualCamera virtualCamera;

    // 現在選択中のキャラクターのインデックス
    private int currentIndex = 0;

    void Start()
    {
        // ゲーム開始時の初期化
        InitializeCharacters();
    }

    void Update()
    {
        // キー入力検知 (1, 2, 3キー)
        if (Keyboard.current.digit1Key.wasPressedThisFrame) SwitchCharacter(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) SwitchCharacter(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) SwitchCharacter(2);
    }

    private void InitializeCharacters()
    {
        // 最初のキャラだけ有効にし、他は無効にする
        for (int i = 0; i < characters.Length; i++)
        {
            characters[i].SetActive(i == currentIndex);
        }

        UpdateCameraTarget();
    }

    private void SwitchCharacter(int newIndex)
    {
        // 既に選択中のキャラなら何もしない、または範囲外なら無視
        if (currentIndex == newIndex || newIndex >= characters.Length) return;

        // 1. 現在のキャラクター（交代元）の位置と回転を取得
        Transform currentTransform = characters[currentIndex].transform;
        Vector3 currentPos = currentTransform.position;
        Quaternion currentRot = currentTransform.rotation;

        // 2. 現在のキャラクターを無効化
        characters[currentIndex].SetActive(false);

        // 3. 新しいキャラクターを有効化
        characters[newIndex].SetActive(true);

        // 4. 新しいキャラクターの位置を合わせる
        // CharacterControllerを使っている場合、直接transformを変えても反映されないことがあるため、一時的にOFFにする
        CharacterController cc = characters[newIndex].GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false; // ワープさせるために一時無効化
            characters[newIndex].transform.position = currentPos;
            characters[newIndex].transform.rotation = currentRot;
            cc.enabled = true;  // 再有効化
        }
        else
        {
            // CharacterControllerがない場合は普通に移動
            characters[newIndex].transform.position = currentPos;
            characters[newIndex].transform.rotation = currentRot;
        }

        // インデックス更新
        currentIndex = newIndex;

        // 5. カメラの追従先を更新
        UpdateCameraTarget();

        // 6. 敵のターゲット情報を更新（重要）
        UpdateEnemyTargets();
    }

    private void UpdateCameraTarget()
    {
        if (virtualCamera != null)
        {
            // StarterAssetsの場合、カメラの追従対象は "PlayerCameraRoot" という子オブジェクトにあることが多いです
            Transform cameraRoot = characters[currentIndex].transform.Find("PlayerCameraRoot");

            // 見つからなければキャラクターそのものをターゲットにする
            if (cameraRoot == null) cameraRoot = characters[currentIndex].transform;

            virtualCamera.Follow = cameraRoot;
            virtualCamera.LookAt = cameraRoot;
        }
    }

    // 敵が古いプレイヤー（非アクティブ）を追いかけ続けないようにターゲットを更新する
    private void UpdateEnemyTargets()
    {
        GameObject activePlayer = characters[currentIndex];

        // シーン上の全てのEvilControllerを探してターゲットを再設定
        EvilController[] enemies = FindObjectsOfType<EvilController>();
        foreach (var enemy in enemies)
        {
            enemy.player = activePlayer.transform;

            // 攻撃中などで停止している敵をリセットしたい場合はここで行う
            // enemy.OnAttackEnd(); 
        }
    }
}