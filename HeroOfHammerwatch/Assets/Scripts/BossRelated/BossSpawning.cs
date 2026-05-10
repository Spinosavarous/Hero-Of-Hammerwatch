using UnityEngine;
using UnityEngine.Playables;

public class BossSpawning : MonoBehaviour
{
    public GameObject boss;
    public PlayableDirector boss_show;

    private bool boss_activation = false;
    private bool boss_isSpawning = false;
    private GameObject player;
    void Start()
    {
        player = FindAnyObjectByType<PlayerMovement>().gameObject;

        boss.SetActive(false);
    }
    void Update()
    {
        if (boss_activation && boss_show.state != PlayState.Playing)
        {
            boss.SetActive(true);
            boss.tag = "Enemy";
            boss_activation = false;
			Destroy(transform.gameObject, 3);
		}
    }

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.Equals(player) && !boss_isSpawning)
        {
            boss_activation = true;
            boss_show.Play();
            boss_isSpawning = true;
        }
	}
}
