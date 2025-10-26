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
        if (core == null || !core.CanMove) return;

        if (core.player == null)
            core.FindPlayer(); // opcional: intenta encontrar jugador cada frame

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

        core.FaceDirection(directionToPlayer);

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

    private void BasicChase(Vector2 direction)
    {
        if (core.rb != null)
        {
            core.rb.linearVelocity = new Vector2(direction.x * chaseSpeed, core.rb.linearVelocity.y);
        }
    }

    private void IntermediateChase(Vector2 direction, bool playerIsHigher)
    {
        if (core.rb != null)
        {
            core.rb.linearVelocity = new Vector2(direction.x * chaseSpeed, core.rb.linearVelocity.y);
        }

        if (playerIsHigher && isGrounded && CanJump())
        {
            Jump();
        }

        if (DetectObstacleAhead() && isGrounded && CanJump())
        {
            Jump();
        }
    }

    // ============================================
    // NIVEL AVANZADO
    // ============================================
    private void AdvancedChase(Vector2 direction, bool playerIsHigher)
    {
        bool obstacleAhead = DetectObstacleAhead();
        bool hasWall = HasWallAhead();
        bool hasPlatformAhead = HasPlatformAhead();
        bool canDropDown = CanDropDown();

        if ((obstacleAhead || hasWall) && isGrounded && CanJump())
        {
            Jump();
        }
        else if (playerIsHigher && hasPlatformAhead && isGrounded && CanJump())
        {
            Jump();
        }
        else if (!playerIsHigher && canDropDown && isGrounded)
        {
            if (core.rb != null)
                core.rb.linearVelocity = new Vector2(direction.x * chaseSpeed, core.rb.linearVelocity.y);
        }
        else
        {
            if (core.rb != null)
                core.rb.linearVelocity = new Vector2(direction.x * chaseSpeed, core.rb.linearVelocity.y);
        }

        core.FaceDirection(direction);
    }

    // ============================================
    // DETECCIÓN DE OBSTÁCULOS MEJORADA
    // ============================================
    private bool DetectObstacleAhead()
    {
        Vector2 dir = core.FacingDirection;
        float[] heights = { 0.2f, 0.5f, 0.8f };
        foreach (float h in heights)
        {
            Vector2 origin = (Vector2)transform.position + Vector2.up * h;
            RaycastHit2D hit = Physics2D.Raycast(origin, dir, obstacleDetectionDistance, obstacleLayer);
            if (hit.collider != null)
            {
                Debug.DrawLine(origin, origin + dir * obstacleDetectionDistance, Color.red);
                return true;
            }
        }
        return false;
    }

    private bool CanJump()
    {
        return Time.time >= lastJumpTime + jumpCooldown;
    }

    private void Jump()
    {
        if (core.rb == null) return;
        float xVel = core.rb.linearVelocity.x;
        core.rb.linearVelocity = new Vector2(xVel, 0f);
        core.rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        lastJumpTime = Time.time;
        Debug.Log($"🦘 {gameObject.name} saltó por obstáculo detectado");
    }

    // ============================================
    // PATRULLA
    // ============================================
    private void Patrol()
    {
        if (!HasGroundAhead()) FlipDirection();

        if (core.rb != null)
        {
            float dir = movingRight ? 1f : -1f;
            core.rb.linearVelocity = new Vector2(dir * patrolSpeed, core.rb.linearVelocity.y);
        }

        if (movingRight && !core.FacingRight)
            core.FaceDirection(Vector2.right);
        else if (!movingRight && core.FacingRight)
            core.FaceDirection(Vector2.left);
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
            core.rb.linearVelocity = new Vector2(0, core.rb.linearVelocity.y);
    }

    private bool HasWallAhead()
    {
        Vector2 origin = (Vector2)transform.position + Vector2.up * wallCheckHeight;
        RaycastHit2D hit = Physics2D.Raycast(origin, core.FacingDirection, obstacleDetectionDistance, obstacleLayer);
        return hit.collider != null;
    }

    private bool HasPlatformAhead()
    {
        Vector2 origin = (Vector2)transform.position + core.FacingDirection * platformCheckDistance + Vector2.up;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 3f, groundLayer);
        return hit.collider != null;
    }

    private bool CanDropDown()
    {
        Vector2 origin = groundCheck.position + (Vector3)core.FacingDirection * 0.5f;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, dropCheckDistance, groundLayer);
        return hit.collider == null;
    }
}

