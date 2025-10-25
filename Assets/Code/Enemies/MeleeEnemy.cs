using UnityEngine;
using System.Collections;

/// <summary>
/// Sistema de ataque cuerpo a cuerpo
/// </summary>
public class EnemyMeleeAttack : MonoBehaviour
{
    [Header("Detección")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask playerLayer;

    [Header("Ataque")]
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float attackDuration = 0.5f;

    [Header("Hitbox (Opcional - para Animation Event)")]
    [SerializeField] private float hitboxRadius = 1f;

    private EnemyCore core;
    private float lastAttackTime = -Mathf.Infinity;

    public void Initialize(EnemyCore enemyCore)
    {
        core = enemyCore;
        CreateAttackPointIfNeeded();
    }

    private void CreateAttackPointIfNeeded()
    {
        if (attackPoint == null)
        {
            GameObject pointObj = new GameObject("AttackPoint");
            pointObj.transform.SetParent(transform);
            pointObj.transform.localPosition = new Vector3(0.5f, 0, 0);
            attackPoint = pointObj.transform;
        }
    }

    private void Update()
    {
        if (!core.CanMove || core.IsAttacking) return;

        // Detectar jugador en rango de ataque
        if (IsPlayerInRange() && CanAttack())
        {
            StartCoroutine(AttackRoutine());
        }
    }

    // ============================================
    // DETECCIÓN
    // ============================================
    private bool IsPlayerInRange()
    {
        if (core.player == null) return false;

        // Raycast en la dirección que mira el enemigo
        Vector2 direction = core.FacingDirection;
        RaycastHit2D hit = Physics2D.Raycast(attackPoint.position, direction, attackRange, playerLayer);

        return hit.collider != null;
    }

    private bool CanAttack()
    {
        return Time.time >= lastAttackTime + attackCooldown;
    }

    // ============================================
    // ATAQUE
    // ============================================
    private IEnumerator AttackRoutine()
    {
        core.SetAttacking(true);
        lastAttackTime = Time.time;

        // Detener movimiento
        if (core.rb != null)
        {
            core.rb.linearVelocity = Vector2.zero;
        }

        // Trigger de animación
        if (core.animController != null)
        {
            core.animController.TriggerAttack();
        }

        // Esperar un momento antes de hacer daño (sincronizado con animación)
        yield return new WaitForSeconds(attackDuration * 0.5f);

        // Hacer daño (también se puede llamar desde Animation Event)
        DealDamage();

        // Esperar resto de la animación
        yield return new WaitForSeconds(attackDuration * 0.5f);

        core.SetAttacking(false);
    }

    /// <summary>
    /// Método público para ser llamado desde Animation Event
    /// </summary>
    public void DealDamage()
    {
        // Detectar jugador en hitbox circular
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, hitboxRadius, playerLayer);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                playerLife playerLife = hit.GetComponent<playerLife>();
                if (playerLife != null)
                {
                    Vector2 attackPosition = new Vector2(transform.position.x, 0);
                    playerLife.TakeDamage(attackPosition, attackDamage);
                    Debug.Log($"⚔️ {gameObject.name} golpeó al jugador");
                }
            }
        }
    }

    // ============================================
    // GIZMOS
    // ============================================
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        // Rango de detección (raycast)
        Gizmos.color = Color.red;
        Vector2 direction = core != null ? core.FacingDirection : Vector2.right;
        Gizmos.DrawLine(attackPoint.position, attackPoint.position + (Vector3)direction * attackRange);

        // Hitbox de daño
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawSphere(attackPoint.position, hitboxRadius);
    }
}