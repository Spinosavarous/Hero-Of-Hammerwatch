using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private EnemyStats stats;

    private Rigidbody2D rb;

    [SerializeField] private float currentHp;
    private float attackTimer;

	private EnemySpawner spawner;
	private bool isDead = false;
	void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        currentHp = stats.maxHp;
        rb.mass = stats.mass;
	}

	void Update()
	{
		if (transform.tag == "Untagged")
			return;

		if (attackTimer > 0)
			attackTimer -= Time.deltaTime;
	}

	public void SetSpawner(EnemySpawner nest)
	{
		spawner = nest;
	}

	public void TakeDamage(float damage)
	{
		currentHp -= damage;
		StartCoroutine(AttackKnockBack((transform.position - GameObject.FindGameObjectWithTag("Player").transform.position).normalized * damage * 0.5f));

		if (currentHp <= 0)
			Die();
	}

	public void Knockback(Vector2 force)
	{
		float resistance = 1f - stats.knockbackResistance;
		rb.AddForce(force * resistance, ForceMode2D.Impulse);
	}

	public bool CanAttack()
	{
		return attackTimer <= 0;
	}

	public void ResetAttackCooldown()
	{
		attackTimer = stats.attackCooldown;
	}

	private void Die()
	{
		if (isDead)
			return;

		isDead = true;

		if (spawner != null)
			spawner.EnemyDied();

		Destroy(gameObject);
	}

	public EnemyStats GetStats()
	{
		return stats;
	}

	IEnumerator AttackKnockBack(Vector2 force)
	{
		yield return new WaitForSeconds(0.1f);
		Knockback(force);
	}
}
