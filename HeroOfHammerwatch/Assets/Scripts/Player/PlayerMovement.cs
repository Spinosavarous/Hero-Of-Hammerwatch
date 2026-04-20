using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerMovement : MonoBehaviour
{
	[Header("Movement")]
	[SerializeField] private float moveSpeed = 5f;
	[SerializeField] private float runSpeed = 6.5f;

	[Header("Stats")]
	[SerializeField] private PlayerStats playerStats;
	[SerializeField] private float currentHp;
	[SerializeField] private float attackDamage;

	[Header("Movement UI")]
	[SerializeField] private Slider stamina;
	[SerializeField] private Slider healthBar;

	[SerializeField] private TextMeshProUGUI stamina_text;
	[SerializeField] private TextMeshProUGUI health_text;

	[Header("Damage Text")]
	[SerializeField] private TextMeshPro damageTextPrefab;
	[SerializeField] private Vector3 damageOffset = new Vector3(0, 1.5f, 0);

	[Header("Movement Smoothing")]
	[SerializeField] private float acceleration = 20f;

	[Header("Mobile")]
	[SerializeField] private FixedJoystick joystick;

	[Header("Combat")]
	[SerializeField] private float attackCooldown = 0.4f;
	[SerializeField] private float attackRadius = 1.5f;
	[SerializeField] private float attackForce = 12f;
	[SerializeField] private LayerMask hitLayers;

	[Header("XP")]
	[SerializeField] private Slider xpSlider;
	[SerializeField] private int playerLevel = 1;
	[SerializeField] private TextMeshProUGUI levelText;
	[SerializeField] private float currentXP = 0f;
	[SerializeField] private TextMeshProUGUI xpText;

	private Rigidbody2D rb;
	private Vector2 movement;

	private PlayerInputActions inputActions;
	private bool isMobile;

	private float currentSpeed;

	private Animator animator;
	private bool isAttacking;
	private float attackTimer;
	private ParticleSystem slash;

	private bool sprintLocked = false;


	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();

		slash = GetComponentsInChildren<ParticleSystem>()[0];
		slash.Stop();

		inputActions = new PlayerInputActions();

		isMobile = Application.isMobilePlatform;
		currentSpeed = moveSpeed;

		if (playerStats == null)
		{
			playerStats = new PlayerStats(1);
		}

		currentHp = playerStats.maxHealth;
		stamina.maxValue = playerStats.stamina;
		stamina.value = playerStats.stamina;
		attackDamage = playerStats.attack;

		xpSlider.maxValue = GetXPRequired(playerLevel);
		xpSlider.value = currentXP;
	}

	void OnEnable() => inputActions.Enable();
	void OnDisable() => inputActions.Disable();

	void Update()
	{
		if (isMobile)
		{
			HandleMobileInput();
		}
		else
		{
			joystick.gameObject.SetActive(false);
			HandlePCInput();
		}

		HandleMovementSpeed();
		UpdateAnimator();

		if (attackTimer > 0)
			attackTimer -= Time.deltaTime;

		HandleAttackInput();
		HandleStamina();
	}

	void FixedUpdate()
	{
		Vector2 targetVelocity = movement * currentSpeed;

		rb.linearVelocity = Vector2.MoveTowards(
			rb.linearVelocity,
			targetVelocity,
			acceleration * Time.fixedDeltaTime
		);

		FaceRotationHandling();
	}

	// ---------------- UI ----------------
	private void LateUpdate()
	{
		healthBar.value = currentHp;
		stamina.value = stamina.value;
		stamina_text.text = Mathf.RoundToInt(stamina.value).ToString() + "/" + Mathf.RoundToInt(stamina.maxValue).ToString();
		health_text.text = Mathf.RoundToInt(currentHp).ToString() + "/" + Mathf.RoundToInt(playerStats.maxHealth).ToString();
		xpSlider.value = currentXP;

		if (xpText != null)
		{
			xpText.text =
				Mathf.RoundToInt(currentXP) + " / " +
				Mathf.RoundToInt(xpSlider.maxValue);
		}

		levelText.text = playerLevel.ToString();
	}


	private int GetXPRequired(int level)
	{
		return 50 + (level * level * 15);
	}

	private void LevelUp()
	{
		playerLevel++;

		xpSlider.maxValue = GetXPRequired(playerLevel);

		Debug.Log("Level Up! Level: " + playerLevel);

		playerStats.maxHealth += 10;
		playerStats.attack += 2f;
		playerStats.stamina += 5f;

		currentHp = playerStats.maxHealth;
		healthBar.maxValue = playerStats.maxHealth;

		attackDamage = playerStats.attack;

		stamina.maxValue = playerStats.stamina;
		stamina.value = playerStats.stamina;
	}

	public void GainXP(float amount)
	{
		currentXP += amount;

		while (currentXP >= xpSlider.maxValue)
		{
			currentXP -= xpSlider.maxValue;
			LevelUp();
		}

		xpSlider.value = currentXP;
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("XP"))
		{
			GainXP(other.GetComponent<XpScript>().GetStrength());

			Destroy(other.gameObject);
		}
	}
	// ---------------- INPUT ----------------

	void HandlePCInput()
	{
		movement = inputActions.Player.Move.ReadValue<Vector2>().normalized;
	}

	void HandleMobileInput()
	{
		if (joystick == null)
		{
			movement = Vector2.zero;
			return;
		}

		movement = new Vector2(joystick.Horizontal, joystick.Vertical).normalized;
	}

	// ---------------- ATTACK ----------------

	private void HandleAttackInput()
	{
		if (inputActions.Player.Attack.triggered && attackTimer <= 0 && !isAttacking)
		{
			Attack();
		}
	}

	private void Attack()
	{
		isAttacking = true;
		attackTimer = attackCooldown;

		slash.Play();
		animator.Play("Attack2");

		stamina.value -= 3f;
		stamina.value = Mathf.Clamp(stamina.value, 0, stamina.maxValue);

		HitTargets();

		StartCoroutine(AttackEnd());
	}

	private void HitTargets()
	{
		Collider2D[] hits = Physics2D.OverlapCircleAll(
			transform.position,
			attackRadius,
			hitLayers
		);

		foreach (Collider2D hit in hits)
		{
			if (hit.CompareTag("Obstacle") || hit.CompareTag("Enemy") || hit.CompareTag("Nest"))
			{
				Rigidbody2D targetRb = hit.GetComponent<Rigidbody2D>();
				EnemyMovement enemy = hit.GetComponent<EnemyMovement>();
				EnemySpawner nest = hit.GetComponent<EnemySpawner>();

				if (targetRb != null && !hit.CompareTag("Enemy"))
				{
					Vector2 dir = (hit.transform.position - transform.position).normalized;

					if (dir == Vector2.zero)
						dir = transform.right;

					targetRb.AddForce(dir * attackForce * 20, ForceMode2D.Impulse);
				}

				if (enemy != null)
				{
					enemy.TakeDamage(attackDamage);

					ShowDamageText(
						attackDamage,
						enemy.transform.position + damageOffset
					);
				}

				if (nest != null)
				{
					nest.TakeDamage(attackDamage);
					ShowDamageText(
						attackDamage,
						nest.transform.position + damageOffset
					);
				}
			}
		}
	}

	// ---------------- SHOW DAMAGE TEXT ----------------

	private void ShowDamageText(float damage, Vector3 worldPos)
	{
		if (damageTextPrefab == null)
			return;

		TextMeshPro txt = Instantiate(
			damageTextPrefab,
			worldPos,
			Quaternion.identity
		);

		txt.text = "-" + Mathf.RoundToInt(damage);

		Vector3 randomOffset = new Vector3(
			Random.Range(-0.25f, 0.25f),
			Random.Range(-0.1f, 0.1f),
			0f
		);

		txt.transform.position += randomOffset;

		StartCoroutine(FloatingDamageText(txt));
	}

	private IEnumerator FloatingDamageText(TextMeshPro txt)
	{
		float time = 0f;
		float duration = 1f;

		Color color = txt.color;

		Vector3 start = txt.transform.position;
		Vector3 end = start + Vector3.up * 1.2f;

		while (time < duration)
		{
			time += Time.deltaTime;

			float t = time / duration;

			txt.transform.position =
				Vector3.Lerp(start, end, t);

			color.a = Mathf.Lerp(1f, 0f, t);
			txt.color = color;

			yield return null;
		}

		Destroy(txt.gameObject);
	}

	private IEnumerator AttackEnd()
	{
		yield return new WaitForSeconds(0.3f);
		isAttacking = false;

		yield return new WaitForSeconds(0.5f);
		slash.Stop();
	}

	// ---------------- MOVEMENT SPEED ----------------

	private void HandleMovementSpeed()
	{
		bool isMoving = movement.magnitude > 0.1f;
		bool canSprint = stamina.value > 5;
		bool isSprinting = false;

		if (!isMobile)
			isSprinting = inputActions.Player.Sprint.IsPressed() && isMoving;
		else
			isSprinting = movement.magnitude > 0.9f;

		isSprinting = isSprinting && canSprint;

		currentSpeed = isSprinting ? runSpeed : moveSpeed;
	}

	// ---------------- ANIMATION ----------------

	private void UpdateAnimator()
	{
		bool isMoving = movement.magnitude > 0.1f;
		bool isRunning = currentSpeed == runSpeed && isMoving;
		bool isWalking = isMoving && !isRunning;

		animator.SetBool("IsWalking", isWalking);
		animator.SetBool("IsRunning", isRunning);
	}

	// ---------------- STAMINA ----------------

	private void HandleStamina()
	{
		float percent = stamina.value / stamina.maxValue;

		if (percent <= 0.20f)
			sprintLocked = true;

		if (percent >= 0.50f)
			sprintLocked = false;

		bool isMoving = movement.magnitude >= 0.1f;

		if (!isMoving)
		{
			stamina.value += Time.deltaTime * 10f;
			stamina.value = Mathf.Clamp(stamina.value, 0, stamina.maxValue);
			return;
		}

		if (currentSpeed == runSpeed && !sprintLocked)
		{
			stamina.value -= Time.deltaTime * 30f;
			stamina.value = Mathf.Clamp(stamina.value, 0, stamina.maxValue);
		}
		else
		{
			stamina.value += Time.deltaTime * 10f;
			stamina.value = Mathf.Clamp(stamina.value, 0, stamina.maxValue);

			currentSpeed = moveSpeed;
		}
	}

	// ---------------- ROTATION ----------------

	private void FaceRotationHandling()
	{
		if (movement.x > 0.01f)
			transform.rotation = Quaternion.Euler(0f, 0f, 0f);
		else if (movement.x < -0.01f)
			transform.rotation = Quaternion.Euler(0f, 180f, 0f);
	}

	// ---------------- Take Damage ----------------
	public void TakeDamage(float damage, Vector3 enemy)
	{
		currentHp -= damage;

		StartCoroutine(KnockBackEffect((transform.position - enemy).normalized * damage * 0.5f));

		if (currentHp <= 0)
		{
			currentHp = 0;
			Die();
		}
	}

	private IEnumerator KnockBackEffect(Vector2 force)
	{
		yield return new WaitForSeconds(0.1f);

		rb.AddForce(force, ForceMode2D.Impulse);
	}

	public void Die()
	{
		animator.Play("Death");

		StartCoroutine(WaitDie());
	}

	IEnumerator WaitDie()
	{
		yield return new WaitForSeconds(2f);
		enabled = false;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, attackRadius);
	}
}