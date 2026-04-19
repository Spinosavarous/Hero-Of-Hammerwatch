using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStats", menuName = "Game/Enemy Stats")]
public class EnemyStats : ScriptableObject
{
	[Header("Base Stats")]
	public string enemyName;

	public float maxHp = 100f;
	public float moveSpeed = 3f;
	public float damage = 10f;

	[Header("Combat")]
	public float attackRange = 1.5f;
	public float attackCooldown = 1f;

	[Header("Knockback")]
	public float mass = 1f;
	public float knockbackResistance = 0f;

	[Header("Rewards")]
	public int xpReward = 10;
	public int goldReward = 5;
}
