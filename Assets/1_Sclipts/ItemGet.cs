using UnityEngine;

public class ItemGet : MonoBehaviour
{
    [SerializeField] private GameObject open;

    public void Start()
    {
        open.SetActive(false);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            OnButtonClick();
        }
    }
    public void OnButtonClick()
    {
        open.SetActive(true);
        //Destroy(gameObject);
    }
}