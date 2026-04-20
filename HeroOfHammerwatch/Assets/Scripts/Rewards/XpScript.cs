using UnityEngine;

public class XpScript : MonoBehaviour
{
    [Header("XP Stats")]
    [SerializeField] private int speed;
    [SerializeField] private int strength;

    private Transform Player;
	void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player").transform;
	}

    void Update()
    {
      transform.position = Vector3.MoveTowards(transform.position, Player.position, speed * Time.deltaTime);
	}

    public int GetStrength()
    {
        return strength;
    }
}
