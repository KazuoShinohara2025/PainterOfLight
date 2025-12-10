using UnityEngine;

public class EvilWeapon : MonoBehaviour
{
    [HideInInspector] public float damagePower;

    private void OnTriggerEnter(Collider other)
    {
        // âΩÇ©Ç…ìñÇΩÇ¡ÇΩÇÁÉçÉOÇèoÇ∑
        Debug.Log($"EvilWeapon hit: {other.name} (Tag: {other.tag})");

        if (other.CompareTag("Player"))
        {
            Debug.Log("Tag is Player!");
            CharacterCombatController playerCombat = other.GetComponent<CharacterCombatController>();

            if (playerCombat != null)
            {
                Debug.Log("Script found! Dealing damage.");
                playerCombat.PlayerTakeDamage(damagePower);
                GetComponent<Collider>().enabled = false;
            }
            else
            {
                Debug.LogError("Player tag found, but CharacterCombatController script is MISSING!");
            }
        }
    }
}