// ===============================
// EnemyMovement.cs
// ===============================

using UnityEngine;
using System.Collections;

[System.Serializable]
public class EnemySaveData
{
	public string id;
	public float hp;
	public float posX;
	public float posY;
	public bool dead;
}

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMovement : MonoBehaviour
{
	[SerializeField] private EnemyStats stats;

	[Header("Drops")]
	[SerializeField] private GameObject xpPrefab;
	[SerializeField] private GameObject goldPrefab;

	[Header("Effects")]
	[SerializeField] private ParticleSystem bloodEffect;

	private Rigidbody2D rb;

	private EnemySpawner spawner;

	public string enemyId;

	public float currentHp;
	private float attackTimer;

	private bool isDead = false;

	void Start()
	{
		rb = GetComponent<Rigidbody2D>();

		currentHp = stats.maxHp;
		rb.mass = stats.mass;
	}

	void OnDisable()
	{
		SaveEnemy();
	}

	// ---------------- SETTERS ----------------

	public void SetSpawner(EnemySpawner nest)
	{
		spawner = nest;
	}

	public void SetEnemyId(string id)
	{
		enemyId = id;
	}

	public bool IsDead()
	{
		return isDead;
	}

	public bool CanAttack()
	{
		return attackTimer <= 0;
	}

	public EnemyStats GetStats()
	{
		return stats;
	}

	public void ResetAttackCooldown()
	{
		attackTimer = stats.attackCooldown;
	}

	// ---------------- DAMAGE ----------------

	public void TakeDamage(float damage)
	{
		if (isDead) return;

		currentHp -= damage;

		if (bloodEffect != null)
		{
			ParticleSystem p =
				Instantiate(
					bloodEffect,
					transform.position,
					Quaternion.identity
				);

			p.Play();

			Destroy(p.gameObject, 2f);
		}

		StartCoroutine(AttackKnockBack((transform.position - GameObject.FindGameObjectWithTag("Player").transform.position).normalized * damage * 0.5f));


		if (currentHp <= 0)
			Die();

		SaveEnemy();
	}

	public void Knockback(Vector2 force)
	{
		float resistance = 1f - stats.knockbackResistance;
		rb.AddForce(force * resistance, ForceMode2D.Impulse);
	}

	void Die()
	{
		if (isDead) return;

		isDead = true;

		SpawnXP();
		SpawnGold();

		if (spawner != null)
			spawner.EnemyDied();

		SaveEnemy();

		Destroy(gameObject);
	}

	// ---------------- DROPS ----------------

	void SpawnXP()
	{
		if (xpPrefab == null) return;

		for (int i = 0; i < stats.xpReward; i++)
		{
			Instantiate(
				xpPrefab,
				transform.position,
				Quaternion.identity
			);
		}
	}

	void SpawnGold()
	{
		if (goldPrefab == null) return;

		for (int i = 0; i < stats.goldReward; i++)
		{
			Instantiate(
				goldPrefab,
				transform.position,
				Quaternion.identity
			);
		}
	}

	// ---------------- SAVE ----------------

	public void SaveEnemy()
	{
		if (string.IsNullOrEmpty(enemyId))
			return;

		EnemySaveData data =
			new EnemySaveData();

		data.id = enemyId;
		data.hp = currentHp;
		data.posX = transform.position.x;
		data.posY = transform.position.y;
		data.dead = isDead;

		string json =
			JsonUtility.ToJson(data);

		PlayerPrefs.SetString(
			"enemy_" + enemyId,
			json
		);
	}

	IEnumerator AttackKnockBack(Vector2 force)
	{
		yield return new WaitForSeconds(0.1f);
		Knockback(force);
	}
}