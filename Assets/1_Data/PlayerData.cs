using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Sample/PlayerData")]
public class PlayerData : ScriptableObject
{
    [Header("HP")]
    [Min(0f)] public float hp = 100f;

    [Header("マナ")]
    [Min(0f)] public float mana = 100f;

    [Header("攻撃力")]
    [Min(0f)] public float attack = 2f;

    [Header("移動速度")]
    [Min(0f)] public float moveSpeed = 2f;

    [Header("ジャンプ力")]
    [Min(0f)] public float jump = 2f;

    [Header("二段ジャンプ")]
    public bool doubleJump = true;

}