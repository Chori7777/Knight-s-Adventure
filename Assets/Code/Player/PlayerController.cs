using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Componentes")]
    private Rigidbody2D rb;
    private Animator anim;

    [Header("Movimiento")]
    public float speed = 5f;
    public float fuerzaSalto = 12f;
    public float fuerzaSaltoDoble = 10f;
    public float impulseForce = 10f;
    public float dashSpeed = 20f;
    public float dashTime = 0.15f;
    public float dashCooldown = 0.5f;
    public float wallSlideSpeed = 2f;

    [Header("Detección")]
    public float longitudRayo = 0.1f;
    public LayerMask capaSuelo;
    public LayerMask capaPared;

    [Header("Opciones Activables")]
    public bool puedeMoverse = true;
    public bool puedeSaltar = true;
    public bool puedeAtacar = true;
    public bool puedeDash = true;
    public bool puedeWallCling = true;
    public bool puedeBloquear = true;

    [Header("Wall Climbing")]
    public float wallGravity = 0.5f;
    private float originalGravity;

    [Header("Salto Variable")]
    public float fallMultiplier = 2.5f; // 🔥 Multiplicador de caída rápida
    public float lowJumpMultiplier = 2f; // 🔥 Multiplicador para saltos cortos

    // Variables internas
    private int jumpCount = 0;
    private int maxJumps = 2;
    private bool estabaEnSuelo = false;
    private float lastDashTime = -10f;
    private float currentAttack = 0;
    private float lastAttackTime = 0f;
    public float comboResetTime = 1f;
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float horizontalInput;
    public bool recibiodaño = false;
    private bool isAttacking = false;

    [Header("Bloqueo")]
    public float shieldMoveSpeed = 2f; // velocidad reducida al bloquear

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        originalGravity = rb.gravityScale;
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (!isDashing)
        {
            if (puedeMoverse) Movimiento();
            if (puedeSaltar) Saltar();
            if (puedeAtacar) Attack();
            if (puedeDash) Dash();
            if (puedeWallCling) WallCling();
            if (puedeBloquear) Bloqueo();
        }
        else
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                isDashing = false;
                StopDashing();
            }
        }

        AplicarCaidaRapida();
        ActualizarAnimaciones();
    }

    void FixedUpdate()
    {
        // 🔸 Movimiento normal
        if (!isDashing && puedeMoverse && !isAttacking && !anim.GetBool("isBlocking"))
        {
            rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocity.y);
        }
        // 🔸 Movimiento reducido al bloquear
        else if (anim.GetBool("isBlocking") && !isAttacking)
        {
            rb.linearVelocity = new Vector2(horizontalInput * shieldMoveSpeed, rb.linearVelocity.y);
        }
        // 🔸 Si está atacando, se queda quieto
        else if (isAttacking)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }


    private void AplicarCaidaRapida()
    {
        if (rb.linearVelocity.y < 0)
        {
            // Caída normal (más rápida)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            // Salto corto (soltó espacio rápido)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    private void Movimiento()
    {
        if (isAttacking) return; // Evita que se mueva durante el ataque

        // Flip sprite
        if (horizontalInput > 0)
            transform.localScale = Vector3.one;
        else if (horizontalInput < 0)
            transform.localScale = new Vector3(-1, 1, 1);

        anim.SetFloat("Movement", Mathf.Abs(horizontalInput));
    }

    private void Saltar()
    {
        bool enSuelo = EstaEnSuelo();
        bool enPared = Physics2D.Raycast(transform.position, Vector2.right * transform.localScale.x, longitudRayo, capaPared);

        if (enSuelo && !estabaEnSuelo)
        {
            jumpCount = 0;
            anim.SetBool("Jump", false);
        }

        estabaEnSuelo = enSuelo;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (jumpCount < maxJumps)
            {
                float fuerza = (jumpCount == 0) ? fuerzaSalto : fuerzaSaltoDoble;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
                rb.AddForce(Vector2.up * fuerza, ForceMode2D.Impulse);
                jumpCount++;
                anim.SetBool("Jump", true);

                if (jumpCount == 2)
                    anim.SetTrigger("DoubleJump");
            }
            else if (!enSuelo && enPared)
            {
                float wallJumpDir = -transform.localScale.x;
                rb.linearVelocity = new Vector2(wallJumpDir * speed, fuerzaSalto);
                transform.localScale = new Vector3(Mathf.Sign(wallJumpDir), 1, 1);
                jumpCount = 1;
                anim.SetTrigger("Jump");
            }
        }
    }

    private void ActualizarAnimaciones()
    {
        anim.SetFloat("SpeedY", rb.linearVelocity.y);
        anim.SetBool("Falling", !EstaEnSuelo() && rb.linearVelocity.y < -0.1f);
        anim.SetBool("Grounded", EstaEnSuelo());
    }

    private bool EstaEnSuelo()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, longitudRayo, capaSuelo);
        return hit.collider != null;
    }

    private void WallCling()
    {
        bool enPared = Physics2D.Raycast(transform.position, Vector2.right * transform.localScale.x, longitudRayo, capaPared);

        if (!EstaEnSuelo() && enPared && Input.GetAxisRaw("Horizontal") == transform.localScale.x)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.y, Mathf.Clamp(rb.linearVelocity.y, -wallSlideSpeed, float.MaxValue));
            rb.gravityScale = wallGravity;
            anim.SetBool("WallCling", true);
            jumpCount = 1;
        }
        else
        {
            rb.gravityScale = originalGravity;
            anim.SetBool("WallCling", false);
        }
    }

    private void Dash()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time > lastDashTime + dashCooldown)
        {
            isDashing = true;
            dashTimer = dashTime;
            lastDashTime = Time.time;
            anim.SetBool("Dash", true);
        }

        if (isDashing)
        {
            float dashDir = transform.localScale.x;
            rb.linearVelocity = new Vector2(dashDir * dashSpeed, 0);
        }
    }

    public void StopDashing()
    {
        anim.SetBool("Dash", false);
    }

    private void Attack()
    {
        // 🔸 Solo ataca si está en el suelo y no está atacando ya
        if (!EstaEnSuelo() || isAttacking) return;

        if (Input.GetKeyDown(KeyCode.K))
        {
            isAttacking = true;
            puedeMoverse = false;
            rb.linearVelocity = Vector2.zero;

            // Reiniciar combo si pasó mucho tiempo desde el último ataque
            if (Time.time - lastAttackTime > comboResetTime)
                currentAttack = 0;

            currentAttack++;
            if (currentAttack > 4) // suponiendo 4 ataques en el combo
                currentAttack = 1;

            anim.SetBool("isAttacking", true);
            anim.SetFloat("AttackIndex", currentAttack);

            lastAttackTime = Time.time;
        }
    }

    // Llamado al final de cada animación de ataque (Animation Event)
    public void stopAttacking()
    {
        anim.SetBool("isAttacking", false);
        isAttacking = false;
        puedeMoverse = true;
    }

    public void Bloqueo()
    {
        if (puedeBloquear)
        {
            if (Input.GetKey(KeyCode.J))
            {
                anim.SetBool("isBlocking", true);
            }
            else
            {
                anim.SetBool("isBlocking", false);
            }
        }
    }

    // 🔸 Mejor knockback direccional
    public void RecibirDaño(Vector2 atacantePosicion)
    {
        recibiodaño = true;
        Vector2 knockDir = (transform.position - (Vector3)atacantePosicion).normalized;
        knockDir.y = Mathf.Clamp(knockDir.y + 0.5f, 0.5f, 1f); // fuerza vertical suave
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(knockDir * impulseForce, ForceMode2D.Impulse);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * longitudRayo);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * transform.localScale.x * longitudRayo);
    }
}