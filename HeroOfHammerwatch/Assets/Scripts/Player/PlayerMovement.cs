using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerMovement : MonoBehaviour
{
	[Header("Movement")]
	[SerializeField] private float moveSpeed = 5f;
	[SerializeField] private float runSpeed = 6.5f;

	[Header("Movement UI")]
	[SerializeField] private Slider stamina;

	[Header("Movement Smoothing")]
	[SerializeField] private float acceleration = 20f;

	[Header("Mobile")]
	[SerializeField] private FixedJoystick joystick;

	[Header("Combat")]
	[SerializeField] private float attackCooldown = 0.4f;
	[SerializeField] private float attackRadius = 1.5f;
	[SerializeField] private float attackForce = 12f;
	[SerializeField] private LayerMask hitLayers;

	private Rigidbody2D rb;
	private Vector2 movement;

	private PlayerInputActions inputActions;
	private bool isMobile;

	private float currentSpeed;

	private Animator animator;
	private bool isAttacking;
	private float attackTimer;
	private ParticleSystem slash;

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();

		slash = GetComponentsInChildren<ParticleSystem>()[0];
		slash.Stop();

		inputActions = new PlayerInputActions();

		isMobile = Application.isMobilePlatform;
		currentSpeed = moveSpeed;
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
		HandleAttackInput();
		HandleStamina();
	}

	void FixedUpdate()
	{
		if (isAttacking)
		{
			rb.linearVelocity = Vector2.zero;
			return;
		}

		Vector2 targetVelocity = movement * currentSpeed;

		rb.linearVelocity = Vector2.MoveTowards(
			rb.linearVelocity,
			targetVelocity,
			acceleration * Time.fixedDeltaTime
		);

		FaceRotationHandling();
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
		if (attackTimer > 0)
		{
			attackTimer -= Time.deltaTime;
			return;
		}

		if (inputActions.Player.Attack.triggered && !isAttacking)
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
			if (hit.CompareTag("Obstacle") || hit.CompareTag("Enemy"))
			{
				Rigidbody2D targetRb = hit.GetComponent<Rigidbody2D>();

				if (targetRb != null)
				{
					Vector2 dir = (hit.transform.position - transform.position).normalized;

					if (dir == Vector2.zero)
						dir = transform.right;

					targetRb.AddForce(dir * attackForce, ForceMode2D.Impulse);
				}
			}
		}
	}

	private IEnumerator AttackEnd()
	{
		yield return new WaitForSeconds(0.8f);
		isAttacking = false;
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
		if (movement.magnitude < 0.1f)
		{
			stamina.value += Time.deltaTime * 10f;
			stamina.value = Mathf.Clamp(stamina.value, 0, stamina.maxValue);
			return;
		}

		if (currentSpeed == runSpeed)
		{
			stamina.value -= Time.deltaTime * 30f;

			if (stamina.value <= 0)
				currentSpeed = moveSpeed;
		}
		else
		{
			stamina.value += Time.deltaTime * 10f;
			stamina.value = Mathf.Clamp(stamina.value, 0, stamina.maxValue);
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

	// Show attack radius in editor
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, attackRadius);
	}
}