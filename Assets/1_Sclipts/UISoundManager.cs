using UnityEngine;

public class UISoundManager : MonoBehaviour
{
    // static instance (シングルトン) は削除しました。
    // 各シーンのボタンは、そのシーン内のGameManagerを直接参照させます。

    [Header("UI Audio Clips")]
    public AudioClip buttonClickSE;

    private AudioSource audioSource;

    private void Awake()
    {
        // ★DontDestroyOnLoad処理を削除

        // AudioSourceの取得・設定
        audioSource = GetComponent<AudioSource>();

        // もしついていなければ追加する（保険）
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D
        }
    }

    public void PlayClickSound()
    {
        if (buttonClickSE != null && audioSource != null)
        {
            audioSource.PlayOneShot(buttonClickSE);
        }
        else
        {
            if (buttonClickSE == null) Debug.LogWarning("ButtonClickSEが未設定です");
            if (audioSource == null) Debug.LogError("AudioSourceがありません！");
        }
    }
}