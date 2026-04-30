using UnityEngine;

public class BossSpawning : MonoBehaviour
{
    public GameObject boss;
    private GameObject player;
    void Start()
    {
        player = FindAnyObjectByType<PlayerMovement>().gameObject;

        boss.SetActive(false);
    }
    void Update()
    {
        
    }

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.Equals(player))
        {
            boss.SetActive(true);
        }
	}
}
