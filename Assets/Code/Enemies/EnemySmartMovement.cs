using UnityEngine;
using System.Collections;

/// <summary>
/// Sistema de movimiento inteligente con pathfinding
/// </summary>
public class EnemySmartMovement : MonoBehaviour
{
    public enum IntelligenceLevel
    {
        Basic,          // Solo sigue en horizontal
        Intermediate,   // Puede saltar
        Advanced        // Pathfinding completo con detección de obstáculos
    }

    [Header("Configuración de Inteligencia")]
    [SerializeField] private IntelligenceLevel intelligenceLevel = IntelligenceLevel.Intermediate;

    [Header("Patrulla")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckDistance = 0.5f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Seguimiento")]
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private bool canChasePlayer = true;

    [Header("Salto (Intermediate/Advanced)")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float jumpCooldown = 1f;
    [SerializeField] private float minJumpHeight = 1f; // Mínima diferencia de altura para saltar

    [Header("Detección de Obstáculos (Advanced)")]
    [SerializeField] private float obstacleDetectionDistance = 1.5f;
    [SerializeField] private float wallCheckHeight = 1f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Detección de Plataformas (Advanced)")]
    [SerializeField] private float platformCheckDistance = 3f;
    [SerializeField] private float dropCheckDistance = 2f;

    private EnemyCore core;
    private bool movingRight = true;
    private bool isGrounded;
    private float lastJumpTime = -Mathf.Infinity;
    private bool isStuck = false;
    private float stuckTimer = 0f;
    private Vector2 lastPosition;

    public void Initialize(EnemyCore enemyCore)
    {
        core = enemyCore;
        CreateGroundCheckIfNeeded();
        lastPosition = transform.position;
    }

    private void CreateGroundCheckIfNeeded()
    {
        if (groundCheck == null)
        {
            GameObject checkObj = new GameObject("GroundCheck");
            checkObj.transform.SetParent(transform);
            checkObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = checkObj.transform;
        }
    }

    private void Update()
    {
        if (!core.CanMove) return;

        UpdateGroundedState();
        DetectIfStuck();

        if (canChasePlayer && IsPlayerInRange())
        {
            SmartChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    // ============================================
    // DETECCIÓN DE ESTADO
    // ============================================
    private void UpdateGroundedState()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
        }
    }

    private void DetectIfStuck()
    {
        // Detectar si el enemigo está trabado (no se mueve)
        float distanceMoved = Vector2.Distance(transform.position, lastPosition);

        if (distanceMoved < 0.1f && core.rb.linearVelocity.magnitude > 0.1f)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer >= 0.5f)
            {
                isStuck = true;
            }
        }
        else
        {
            stuckTimer = 0f;
            isStuck = false;
        }

        lastPosition = transform.position;
    }

    // ============================================
    // SEGUIMIENTO INTELIGENTE
    // ============================================
    private bool IsPlayerInRange()
    {
        return core.player != null && core.DistanceToPlayer() <= detectionRange;
    }

    private void SmartChasePlayer()
    {
        if (core.player == null) return;

        Vector2 directionToPlayer = core.DirectionToPlayer();
        float heightDifference = core.player.position.y - transform.position.y;
        bool playerIsHigher = heightDifference > minJumpHeight;

        // Actualizar orientación
        core.FaceDirection(directionToPlayer);

        // Aplicar lógica según nivel de inteligencia
        switch (intelligenceLevel)
        {
            case IntelligenceLevel.Basic:
                BasicChase(directionToPlayer);
                break;

            case IntelligenceLevel.Intermediate:
                IntermediateChase(directionToPlayer, playerIsHigher);
                break;

            case IntelligenceLevel.Advanced:
                AdvancedChase(directionToPlayer, playerIsHigher);
                break;
        }
    }

    // ============================================
    // NIVEL BÁSICO: Solo seguir horizontalmente
    // ============================================
    private void BasicChase(Vector2 direction)
    {
        if (core.rb != null)
        {
            core.rb.linearVelocity = new Vector2(direction.x * chaseSpeed, core.rb.linearVelocity.y);
        }
    }

    // ============================================
    // NIVEL INTERMEDIO: Puede saltar
    // ============================================
    private void IntermediateChase(Vector2 direction, bool playerIsHigher)
    {
        // Movimiento horizontal
        if (core.rb != null)
        {
            core.rb.linearVelocity = new Vector2(direction.x * chaseSpeed, core.rb.linearVelocity.y);
        }

        // Saltar si el jugador está más alto
        if (playerIsHigher && isGrounded && CanJump())
        {
            Jump();
        }

        // Saltar si hay un obstáculo adelante
        if (HasObstacleAhead() && isGrounded && CanJump())
        {
            Jump();
        }
    }

    // ============================================
    // NIVEL AVANZADO: Pathfinding completo
    // ============================================
    private void AdvancedChase(Vector2 direction, bool playerIsHigher)
    {
        bool hasObstacle = HasObstacleAhead();
        bool hasWall = HasWallAhead();
        bool hasPlatformAhead = HasPlatformAhead();
        bool canDropDown = CanDropDown();

        // Si está trabado, intentar saltar
        if (isStuck && isGrounded && CanJump())
        {
            Jump();
            isStuck = false;
            return;
        }

        // Si hay pared, saltar
        if (hasWall && isGrounded && CanJump())
        {
            Jump();
        }
        // Si hay obstáculo pequeño, saltar
        else if (hasObstacle && isGrounded && CanJump())
        {
            Jump();
        }
        // Si el jugador está más alto y hay plataforma, saltar
        else if (playerIsHigher && hasPlatformAhead && isGrounded && CanJump())
        {
            Jump();
        }
        // Si el jugador está abajo y puede bajar, hacerlo
        else if (!playerIsHigher && canDropDown && isGrounded)
        {
            // Avanzar hacia el borde para caer
            if (core.rb != null)
            {
                core.rb.linearVelocity = new Vector2(direction.x * chaseSpeed, core.rb.linearVelocity.y);
            }
        }
        // Movimiento normal
        else
        {
            if (core.rb != null)
            {
                core.rb.linearVelocity = new Vector2(direction.x * chaseSpeed, core.rb.linearVelocity.y);
            }
        }
    }

    // ============================================
    // DETECCIÓN DE OBSTÁCULOS
    // ============================================
    private bool HasObstacleAhead()
    {
        Vector2 direction = core.FacingDirection;
        Vector2 origin = (Vector2)transform.position + Vector2.up * 0.5f;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, obstacleDetectionDistance, obstacleLayer);
        return hit.collider != null;
    }

    private bool HasWallAhead()
    {
        Vector2 direction = core.FacingDirection;
        Vector2 origin = (Vector2)transform.position + Vector2.up * wallCheckHeight;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, obstacleDetectionDistance, obstacleLayer);
        return hit.collider != null;
    }

    private bool HasPlatformAhead()
    {
        Vector2 direction = core.FacingDirection;
        Vector2 origin = (Vector2)transform.position + direction * platformCheckDistance + Vector2.up;

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 3f, groundLayer);
        return hit.collider != null;
    }

    private bool CanDropDown()
    {
        Vector2 direction = core.FacingDirection;
        Vector2 origin = groundCheck.position + (Vector3)direction * 0.5f;

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, dropCheckDistance, groundLayer);
        return hit.collider == null; // No hay suelo = puede bajar
    }

    // ============================================
    // SALTO
    // ============================================
    private bool CanJump()
    {
        return Time.time >= lastJumpTime + jumpCooldown;
    }

    private void Jump()
    {
        if (core.rb == null) return;

        core.rb.linearVelocity = new Vector2(core.rb.linearVelocity.x, 0f);
        core.rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        lastJumpTime = Time.time;

        Debug.Log($"🦘 {gameObject.name} saltó para seguir al jugador");
    }

    // ============================================
    // PATRULLAR
    // ============================================
    private void Patrol()
    {
        if (!HasGroundAhead())
        {
            FlipDirection();
        }

        if (core.rb != null)
        {
            float direction = movingRight ? 1f : -1f;
            core.rb.linearVelocity = new Vector2(direction * patrolSpeed, core.rb.linearVelocity.y);
        }

        if (movingRight && !core.FacingRight)
        {
            core.FaceDirection(Vector2.right);
        }
        else if (!movingRight && core.FacingRight)
        {
            core.FaceDirection(Vector2.left);
        }
    }

    private bool HasGroundAhead()
    {
        if (groundCheck == null) return true;

        Vector2 rayOrigin = groundCheck.position + (movingRight ? Vector3.right * 0.3f : Vector3.left * 0.3f);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, groundCheckDistance, groundLayer);

        return hit.collider != null;
    }

    private void FlipDirection()
    {
        movingRight = !movingRight;

        if (core.rb != null)
        {
            core.rb.linearVelocity = new Vector2(0, core.rb.linearVelocity.y);
        }
    }

    // ============================================
    // GIZMOS
    // ============================================
    private void OnDrawGizmosSelected()
    {
        if (core == null) return;

        // Rango de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Vector2 direction = core.FacingDirection;

        // Detección de obstáculos (naranja)
        Gizmos.color = Color.red;
        Vector2 obstacleOrigin = (Vector2)transform.position + Vector2.up * 0.5f;
        Gizmos.DrawLine(obstacleOrigin, obstacleOrigin + direction * obstacleDetectionDistance);

        // Detección de pared (rojo oscuro)
        Gizmos.color = new Color(0.5f, 0, 0);
        Vector2 wallOrigin = (Vector2)transform.position + Vector2.up * wallCheckHeight;
        Gizmos.DrawLine(wallOrigin, wallOrigin + direction * obstacleDetectionDistance);

        // Detección de plataforma adelante (cyan)
        if (intelligenceLevel == IntelligenceLevel.Advanced)
        {
            Gizmos.color = Color.cyan;
            Vector2 platformOrigin = (Vector2)transform.position + direction * platformCheckDistance + Vector2.up;
            Gizmos.DrawLine(platformOrigin, platformOrigin + Vector2.down * 3f);

            // Detección de caída (verde)
            Gizmos.color = Color.green;
            if (groundCheck != null)
            {
                Vector2 dropOrigin = groundCheck.position + (Vector3)direction * 0.5f;
                Gizmos.DrawLine(dropOrigin, dropOrigin + Vector2.down * dropCheckDistance);
            }
        }

        // Detección de suelo (azul)
        if (groundCheck != null)
        {
            Gizmos.color = Color.blue;
            Vector3 offset = movingRight ? Vector3.right * 0.3f : Vector3.left * 0.3f;
            Vector3 rayOrigin = groundCheck.position + offset;
            Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * groundCheckDistance);
        }
    }
}