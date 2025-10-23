using UnityEngine;

// ============================================
// CONTROLADOR DE ANIMACIONES - SCRIPT SEPARADO
// ============================================
public class PlayerAnimationController : MonoBehaviour
{
    private Animator anim;
    private PlayerMovement player;

    private bool isDoubleJumping;          // NUEVO: Flag para controlar DoubleJump
    private float doubleJumpAnimTime = 0.6f; // NUEVO: Duración de la animación de DoubleJump

    public void Initialize(PlayerMovement playerMovement)
    {
        anim = GetComponent<Animator>();
        player = playerMovement;
    }

    private void LateUpdate()
    {
        if (anim == null || player == null) return;
        UpdateAllAnimations();
    }

    // ============================================
    // ACTUALIZAR TODAS LAS ANIMACIONES
    // ============================================
    private void UpdateAllAnimations()
    {
        UpdateMovementAnimation();
        UpdateJumpAnimation();
        UpdateFallingAnimation();
        UpdateGroundedAnimation();
        UpdateDashAnimation();
        UpdateWallClingAnimation();
        UpdateAttackAnimation();
        UpdateBlockAnimation();
        UpdateSpeedYAnimation();
    }

    // ============================================
    // ANIMACIONES INDIVIDUALES
    // ============================================
    private void UpdateMovementAnimation()
    {
        float moveAmount = Mathf.Abs(player.HorizontalInput);
        anim.SetFloat("Movement", moveAmount);
    }

    private void UpdateJumpAnimation()
    {
        if (player.IsGrounded)
        {
            anim.SetBool("Jump", false);
            isDoubleJumping = false; // NUEVO: Resetear flag al tocar suelo
        }
        else if (player.VerticalVelocity > 0.1f && !isDoubleJumping) // CORREGIDO: No activar si estamos en DoubleJump
        {
            anim.SetBool("Jump", true);
        }
    }

    private void UpdateFallingAnimation()
    {
        // CORREGIDO: No activar Falling si estamos en DoubleJump
        bool isFalling = !player.IsGrounded && player.VerticalVelocity < -0.1f && !isDoubleJumping;
        anim.SetBool("Falling", isFalling);
    }

    private void UpdateGroundedAnimation()
    {
        if (!player.IsAttacking)
        {
            anim.SetBool("Grounded", player.IsGrounded);
        }
    }

    private void UpdateDashAnimation()
    {
        anim.SetBool("Dash", player.IsDashing);
    }

    private void UpdateWallClingAnimation()
    {
        bool isWallClinging = !player.IsGrounded && player.IsTouchingWall;
        anim.SetBool("WallCling", isWallClinging);
    }

    private void UpdateAttackAnimation()
    {
        anim.SetBool("isAttacking", player.IsAttacking);
    }

    private void UpdateBlockAnimation()
    {
        anim.SetBool("isBlocking", player.IsBlocking);
    }

    private void UpdateSpeedYAnimation()
    {
        anim.SetFloat("SpeedY", player.VerticalVelocity);
    }

 

    // ============================================
    // TRIGGERS PUBLICOS (llamados desde PlayerMovement)
    // ============================================
    public void TriggerDoubleJump()
    {
        anim.SetTrigger("DoubleJump");
        isDoubleJumping = true; // NUEVO: Activar flag

        // NUEVO: Desactivar flag después del tiempo de animación
        Invoke(nameof(ResetDoubleJump), doubleJumpAnimTime);
    }

    // NUEVO: Método para resetear el flag de DoubleJump
    private void ResetDoubleJump()
    {
        isDoubleJumping = false;
    }

    public void TriggerThrow()
    {
        anim.SetTrigger("Throw");
    }

    public void TriggerDamage()
    {
        anim.SetBool("damage", true);
    }

    public void StopDamage()
    {
        anim.SetBool("damage", false);
    }

    public void TriggerDeath()
    {
        anim.SetTrigger("Death");
    }

    public void SetComboIndex(int index)
    {
        anim.SetInteger("ComboIndex", index);
    }
}