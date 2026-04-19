using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private EnemyMovement enemy;
	[SerializeField] private Transform player;

	[Header("Movement")]
	[SerializeField] private float stopDistance = 0.8f;
	[SerializeField] private float acceleration = 12f;

	private Animator animator;

	private Rigidbody2D rb;

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();

		if (enemy == null)
			enemy = GetComponent<EnemyMovement>();

		if (player == null)
		{
			GameObject p = GameObject.FindGameObjectWithTag("Player");
			if (p != null)
				player = p.transform;
		}
	}

	void FixedUpdate()
	{
		if (player == null || enemy == null || enemy.tag == "Untagged")
			return;

		EnemyStats stats = enemy.GetStats();

		float distance = Vector2.Distance(transform.position, player.position);

		if (distance <= stats.attackRange)
		{
			rb.linearVelocity = Vector2.zero;

			if (enemy.CanAttack())
			{
				AttackPlayer();
				enemy.ResetAttackCooldown();
			}

			FacePlayer();
			return;
		}

		if (distance > stopDistance)
		{
			Vector2 dir = (player.position - transform.position).normalized;

			Vector2 targetVelocity = dir * stats.moveSpeed;

			rb.linearVelocity = Vector2.MoveTowards(
				rb.linearVelocity,
				targetVelocity,
				acceleration * Time.fixedDeltaTime
			);

			animator.SetBool("isMoving", true);

			FacePlayer();
		}
		else
		{
			rb.linearVelocity = Vector2.zero;
		}
	}

	private void AttackPlayer()
	{
		Debug.Log(enemy.GetStats().enemyName + " attacked player!");

		animator.SetTrigger("attack");
		player.GetComponent<PlayerMovement>().TakeDamage(enemy.GetStats().damage, transform.position);
	}

	private void FacePlayer()
	{
		if (player.position.x > transform.position.x)
			transform.rotation = Quaternion.Euler(0, 0, 0);
		else
			transform.rotation = Quaternion.Euler(0, 180, 0);
	}
}
