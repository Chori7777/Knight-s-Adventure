using UnityEngine;

/// <summary>
/// Controlador de animaciones flexible - solo usa las que existan
/// </summary>
public class EnemyAnimationController : MonoBehaviour
{
    private Animator anim;
    private EnemyCore core;

    // Parámetros disponibles (se detectan automáticamente)
    private bool hasMovementParam;
    private bool hasSpeedXParam;
    private bool hasSpeedYParam;
    private bool hasGroundedParam;
    private bool hasDamageParam;
    private bool hasDeathParam;
    private bool hasAttackTrigger;
    private bool hasIsAttackingParam;

    public void Initialize(EnemyCore enemyCore)
    {
        core = enemyCore;
        anim = core.anim;

        if (anim == null) return;

        // Detectar qué parámetros existen
        DetectAvailableParameters();
    }

    private void DetectAvailableParameters()
    {
        if (anim == null) return;

        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            switch (param.name)
            {
                case "Movement":
                    hasMovementParam = true;
                    break;
                case "SpeedX":
                    hasSpeedXParam = true;
                    break;
                case "SpeedY":
                    hasSpeedYParam = true;
                    break;
                case "Grounded":
                case "isGrounded":
                    hasGroundedParam = true;
                    break;
                case "damage":
                case "Damage":
                    hasDamageParam = true;
                    break;
                case "Death":
                case "isDead":
                    hasDeathParam = true;
                    break;
                case "Attack":
                    hasAttackTrigger = true;
                    break;
                case "isAttacking":
                    hasIsAttackingParam = true;
                    break;
            }
        }

        Debug.Log($"[{gameObject.name}] Parámetros detectados: " +
                  $"Movement={hasMovementParam}, " +
                  $"Damage={hasDamageParam}, " +
                  $"Death={hasDeathParam}, " +
                  $"Attack={hasAttackTrigger}");
    }

    private void LateUpdate()
    {
        if (anim == null || core == null) return;
        UpdateAllAnimations();
    }

    private void UpdateAllAnimations()
    {
        UpdateMovementAnimation();
        UpdateVelocityAnimation();
        UpdateStateAnimations();
    }

    // ============================================
    // ANIMACIONES DE MOVIMIENTO
    // ============================================
    private void UpdateMovementAnimation()
    {
        if (!hasMovementParam || core.rb == null) return;

        float moveAmount = Mathf.Abs(core.rb.linearVelocity.x);
        anim.SetFloat("Movement", moveAmount);
    }

    private void UpdateVelocityAnimation()
    {
        if (core.rb == null) return;

        if (hasSpeedXParam)
        {
            anim.SetFloat("SpeedX", Mathf.Abs(core.rb.linearVelocity.x));
        }

        if (hasSpeedYParam)
        {
            anim.SetFloat("SpeedY", core.rb.linearVelocity.y);
        }
    }

    private void UpdateStateAnimations()
    {
        if (hasIsAttackingParam)
        {
            anim.SetBool("isAttacking", core.IsAttacking);
        }
    }

    // ============================================
    // TRIGGERS Y ESTADOS
    // ============================================
    public void TriggerAttack()
    {
        if (hasAttackTrigger)
        {
            anim.SetTrigger("Attack");
        }
    }

    public void SetDamage(bool value)
    {
        if (hasDamageParam)
        {
            anim.SetBool("damage", value);
        }
    }

    public void SetDeath(bool value)
    {
        if (hasDeathParam)
        {
            anim.SetBool("Death", value);
        }
    }

    public void SetGrounded(bool value)
    {
        if (hasGroundedParam)
        {
            anim.SetBool("Grounded", value);
        }
    }

    // ============================================
    // EVENTOS DE ANIMACIÓN (llamados desde Animation Events)
    // ============================================
    public void OnAttackHitFrame()
    {
        // Este método puede ser llamado desde el Animation Event
        if (core.meleeAttack != null)
        {
            core.meleeAttack.DealDamage();
        }
    }

    public void OnAttackEnd()
    {
        core.SetAttacking(false);
    }

    public void OnDamageEnd()
    {
        SetDamage(false);
        core.SetTakingDamage(false);
    }
}