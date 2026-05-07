using UnityEngine;
using UnityEngine.Playables;

public class BossSpawning : MonoBehaviour
{
    public GameObject boss;
    public PlayableDirector boss_show;

    private bool boss_activation = false;
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
            boss_activation = false;
        }
    }

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.Equals(player))
        {
            boss_activation = true;
        }
	}
}
