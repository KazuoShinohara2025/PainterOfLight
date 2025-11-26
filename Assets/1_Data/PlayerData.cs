using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Sample/PlayerData")]
public class PlayerData : ScriptableObject
{

    [Header("名前")]
    public string Name;

    [Header("HP")]
    [Min(0f)] public float hp = 100f;

    [Header("マナ")]
    [Min(0f)] public float mana = 100f;

    [Header("攻撃力")]
    [Min(0f)] public float attack = 2f;

    [Header("防御力")]
    [Min(0f)] public float def = 1f;

    [Header("移動速度")]
    [Min(0f)] public float moveSpeed = 5.335f;

    [Header("ジャンプ力")]
    [Min(0f)] public float jump = 1.2f;

    [Header("レベル")]
    [Min(0f)] public float lv = 1f;

    [Header("二段ジャンプ")]
    public bool doubleJump = false;

}