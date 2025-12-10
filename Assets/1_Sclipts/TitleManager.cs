using UnityEngine;
using UnityEngine.SceneManagement; // これがないとシーン遷移できません

public class TitleManager : MonoBehaviour
{
    // ボタンが押されたときに実行する関数
    // (Buttonに割り当てるため public にする必要があります)
    public void LoadStandByScene()
    {
        // "StandByScene" という名前のシーンを読み込む
        SceneManager.LoadScene("StandByScene");
    }
}