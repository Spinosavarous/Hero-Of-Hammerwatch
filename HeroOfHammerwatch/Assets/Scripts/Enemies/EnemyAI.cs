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

	[Header("Avoidance")]
	[SerializeField] private float avoidCheckDistance = 0.8f;
	[SerializeField] private float sideCheckAngle = 45f;

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

		// ATTACK STATE
		if (distance <= stats.attackRange)
		{
			rb.linearVelocity = Vector2.zero;
			animator.SetBool("isMoving", false);

			if (enemy.CanAttack())
			{
				AttackPlayer();
				enemy.ResetAttackCooldown();
			}

			FacePlayer();
			return;
		}

		// MOVE STATE
		if (distance > stopDistance)
		{
			Vector2 dir = GetSmartDirection();

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
			animator.SetBool("isMoving", false);
		}
	}

	private Vector2 GetSmartDirection()
	{
		Vector2 toPlayer = (player.position - transform.position).normalized;

		if (!IsBlocked(toPlayer))
			return toPlayer;

		Vector2 left = Quaternion.Euler(0, 0, sideCheckAngle) * toPlayer;
		Vector2 right = Quaternion.Euler(0, 0, -sideCheckAngle) * toPlayer;

		bool leftFree = !IsBlocked(left);
		bool rightFree = !IsBlocked(right);

		if (leftFree && !rightFree)
			return left;

		if (rightFree && !leftFree)
			return right;

		if (leftFree && rightFree)
			return Random.value > 0.5f ? left : right;

		return Vector2.Perpendicular(toPlayer);
	}

	private bool IsBlocked(Vector2 direction)
	{
		RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, avoidCheckDistance);

		if (hit.collider == null)
			return false;

		if (hit.collider.gameObject == gameObject)
			return false;

		if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("Enemy"))
			return false;

		if (hit.collider.isTrigger)
			return false;

		return true;
	}

	private void AttackPlayer()
	{
		Debug.Log(enemy.GetStats().enemyName + " attacked player!");

		animator.SetTrigger("attack");
		player.GetComponent<PlayerMovement>()
			.TakeDamage(enemy.GetStats().damage, transform.position);
	}

	private void FacePlayer()
	{
		if (player.position.x > transform.position.x)
			transform.rotation = Quaternion.Euler(0, 180, 0);
		else
			transform.rotation = Quaternion.Euler(0, 0, 0);
	}
}