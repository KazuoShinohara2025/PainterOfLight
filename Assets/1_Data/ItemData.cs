using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Sample/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("HP")]
    [Min(0f)] public float hp = 1.1f;

    [Header("マナ")]
    [Min(0f)] public float mana = 1.1f;

    [Header("攻撃力")]
    [Min(0f)] public float attack = 1.1f;

    [Header("移動速度")]
    [Min(0f)] public float moveSpeed = 1.1f;

    [Header("ジャンプ力")]
    [Min(0f)] public float jump = 1.1f;

    [Header("二段ジャンプ")]
    public bool doubleJump = true;

}