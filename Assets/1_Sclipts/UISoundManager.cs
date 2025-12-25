using UnityEngine;

public class UISoundManager : MonoBehaviour
{
    public static UISoundManager instance;

    [Header("UI Audio Clips")]
    public AudioClip buttonClickSE;

    private AudioSource audioSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // シーン遷移しても音を途切れさせない
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    // ボタンの OnClick() に登録するメソッド
    public void PlayClickSound()
    {
        // ★このログが出るか確認してください
        Debug.Log("Button Clicked! Attempting to play sound.");

        if (buttonClickSE != null && audioSource != null)
        {
            audioSource.PlayOneShot(buttonClickSE);
        }
        else
        {
            if (buttonClickSE == null) Debug.LogError("ButtonClickSE が設定されていません！");
            if (audioSource == null) Debug.LogError("AudioSource がありません！");
        }
    }
}