using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Sample/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("HP")]
    [Min(0f)] public float hp = 10f;

    [Header("ƒ}ƒi")]
    [Min(0f)] public float mana = 10f;

    [Header("UŒ‚—Í")]
    [Min(0f)] public float baseAttack = 5f;

    [Header("–hŒä—Í")]
    [Min(0f)] public float baseDefense = 5f;

    [Header("ƒXƒLƒ‹”{—¦")]
    [Min(0f)] public float skillDamageMultiplier = 0.2f;

    [Header("ƒAƒ‹ƒeƒBƒƒbƒg”{—¦")]
    [Min(0f)] public float ultDamageMultiplier = 0.2f;

    [Header("Šl“¾ƒS[ƒ‹ƒh")]
    [Min(0)] public int totalGold = 2000;

}