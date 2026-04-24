using UnityEngine;

public class GoldScript : MonoBehaviour
{
	[Header("Gold Stats")]
	[SerializeField] private int speed;

	private Transform Player;
	void Start()
	{
		Player = GameObject.FindGameObjectWithTag("Player").transform;
	}

	void Update()
	{
		transform.position = Vector3.MoveTowards(transform.position, Player.position, speed * Time.deltaTime);
	}

}
