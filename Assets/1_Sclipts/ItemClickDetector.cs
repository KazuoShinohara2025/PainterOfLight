using UnityEngine;

public class ItemClickDetector : MonoBehaviour
{
    [SerializeField] private GameObject open;
    public Transform WoodChest; // WoodChestのTransformをInspectorで指定
    public float interactDistance = 3f; // クリック可能な距離

    public void Start()
    {
        open.SetActive(false);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F)) 
        {
            // プレイヤーとWoodChestの距離をチェック
            float distance = Vector3.Distance(transform.position, WoodChest.position);
            if (distance <= interactDistance)
            {
                // Raycastでクリック対象がWoodChestか確認
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform == WoodChest)
                    {
                        open.SetActive(true);
                        //Destroy(WoodChest.gameObject);
                    }
                }
            }
        }
    }
}