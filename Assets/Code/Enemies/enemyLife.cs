using UnityEngine;
using System.Collections;

/// <summary>
/// Sistema de vida universal para enemigos
/// </summary>
public class EnemyLife : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 6f;
    [SerializeField] private float knockbackRecoveryTime = 0.4f;

    [Header("Muerte")]
    [SerializeField] private float deathDelay = 1.5f;

    private EnemyCore core;

    public void Initialize(EnemyCore enemyCore)
    {
        core = enemyCore;
        currentHealth = maxHealth;
    }

    // ============================================
    // RECIBIR DAÑO
    // ============================================
    public void TakeDamage(int damage)
    {
        if (core.IsDead || core.IsTakingDamage) return;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        Debug.Log($"💢 {gameObject.name} recibió {damage} daño. Vida: {currentHealth}/{maxHealth}");

        // Animación de daño
        if (core.animController != null)
        {
            core.animController.SetDamage(true);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            core.SetTakingDamage(true);
            StartCoroutine(DamageRecovery());
        }
    }

    public void TakeDamageWithKnockback(Vector2 attackPosition, int damage)
    {
        if (core.IsDead || core.IsTakingDamage) return;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        Debug.Log($"💢 {gameObject.name} recibió {damage} daño. Vida: {currentHealth}/{maxHealth}");

        core.SetTakingDamage(true);

        // Animación de daño
        if (core.animController != null)
        {
            core.animController.SetDamage(true);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            ApplyKnockback(attackPosition);
            StartCoroutine(DamageRecovery());
        }
    }

    private void ApplyKnockback(Vector2 attackPosition)
    {
        if (core.rb == null) return;

        Vector2 knockbackDir = ((Vector2)transform.position - attackPosition).normalized;
        knockbackDir.y = Mathf.Clamp(knockbackDir.y + 0.5f, 0.5f, 1f);

        core.rb.linearVelocity = Vector2.zero;
        core.rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
    }

    private IEnumerator DamageRecovery()
    {
        yield return new WaitForSeconds(knockbackRecoveryTime);

        core.SetTakingDamage(false);

        if (core.animController != null)
        {
            core.animController.SetDamage(false);
        }

        // Detener movimiento horizontal después del knockback
        if (core.rb != null)
        {
            core.rb.linearVelocity = new Vector2(0, core.rb.linearVelocity.y);
        }
    }

    // ============================================
    // MUERTE
    // ============================================
    private void Die()
    {
        if (core.IsDead) return;

        core.SetDead(true);
        core.SetTakingDamage(false);

        Debug.Log($"☠️ {gameObject.name} ha muerto");

        // Animación de muerte
        if (core.animController != null)
        {
            core.animController.SetDamage(false);
            core.animController.SetDeath(true);
        }

        // Detener movimiento
        if (core.rb != null)
        {
            core.rb.linearVelocity = Vector2.zero;
        }

        // Desactivar colisiones
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Desactivar módulos
        DisableModules();

        // Destruir después del delay
        Destroy(gameObject, deathDelay);
    }

    private void DisableModules()
    {
        if (core.movement != null) core.movement.enabled = false;
        if (core.meleeAttack != null) core.meleeAttack.enabled = false;
        if (core.rangedAttack != null) core.rangedAttack.enabled = false;
        if (core.flying != null) core.flying.enabled = false;
    }

    // ============================================
    // COLISIONES (detección de espada del jugador)
    // ============================================
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Espada") && !core.IsDead)
        {
            Vector2 attackPosition = new Vector2(collision.transform.position.x, transform.position.y);
            TakeDamageWithKnockback(attackPosition, 1);
        }
    }

    // ============================================
    // PROPIEDADES PÚBLICAS
    // ============================================
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public float HealthPercentage => (float)currentHealth / maxHealth;
}