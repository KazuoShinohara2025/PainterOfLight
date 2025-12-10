using UnityEngine;

public class MagicSphereTrigger : MonoBehaviour
{
    // スフィアが何かに触れたときに呼ばれる
    private void OnTriggerEnter(Collider other)
    {
        // 触れた相手が RespawnPoint か確認
        RespawnPoint point = other.GetComponent<RespawnPoint>();

        if (point != null)
        {
            // そのポイントが「敵用 (Enemy)」なら処理開始
            if (point.type == SpawnType.Enemy)
            {
                // プレイヤーのレベルを取得する
                // スフィアは Player の子オブジェクトとして生成されているため、親(Player)から取得
                CharacterCombatController playerController = GetComponentInParent<CharacterCombatController>();

                if (playerController != null && playerController.characterStatus != null)
                {
                    // レベルを取得（floatなのでintに変換）
                    int level = Mathf.FloorToInt(playerController.characterStatus.lv);

                    // 最低でも1体は出す
                    if (level < 1) level = 1;

                    // ポイントに敵出現を依頼
                    point.SpawnEnemies(level);
                }
                else
                {
                    // 万が一プレイヤーが取得できない場合の保険（1体出す）
                    point.SpawnEnemies(1);
                }
            }
        }
    }
}