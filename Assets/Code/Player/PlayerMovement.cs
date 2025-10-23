using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    // ============================================
    // COMPONENTES
    // ============================================
    private Rigidbody2D rb;
    private PlayerAnimationController animController;

    // ============================================
    // REFERENCIAS
    // ============================================
    [Header("Puntos de Detección")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("Prefabs y Capas")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;

    // ============================================
    // MOVIMIENTO
    // ============================================
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float blockMoveSpeed = 2f;
    [SerializeField] private float sprintMultiplier = 1.5f;    // NUEVO: Multiplicador de velocidad (>1 = corre, <1 = lento)
    [SerializeField] private bool isSprintMode = true;         // NUEVO: true = sprint, false = slow

    private float horizontalInput;
    private bool facingRight = true;

    // ============================================
    // SALTO
    // ============================================
    [Header("Salto")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float doubleJumpForce = 10f;
    [SerializeField] private float fallMultiplier = 3f;        // Aumentado para que se note más
    [SerializeField] private float lowJumpMultiplier = 2.5f;   // Aumentado para que se note más
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.1f;

    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool hasDoubleJumped;

    // ============================================
    // DASH
    // ============================================
    [Header("Dash")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.5f;

    private bool isDashing;
    private float dashTimer;
    private float lastDashTime = -10f;

    // ============================================
    // PARED
    // ============================================
    [Header("Interacción con Paredes")]
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private float wallSlideFastSpeed = 6f;
    [SerializeField] private Vector2 wallJumpForce = new Vector2(10f, 12f);
    [SerializeField] private float wallGravity = 0.5f;
    [SerializeField] private float wallJumpLockTime = 0.2f;    // NUEVO: tiempo para evitar volver a pegarse

    private float originalGravity;
    private bool isFastWallSliding;
    private bool isWallSliding;                                // NUEVO: estado de wall slide
    private float wallJumpLockTimer;                           // NUEVO: timer para lockeo

    // ============================================
    // COMBATE
    // ============================================
    [Header("Combate")]
    [SerializeField] private float attackStepSpeed = 5f;       // Velocidad del empuje
    [SerializeField] private float attackStepDelay = 0.15f;    // Delay antes de empujar (tiempo de windup)
    [SerializeField] private float attackStepDuration = 0.1f;  // Duración del empuje
    [SerializeField] private float attackGroundDuration = 0.4f;
    [SerializeField] private float attackAirDuration = 0.4f;
    [SerializeField] private float attackCooldown = 0.1f;      // NUEVO: Cooldown entre ataques
    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private float damageRecoveryTime = 0.5f;

    private int currentCombo;
    private bool isAttacking;
    private float attackDelayTimer;
    private float attackMoveTimer;
    private bool attackStepActive;
    private float lastAttackTime = -10f;                       // NUEVO: Tiempo del último ataque

    // ============================================
    // PROYECTILES
    // ============================================
    [Header("Proyectiles")]
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float projectileCooldown = 0.5f;

    private float lastProjectileTime;

    // ============================================
    // HABILIDADES
    // ============================================
    [Header("Habilidades Desbloqueables")]
    public bool canMove = true;
    public bool canJump = true;
    public bool canDoubleJump = true;
    public bool canAttack = true;
    public bool canDash = true;
    public bool canWallCling = true;
    public bool canBlock = true;
    public bool canThrowProjectile = true;

    // ============================================
    // DETECCION
    // ============================================
    [Header("Detección")]
    [SerializeField] private float groundCheckRay = 0.2f;
    [SerializeField] private float wallCheckDistance = 0.5f;
    [SerializeField] private float inputDeadzone = 0.1f;

    private bool isGrounded;
    private bool isTouchingWall;
    private bool wasGrounded;

    // ============================================
    // ESTADO
    // ============================================
    [Header("Estado")]
    [SerializeField] private bool isTakingDamage;

    // ============================================
    // UNITY CALLBACKS
    // ============================================

    private void Start()
    {
        InitializeComponents();
        CreateDetectionPoints();
    }

    private void Update()
    {
        if (isTakingDamage) return;

        CaptureInput();
        UpdateDetectionStates();
        UpdateJumpTimers();
        UpdateWallJumpLock();
        UpdateAttackStepTimer(); // NUEVO: actualizar timer de empuje

        if (!isDashing)
        {
            HandleAllActions();
        }
        else
        {
            UpdateDashTimer();
        }

        ApplyBetterFalling();
    }

    private void FixedUpdate()
    {
        if (isTakingDamage) return;

        bool isBlocking = Input.GetKey(KeyCode.X);
        bool isHoldingCtrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        // CORREGIDO: Empuje de ataque con DELAY - solo se activa después del windup
        if (attackStepActive && attackMoveTimer > 0)
        {
            float direction = facingRight ? 1f : -1f;
            rb.linearVelocity = new Vector2(direction * attackStepSpeed, rb.linearVelocity.y);
            attackMoveTimer -= Time.fixedDeltaTime;

            if (attackMoveTimer <= 0)
            {
                attackStepActive = false;
            }
        }
        // Movimiento normal con sprint/slow
        else if (!isDashing && canMove && !isAttacking && !isBlocking)
        {
            float finalSpeed = moveSpeed;

            // Sistema de Sprint/Slow con Ctrl
            if (isHoldingCtrl)
            {
                if (isSprintMode)
                {
                    // Modo Sprint: Ctrl multiplica velocidad
                    finalSpeed = moveSpeed * sprintMultiplier;
                }
                else
                {
                    // Modo Slow: Ctrl divide velocidad
                    finalSpeed = moveSpeed / sprintMultiplier;
                }
            }

            ApplyMovement(horizontalInput * finalSpeed);
        }
        // Bloqueo con movimiento lento
        else if (isBlocking && !isAttacking)
        {
            ApplyMovement(horizontalInput * blockMoveSpeed);
        }
        // Si estamos atacando pero NO en empuje, frenar el movimiento horizontal
        else if (isAttacking && !attackStepActive)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Finish"))
        {
            SceneManager.LoadScene("Victory");
        }
    }

    // ============================================
    // INICIALIZACION
    // ============================================

    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        animController = GetComponent<PlayerAnimationController>();

        if (animController == null)
        {
            animController = gameObject.AddComponent<PlayerAnimationController>();
        }

        animController.Initialize(this);
        originalGravity = rb.gravityScale;
    }

    private void CreateDetectionPoints()
    {
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = groundCheckObj.transform;
        }

        if (wallCheck == null)
        {
            GameObject wallCheckObj = new GameObject("WallCheck");
            wallCheckObj.transform.SetParent(transform);
            wallCheckObj.transform.localPosition = new Vector3(0.3f, 0, 0);
            wallCheck = wallCheckObj.transform;
        }

        if (projectileSpawnPoint == null)
        {
            GameObject spawnObj = new GameObject("ProjectileSpawn");
            spawnObj.transform.SetParent(transform);
            spawnObj.transform.localPosition = new Vector3(0.5f, 0.2f, 0);
            projectileSpawnPoint = spawnObj.transform;
        }
    }

    // ============================================
    // INPUT
    // ============================================

    private void CaptureInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
    }

    // ============================================
    // DETECCION DE ESTADOS
    // ============================================

    private void UpdateDetectionStates()
    {
        wasGrounded = isGrounded;
        isGrounded = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckRay, groundLayer);

        // CORREGIDO: Solo detectar pared si NO estamos en lockeo de wall jump
        Vector2 wallDirection = facingRight ? Vector2.right : Vector2.left;
        isTouchingWall = wallJumpLockTimer <= 0 &&
                        Physics2D.Raycast(wallCheck.position, wallDirection, wallCheckDistance, wallLayer);

        // Reset de doble salto y fast slide solo cuando realmente tocamos suelo
        if (isGrounded && !wasGrounded)
        {
            hasDoubleJumped = false;
            isFastWallSliding = false;
        }
    }

    private void UpdateJumpTimers()
    {
        coyoteTimeCounter = isGrounded ? coyoteTime : coyoteTimeCounter - Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }

    private void UpdateWallJumpLock()
    {
        if (wallJumpLockTimer > 0)
        {
            wallJumpLockTimer -= Time.deltaTime;
        }
    }

    // ============================================
    // ACCIONES
    // ============================================

    private void HandleAllActions()
    {
        if (canMove) HandleMovement();
        if (canWallCling) HandleWallCling(); // ORDEN IMPORTANTE: antes del salto
        if (canJump) HandleJump();
        if (canAttack) HandleAttack();
        if (canDash) HandleDash();
        if (canThrowProjectile) HandleProjectile();
    }

    // ============================================
    // MOVIMIENTO
    // ============================================

    private void HandleMovement()
    {
        if (isAttacking) return;
        FlipCharacter(horizontalInput);
    }

    private void ApplyMovement(float speed)
    {
        rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
    }

    private void FlipCharacter(float direction)
    {
        if (direction > 0 && !facingRight)
        {
            Flip();
        }
        else if (direction < 0 && facingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // ============================================
    // SALTO
    // ============================================

    private void HandleJump()
    {
        bool wantsToJump = jumpBufferCounter > 0f;

        if (wantsToJump)
        {
            // 1️⃣ Salto en pared (PRIORIDAD MÁXIMA para evitar conflictos)
            if (isWallSliding && isTouchingWall)
            {
                PerformWallJump();
                jumpBufferCounter = 0f;
            }
            // 2️⃣ Salto normal o con coyote time
            else if (isGrounded || coyoteTimeCounter > 0f)
            {
                PerformJump(jumpForce);
                jumpBufferCounter = 0f;
                hasDoubleJumped = false;
                coyoteTimeCounter = 0f;
            }
            // 3️⃣ Doble salto (solo si ya estás en el aire Y no estás en pared)
            else if (!isGrounded && !hasDoubleJumped && canDoubleJump && !isTouchingWall)
            {
                PerformDoubleJump();
                jumpBufferCounter = 0f;
            }
        }
    }

    // CORREGIDO: Salto normal con reset de velocidad Y
    private void PerformJump(float force)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
    }

    // NUEVO: Doble salto con reset COMPLETO de velocidad Y para que funcione bien al caer
    private void PerformDoubleJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // Reset crucial
        rb.AddForce(Vector2.up * doubleJumpForce, ForceMode2D.Impulse);
        hasDoubleJumped = true;
        animController.TriggerDoubleJump();
    }

    private void PerformWallJump()
    {
        float jumpDirection = facingRight ? -1f : 1f;
        rb.linearVelocity = new Vector2(jumpDirection * wallJumpForce.x, wallJumpForce.y);
        hasDoubleJumped = false;
        isWallSliding = false;
        wallJumpLockTimer = wallJumpLockTime; // Activar lockeo
    }

    // CORREGIDO: Mejor detección de caída y salto corto
    private void ApplyBetterFalling()
    {
        // No aplicar si estamos en wall slide
        if (isWallSliding) return;

        // Caída rápida
        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = fallMultiplier;
        }
        // Salto corto (soltar espacio)
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            rb.gravityScale = lowJumpMultiplier;
        }
        // Gravedad normal
        else
        {
            rb.gravityScale = originalGravity;
        }
    }

    // ============================================
    // DASH
    // ============================================

    private void HandleDash()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time > lastDashTime + dashCooldown)
        {
            isDashing = true;
            dashTimer = dashDuration;
            lastDashTime = Time.time;
        }

        if (isDashing)
        {
            float dashDirection = facingRight ? 1f : -1f;
            rb.linearVelocity = new Vector2(dashDirection * dashSpeed, rb.linearVelocity.y);
        }
    }

    private void UpdateDashTimer()
    {
        dashTimer -= Time.deltaTime;
        if (dashTimer <= 0)
        {
            isDashing = false;
        }
    }

    // ============================================
    // PARED
    // ============================================

    private void HandleWallCling()
    {
        // CORREGIDO: Solo hacer wall slide si:
        // 1. No estamos en suelo
        // 2. Tocamos pared
        // 3. Estamos presionando HACIA la pared
        // 4. No estamos en lockeo de wall jump
        bool isPressingTowardsWall = Mathf.Abs(horizontalInput) > inputDeadzone &&
                                     Mathf.Sign(horizontalInput) == (facingRight ? 1 : -1);

        isWallSliding = !isGrounded && isTouchingWall && isPressingTowardsWall && wallJumpLockTimer <= 0;

        if (isWallSliding)
        {
            // Resetear doble salto mientras nos deslizamos
            hasDoubleJumped = false;

            // Deslizamiento rápido o normal
            if (isFastWallSliding)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideFastSpeed);
            }
            else
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x,
                    Mathf.Clamp(rb.linearVelocity.y, -wallSlideSpeed, float.MaxValue));
            }

            rb.gravityScale = wallGravity;

            // Fast slide
            if (Input.GetKeyDown(KeyCode.S))
            {
                StartCoroutine(FastWallSlideCoroutine());
            }
        }
        else
        {
            // Solo restaurar gravedad si no estamos en wall slide
            if (!isWallSliding && rb.gravityScale == wallGravity)
            {
                rb.gravityScale = originalGravity;
            }
            isFastWallSliding = false;
        }
    }

    private IEnumerator FastWallSlideCoroutine()
    {
        isFastWallSliding = true;
        yield return new WaitForSeconds(dashDuration);
        isFastWallSliding = false;
    }

    // ============================================
    // COMBATE
    // ============================================

    private void UpdateAttackStepTimer()
    {
        // Si estamos atacando y el delay no ha terminado
        if (isAttacking && attackDelayTimer > 0)
        {
            attackDelayTimer -= Time.deltaTime;

            // Cuando el delay termina, activar el empuje
            if (attackDelayTimer <= 0 && !attackStepActive)
            {
                attackStepActive = true;
                attackMoveTimer = attackStepDuration;
            }
        }
    }

    private void HandleAttack()
    {
        // CORREGIDO: Verificar tanto isAttacking como el cooldown
        if (isAttacking || Time.time < lastAttackTime + attackCooldown) return;

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (isGrounded)
            {
                // Sistema de combo alternado: 1 -> 2 -> 1 -> 2
                currentCombo = (currentCombo == 1) ? 2 : 1;
                StartAttack(currentCombo);
            }
            else
            {
                // Ataque aéreo (siempre combo 1)
                StartAttack(1);
            }
        }
    }

    private void StartAttack(int comboIndex)
    {
        isAttacking = true;
        lastAttackTime = Time.time; // NUEVO: Registrar tiempo del ataque
        animController.SetComboIndex(comboIndex);

        // Reiniciar timers
        attackDelayTimer = attackStepDelay;
        attackStepActive = false;
        attackMoveTimer = 0;

        float duration = isGrounded ? attackGroundDuration : attackAirDuration;
        StartCoroutine(AttackCoroutine(duration));
    }

    private IEnumerator AttackCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        StopAttack();
    }

    private void StopAttack()
    {
        isAttacking = false;
        attackDelayTimer = 0;
        attackMoveTimer = 0;
        attackStepActive = false;
        // NO resetear currentCombo aquí - mantiene el estado para el siguiente ataque
    }

    // Este método ya no hace nada, pero lo dejamos por compatibilidad con animaciones
    public void OnAttackHitFrame()
    {
        // El empuje ahora se maneja en FixedUpdate con attackMoveTimer
    }

    public void TakeDamage(Vector2 attackerPosition)
    {
        isTakingDamage = true;
        animController.TriggerDamage();

        Vector2 knockbackDirection = ((Vector2)transform.position - attackerPosition).normalized;
        float minKnockbackY = 0.5f;
        float maxKnockbackY = 1f;
        knockbackDirection.y = Mathf.Clamp(knockbackDirection.y + minKnockbackY, minKnockbackY, maxKnockbackY);

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        StartCoroutine(DamageRecoveryCoroutine());
    }

    private IEnumerator DamageRecoveryCoroutine()
    {
        yield return new WaitForSeconds(damageRecoveryTime);
        isTakingDamage = false;
        animController.StopDamage();
    }

    // ============================================
    // PROYECTILES
    // ============================================

    private void HandleProjectile()
    {
        if (Input.GetKeyDown(KeyCode.C) && Time.time > lastProjectileTime + projectileCooldown)
        {
            if (projectilePrefab != null && projectileSpawnPoint != null)
            {
                lastProjectileTime = Time.time;

                GameObject projectile = Instantiate(projectilePrefab,
                    projectileSpawnPoint.position, Quaternion.identity);

                Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
                if (projectileRb != null)
                {
                    float direction = facingRight ? 1f : -1f;
                    projectileRb.linearVelocity = new Vector2(direction * projectileSpeed, 0);
                }

                animController.TriggerThrow();
            }
        }
    }

    // ============================================
    // PROPIEDADES PUBLICAS (para animaciones)
    // ============================================

    public bool IsGrounded => isGrounded;
    public bool IsTouchingWall => isTouchingWall;
    public bool IsAttacking => isAttacking;
    public bool IsDashing => isDashing;
    public bool IsTakingDamage => isTakingDamage;
    public float HorizontalInput => horizontalInput;
    public float VerticalVelocity => rb.linearVelocity.y;
    public bool IsBlocking => Input.GetKey(KeyCode.X);
    public bool IsWallSliding => isWallSliding; // NUEVO: para animaciones
    public bool IsSprinting => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl); // NUEVO: para animaciones

    // ============================================
    // GIZMOS
    // ============================================

    private void OnDrawGizmosSelected()
    {
        // Raycast de suelo
        if (groundCheck != null)
        {
            Vector2 origin = groundCheck.position;
            Vector2 direction = Vector2.down;
            float distance = groundCheckRay;

            RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, groundLayer);
            Gizmos.color = hit.collider != null ? Color.green : Color.red;
            Gizmos.DrawLine(origin, origin + direction * distance);
        }

        // Raycast de pared
        if (wallCheck != null)
        {
            Vector2 dir = facingRight ? Vector2.right : Vector2.left;
            bool wall = Physics2D.Raycast(wallCheck.position, dir, wallCheckDistance, wallLayer);
            Gizmos.color = wall ? Color.blue : Color.yellow;
            Gizmos.DrawRay(wallCheck.position, (Vector3)dir * wallCheckDistance);
        }
    }
}