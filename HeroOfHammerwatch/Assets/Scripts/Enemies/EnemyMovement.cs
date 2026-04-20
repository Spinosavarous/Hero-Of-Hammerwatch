using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private EnemyStats stats;

	[Header("Blood Effect")]
	[SerializeField] private ParticleSystem bloodEffect;

	[Header("XP")]
	[SerializeField] private GameObject xpPrefab;

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
		ParticleSystem p = Instantiate(bloodEffect, transform.position, Quaternion.identity);
		p.Play();

		Destroy(p.gameObject, 2);
		
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

		SpawnXP();

		if (spawner != null)
			spawner.EnemyDied();

		Destroy(gameObject);
	}

	private void SpawnXP()
	{
		for (int i = 0; i < stats.xpReward; i++ )
		{
			GameObject xp = Instantiate(xpPrefab, transform.position, Quaternion.identity);

			xp.tag = "XP";

			Rigidbody2D rb = xp.GetComponent<Rigidbody2D>();
			if (rb != null)
			{
				Vector2 randomDirection = Random.insideUnitCircle.normalized;

				float forceAmount = Random.Range(3f, 8f);

				rb.AddForce(randomDirection * forceAmount, ForceMode2D.Impulse);
			}
		}
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
