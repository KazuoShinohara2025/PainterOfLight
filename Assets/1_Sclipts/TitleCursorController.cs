using UnityEngine;

public class TitleCursorController : MonoBehaviour
{
    void Start()
    {
        // シーン開始時にカーソルを表示・ロック解除
        ShowCursor();
    }

    void Update()
    {
        // 毎フレーム強制的に表示（他のスクリプトによる上書き防止）
        ShowCursor();
    }

    private void ShowCursor()
    {
        if (!Cursor.visible || Cursor.lockState != CursorLockMode.None)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}