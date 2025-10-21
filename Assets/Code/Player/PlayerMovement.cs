using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    // COMPONENTES BASE
    // Rigidbody2D: controla la fisica del jugador (velocidad, gravedad, fuerzas)
    private Rigidbody2D rb;
    // Animator: controla las animaciones del jugador
    private Animator anim;

    [Header("Componentes")]
    // Transform que marca donde se detecta el suelo (abajo del jugador)
    [SerializeField] private Transform groundCheck;
    // Transform que marca donde se detecta la pared (al costado del jugador)
    [SerializeField] private Transform wallCheck;
    // Transform que marca donde aparecen los proyectiles al lanzarlos
    [SerializeField] private Transform projectileSpawnPoint;
    // Prefab del proyectil que se va a instanciar
    [SerializeField] private GameObject projectilePrefab;

    [Header("Movimiento")]
    // Velocidad de movimiento horizontal
    public float speed = 5f;
    // Fuerza del primer salto
    public float fuerzaSalto = 12f;
    // Fuerza del segundo salto (doble salto)
    public float fuerzaSaltoDoble = 10f;
    // Fuerza del empuje al recibir daño (knockback)
    public float impulseForce = 10f;
    // Velocidad del dash (desplazamiento rapido)
    public float dashSpeed = 20f;
    // Duracion del dash en segundos
    public float dashTime = 0.15f;
    // Tiempo de espera entre dashes
    public float dashCooldown = 0.5f;
    // Velocidad de deslizamiento por la pared
    public float wallSlideSpeed = 2f;

    [Header("Detección")]
    // Longitud del rayo para detectar suelo y paredes
    public float longitudRayo = 0.1f;
    // Capa que representa el suelo (se configura en Unity)
    public LayerMask capaSuelo;
    // Capa que representa las paredes
    public LayerMask capaPared;

    [Header("Wall Climbing")]
    // Gravedad reducida al estar en la pared (0.5 = mitad de gravedad normal)
    public float wallGravity = 0.5f;
    // Guarda la gravedad original para restaurarla despues
    private float originalGravity;

    [Header("Wall Jump")]
    // Fuerza del salto de pared (x = horizontal, y = vertical)
    public Vector2 wallJumpForce = new Vector2(10f, 12f);

    [Header("Wall Slide Derrape")]
    // Velocidad rapida de deslizamiento al presionar S
    public float wallSlideFastSpeed = 6f;
    // Tecla para activar deslizamiento rapido
    public KeyCode wallSlideKey = KeyCode.S;
    // Flag que indica si esta haciendo deslizamiento rapido
    private bool isFastWallSliding = false;

    [Header("Salto Caida")]
    // Multiplicador de gravedad al caer (hace que caiga mas rapido)
    public float fallMultiplier = 2.5f;
    // Multiplicador al soltar espacio (salto mas bajo si no mantenes la tecla)
    public float lowJumpMultiplier = 2f;

    [Header("Habilidades Activables")]
    // Flags booleanos que activan o desactivan habilidades
    public bool puedeMoverse = true;
    public bool puedeSaltar = true;
    public bool puedeDobleSalto = true; // Controla si puede hacer doble salto
    public bool puedeAtacar = true;
    public bool puedeDash = true;
    public bool puedeWallCling = true;
    public bool puedeBloquear = true;
    public bool puedeLanzar = true;

    [Header("Sistema de Combate")]
    // Fuerza del empuje hacia adelante al atacar
    public float attackStepForce = 3f;
    // Duracion de cada animacion de ataque (ajustar segun tus animaciones)
    public float attackGroundDuration = 0.4f;
    public float attackAirDuration = 0.4f;

    // VARIABLES PRIVADAS DE COMBATE
    // Contador de ataques en el combo actual (0, 1 o 2)
    private int currentCombo = 0;
    // Flag que indica si esta ejecutando un ataque
    private bool isAttacking = false;

    [Header("Proyectiles")]
    // Velocidad horizontal del proyectil
    public float projectileSpeed = 10f;
    // Tiempo de espera entre lanzamientos
    public float projectileCooldown = 0.5f;
    // Ultimo momento en que se lanzo un proyectil
    private float lastProjectileTime = 0f;

    [Header("Variables Internas")]
    // Contador de saltos realizados (0, 1 o 2)
    private int jumpCount = 0;
    // Cantidad maxima de saltos permitidos
    private int maxJumps = 2;
    // Flag que indica si estaba en el suelo en el frame anterior
    private bool estabaEnSuelo = false;
    // Ultimo momento en que se hizo dash
    private float lastDashTime = -10f;
    // Flag que indica si esta dasheando actualmente
    private bool isDashing = false;
    // Timer del dash (cuenta regresiva)
    private float dashTimer = 0f;
    // Almacena el input horizontal (-1, 0 o 1)
    private float horizontalInput;

    [Header("Bloqueo")]
    // Velocidad reducida al moverse con el escudo
    public float shieldMoveSpeed = 2f;

    [Header("Estado")]
    // Flag publico que indica si recibio daño (bloquea controles)
    [SerializeField] public bool recibiodaño = false;

    // START: se ejecuta una vez al inicio
    void Start()
    {
        // Obtener componentes del GameObject
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        // Guardar la gravedad original para poder restaurarla
        originalGravity = rb.gravityScale;

        // CREAR OBJETOS DE DETECCION SI NO EXISTEN
        // Esto permite que el script funcione aunque no asignes los transforms manualmente

        if (groundCheck == null)
        {
            // Crear un GameObject hijo para detectar el suelo
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            // Posicionarlo abajo del jugador
            groundCheckObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = groundCheckObj.transform;
        }

        if (wallCheck == null)
        {
            // Crear un GameObject hijo para detectar paredes
            GameObject wallCheckObj = new GameObject("WallCheck");
            wallCheckObj.transform.SetParent(transform);
            // Posicionarlo al costado del jugador
            wallCheckObj.transform.localPosition = new Vector3(0.3f, 0, 0);
            wallCheck = wallCheckObj.transform;
        }

        if (projectileSpawnPoint == null)
        {
            // Crear un GameObject hijo para el spawn de proyectiles
            GameObject spawnObj = new GameObject("ProjectileSpawn");
            spawnObj.transform.SetParent(transform);
            // Posicionarlo adelante y arriba del centro del jugador
            spawnObj.transform.localPosition = new Vector3(0.5f, 0.2f, 0);
            projectileSpawnPoint = spawnObj.transform;
        }
    }

    // UPDATE: se ejecuta cada frame
    void Update()
    {
        // Si recibio daño, no procesar inputs (el jugador esta en hitstun)
        if (recibiodaño) return;

        // Capturar input horizontal (-1 = izquierda, 0 = nada, 1 = derecha)
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Si NO esta dasheando, procesar controles normales
        if (!isDashing)
        {
            // Ejecutar cada funcion solo si la habilidad esta activada
            if (puedeMoverse) Movimiento();
            if (puedeSaltar) Saltar();
            if (puedeAtacar) Attack();
            if (puedeDash) Dash();
            if (puedeWallCling) WallCling();
            if (puedeBloquear) Bloqueo();
            if (puedeLanzar) LanzarProyectil();
        }
        // Si esta dasheando, actualizar el timer del dash
        else if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            // Cuando el timer llega a 0, terminar el dash
            if (dashTimer <= 0)
            {
                isDashing = false;
                StopDashing();
            }
        }

        // Aplicar fisica de caida mejorada
        AplicarCaidaRapida();
        // Actualizar parametros del animator
        ActualizarAnimaciones();
    }

    // FIXEDUPDATE: se ejecuta a intervalos fijos (mejor para fisica)
    void FixedUpdate()
    {
        // Si recibio daño, no aplicar movimiento
        if (recibiodaño) return;

        // Verificar si esta bloqueando
        bool estaBloquando = anim.GetBool("isBlocking");

        // APLICAR VELOCIDAD HORIZONTAL SEGUN EL ESTADO

        // Movimiento normal: no esta dasheando, bloqueando ni atacando
        if (!isDashing && puedeMoverse && !isAttacking && !estaBloquando)
        {
            SetVelocityX(horizontalInput * speed);
        }
        // Movimiento reducido: esta bloqueando pero no atacando
        else if (estaBloquando && !isAttacking)
        {
            SetVelocityX(horizontalInput * shieldMoveSpeed);
        }
        // Movimiento durante ataque: manejado en StartAttack() con attackStepForce
        // No aplicar velocidad adicional aqui
    }

    // Detectar colision con trigger de victoria
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Finish"))
        {
            SceneManager.LoadScene("Victory");
        }
    }

    // ===== FUNCIONES HELPER =====
    // Estas funciones simplifican operaciones comunes

    // Cambiar solo la velocidad horizontal (mantener Y)
    private void SetVelocityX(float velocity)
    {
        rb.linearVelocity = new Vector2(velocity, rb.linearVelocity.y);
    }

    // Cambiar solo la velocidad vertical (mantener X)
    private void SetVelocityY(float velocity)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, velocity);
    }

    // Cambiar la escala de gravedad del rigidbody
    private void SetGravity(float scale)
    {
        rb.gravityScale = scale;
    }

    // Voltear el sprite segun la direccion del movimiento
    private void FlipSprite(float direction)
    {
        // Solo voltear si hay direccion (no es 0)
        if (direction != 0)
        {
            // Mathf.Sign devuelve -1 o 1
            // Cambiar scale.x voltea el sprite horizontalmente
            transform.localScale = new Vector3(Mathf.Sign(direction), 1, 1);
        }
    }

    // Detectar si esta tocando el suelo usando raycast
    private bool EstaEnSuelo()
    {
        // Lanzar un rayo hacia abajo desde groundCheck
        // Si golpea algo en la capa capaSuelo, devolver true
        return Physics2D.Raycast(groundCheck.position, Vector2.down, longitudRayo, capaSuelo).collider != null;
    }

    // Detectar si esta tocando una pared usando raycast
    private bool EstaEnPared()
    {
        // Lanzar un rayo horizontal desde wallCheck en la direccion que mira
        // transform.localScale.x es -1 o 1 dependiendo de donde mira
        return Physics2D.Raycast(wallCheck.position, Vector2.right * transform.localScale.x, longitudRayo, capaPared).collider != null;
    }

    // ===== MOVIMIENTO =====
    private void Movimiento()
    {
        // No mover si esta atacando
        if (isAttacking) return;

        // Voltear el sprite segun la direccion del input
        FlipSprite(horizontalInput);
        // Actualizar parametro del animator para animacion de caminar
        // Mathf.Abs convierte negativo en positivo (necesitamos 0 o 1, no -1)
        anim.SetFloat("Movement", Mathf.Abs(horizontalInput));
    }

    // ===== SALTO =====
    private void Saltar()
    {
        // Verificar estado actual
        bool enSuelo = EstaEnSuelo();
        bool enPared = EstaEnPared();

        // RESET DE SALTO AL TOCAR EL SUELO
        // Si acaba de tocar el suelo (esta en suelo pero no estaba antes)
        if (enSuelo && !estabaEnSuelo)
        {
            jumpCount = 0; // Resetear contador de saltos
            anim.SetBool("Jump", false); // Desactivar animacion de salto
            isFastWallSliding = false; // Desactivar deslizamiento rapido
        }

        // Guardar estado para el proximo frame
        estabaEnSuelo = enSuelo;

        // EJECUTAR SALTO AL PRESIONAR ESPACIO
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // SALTO NORMAL O DOBLE SALTO
            if (jumpCount < maxJumps)
            {
                // Si es el segundo salto, verificar que este habilitado el doble salto
                if (jumpCount == 1 && !puedeDobleSalto)
                {
                    // No hacer nada si el doble salto esta desactivado
                    return;
                }

                // Elegir fuerza segun si es primer o segundo salto
                float fuerza = (jumpCount == 0) ? fuerzaSalto : fuerzaSaltoDoble;
                // Resetear velocidad vertical antes de saltar (evita acumulacion)
                SetVelocityY(0);
                // Aplicar fuerza hacia arriba (Impulse = instantaneo)
                rb.AddForce(Vector2.up * fuerza, ForceMode2D.Impulse);
                jumpCount++; // Incrementar contador
                anim.SetBool("Jump", true); // Activar animacion

                // Trigger especial para segundo salto
                if (jumpCount == 2)
                    anim.SetTrigger("DoubleJump");
            }
            // WALL JUMP (salto de pared)
            // Solo si NO esta en suelo pero SI esta en pared
            else if (!enSuelo && enPared)
            {
                // Saltar en direccion opuesta a donde mira (-1 o 1)
                float wallJumpDir = -transform.localScale.x;
                // Aplicar velocidad directamente (no AddForce)
                rb.linearVelocity = new Vector2(wallJumpDir * speed, fuerzaSalto);
                // Voltear sprite hacia la nueva direccion
                FlipSprite(wallJumpDir);
                // Setear jumpCount a 1 (puede hacer un salto mas)
                jumpCount = 1;
                anim.SetTrigger("Jump");
            }
        }
    }

    // ===== WALL CLING (agarrarse a la pared) =====
    private void WallCling()
    {
        bool enPared = EstaEnPared();

        // CONDICIONES PARA ACTIVAR WALL CLING:
        // 1. No esta en el suelo
        // 2. Esta tocando una pared
        // 3. Esta presionando hacia la pared (horizontalInput > 0.1)
        // 4. La direccion del input coincide con donde mira
        bool wallClinging = !EstaEnSuelo() && enPared && Mathf.Abs(horizontalInput) > 0.1f &&
                           Mathf.Sign(horizontalInput) == Mathf.Sign(transform.localScale.x);

        if (wallClinging)
        {
            // DESLIZAMIENTO RAPIDO (si presiono S)
            if (isFastWallSliding)
            {
                SetVelocityY(-wallSlideFastSpeed);
            }
            // DESLIZAMIENTO NORMAL
            else
            {
                // Limitar velocidad de caida (no puede caer mas rapido que wallSlideSpeed)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x,
                    Mathf.Clamp(rb.linearVelocity.y, -wallSlideSpeed, float.MaxValue));
            }

            // Reducir gravedad para deslizamiento mas lento
            SetGravity(wallGravity);
            anim.SetBool("WallCling", true);
            // Permitir un salto desde la pared
            jumpCount = 1;

            // WALL JUMP desde wall cling
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // Saltar en direccion opuesta
                float jumpDirection = -transform.localScale.x;
                rb.linearVelocity = new Vector2(jumpDirection * wallJumpForce.x, wallJumpForce.y);
                anim.SetBool("WallCling", false);
                isFastWallSliding = false;
            }

            // Activar deslizamiento rapido
            if (Input.GetKeyDown(wallSlideKey))
            {
                StartCoroutine(ActivarWallSlideFast());
            }
        }
        else
        {
            // Restaurar gravedad normal
            SetGravity(originalGravity);
            anim.SetBool("WallCling", false);
            isFastWallSliding = false;
        }
    }

    // Coroutine que activa deslizamiento rapido temporalmente
    private IEnumerator ActivarWallSlideFast()
    {
        isFastWallSliding = true;
        anim.SetBool("FastWallSlide", true);
        // Esperar medio segundo
        yield return new WaitForSeconds(0.5f);
        // Desactivar
        isFastWallSliding = false;
        anim.SetBool("FastWallSlide", false);
    }

    // ===== CAIDA RAPIDA =====
    // Mejora la sensacion del salto haciendo que caiga mas rapido
    private void AplicarCaidaRapida()
    {
        // No aplicar si esta en wall slide rapido
        if (isFastWallSliding) return;

        // Si esta cayendo (velocidad Y negativa)
        if (rb.linearVelocity.y < 0)
        {
            // Aumentar gravedad multiplicando por fallMultiplier
            // Esto hace que caiga mas rapido de lo normal
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        // Si esta subiendo pero solto la tecla de salto
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            // Aplicar gravedad extra para cortar el salto (salto mas bajo)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    // ===== DASH =====
    private void Dash()
    {
        // Verificar si presiono shift Y ya paso el cooldown
        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time > lastDashTime + dashCooldown)
        {
            // Activar dash
            isDashing = true;
            dashTimer = dashTime; // Iniciar timer
            lastDashTime = Time.time; // Guardar momento del dash
            anim.SetBool("Dash", true);
        }

        // Mientras esta dasheando, aplicar velocidad rapida
        if (isDashing)
        {
            float dashDir = transform.localScale.x; // Direccion donde mira
            SetVelocityX(dashDir * dashSpeed);
        }
    }

    // Funcion publica para terminar el dash (puede ser llamada desde animacion)
    public void StopDashing()
    {
        anim.SetBool("Dash", false);
    }

    // ===== ATAQUE SIMPLE =====
    private void Attack()
    {
        // Solo permitir atacar si NO esta atacando actualmente
        if (isAttacking) return;

        // Detectar presion de tecla Z
        if (Input.GetKeyDown(KeyCode.Z))
        {
            // ATAQUES EN TIERRA (alternan entre ataque 1 y 2)
            if (EstaEnSuelo())
            {
                // Alternar entre ataque 1 y 2
                if (currentCombo == 0 || currentCombo == 2)
                {
                    StartAttack(1); // Primer ataque
                    currentCombo = 1;
                }
                else
                {
                    StartAttack(2); // Segundo ataque
                    currentCombo = 2;
                }
            }
            // ATAQUES EN EL AIRE (siempre usa ataque 1)
            // El animator diferencia con el parametro Grounded o SpeedY
            else
            {
                StartAttack(1); // Usar ataque 1 en el aire
            }
        }
    }

    // Iniciar un ataque
    private void StartAttack(int attackIndex)
    {
        // Marcar como atacando
        isAttacking = true;

        // Activar parametros del animator
        anim.SetBool("isAttacking", true);
        anim.SetInteger("ComboIndex", attackIndex);

        // EMPUJE HACIA ADELANTE (reducido para que no se trabe)
        float direction = transform.localScale.x;
        // Usar velocidad baja para evitar trabarse con el suelo
        rb.linearVelocity = new Vector2(direction * attackStepForce * 0.5f, rb.linearVelocity.y);

        // Obtener duracion de la animacion
        float attackDuration = EstaEnSuelo() ? attackGroundDuration : attackAirDuration;

        // Timer automatico para terminar
        StartCoroutine(AutoStopAttack(attackDuration));
    }

    // Funcion publica para terminar ataque
    public void StopAttacking()
    {
        if (!isAttacking) return;

        anim.SetBool("isAttacking", false);
        // NO resetear ComboIndex aqui - se mantiene para el proximo ataque
        isAttacking = false;

        // Actualizar Grounded ahora que termino el ataque
        anim.SetBool("Grounded", EstaEnSuelo());
    }

    // Timer automatico que termina el ataque
    private IEnumerator AutoStopAttack(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (isAttacking)
        {
            StopAttacking();
        }
    }

    // ===== LANZAR PROYECTILES =====
    private void LanzarProyectil()
    {
        // Verificar si presiono C Y ya paso el cooldown
        if (Input.GetKeyDown(KeyCode.C) && Time.time > lastProjectileTime + projectileCooldown)
        {
            // Verificar que existan los objetos necesarios
            if (projectilePrefab != null && projectileSpawnPoint != null)
            {
                // Registrar momento del lanzamiento
                lastProjectileTime = Time.time;
                // Instanciar proyectil en la posicion del spawn point
                GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);

                // Obtener rigidbody del proyectil
                Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
                if (projectileRb != null)
                {
                    // Calcular direccion (donde mira el jugador)
                    float direction = transform.localScale.x;
                    // Aplicar velocidad al proyectil
                    projectileRb.linearVelocity = new Vector2(direction * projectileSpeed, 0);
                }

                // Activar animacion de lanzar
                anim.SetTrigger("Throw");
            }
        }
    }

    // ===== BLOQUEO =====
    public void Bloqueo()
    {
        if (puedeBloquear)
        {
            // Mantener presionado X para bloquear
            if (Input.GetKey(KeyCode.X))
            {
                anim.SetBool("isBlocking", true);
            }
            else
            {
                anim.SetBool("isBlocking", false);
            }
        }
    }

    // ===== DAÑO =====
    // Funcion publica llamada cuando el jugador recibe daño
    public void RecibirDaño(Vector2 atacantePosicion)
    {
        // Activar flag de daño (bloquea controles)
        recibiodaño = true;

        // CALCULAR DIRECCION DEL KNOCKBACK
        // Obtener vector desde atacante hacia jugador
        Vector2 knockDir = (transform.position - (Vector3)atacantePosicion).normalized;
        // Asegurar que tenga componente vertical minima (siempre salta un poco)
        knockDir.y = Mathf.Clamp(knockDir.y + 0.5f, 0.5f, 1f);

        // Resetear velocidad
        SetVelocityX(0);
        // Aplicar fuerza de empuje
        rb.AddForce(knockDir * impulseForce, ForceMode2D.Impulse);

        // Iniciar coroutine para resetear flag de daño
        StartCoroutine(ResetRecibioDano());
    }

    // Coroutine que resetea el flag de daño despues de un tiempo
    private IEnumerator ResetRecibioDano()
    {
        yield return new WaitForSeconds(0.5f);
        recibiodaño = false;
    }

    // ===== ANIMACIONES =====
    // Actualizar parametros del animator cada frame
    private void ActualizarAnimaciones()
    {
        // Velocidad vertical para blend trees de salto/caida
        anim.SetFloat("SpeedY", rb.linearVelocity.y);

        // NO actualizar Grounded si esta atacando (evita cancelar animacion aereas)
        if (!isAttacking)
        {
            anim.SetBool("Grounded", EstaEnSuelo());
        }

        // Flag de caida (true si no esta en suelo Y esta cayendo)
        anim.SetBool("Falling", !EstaEnSuelo() && rb.linearVelocity.y < -0.1f);
    }

    // ===== GIZMOS =====
    // Dibuja lineas en el editor para visualizar los raycasts
    private void OnDrawGizmos()
    {
        // Rayo de deteccion de suelo (rojo)
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * longitudRayo);
        }

        // Rayo de deteccion de pared (azul)
        if (wallCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + Vector3.right * transform.localScale.x * longitudRayo);
        }
    }
    
    }