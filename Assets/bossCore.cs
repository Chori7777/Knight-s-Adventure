using UnityEngine;

/// <summary>
/// Núcleo del jefe - Gestiona referencias y estado general (VERSIÓN COMPLETA)
/// </summary>
public class bossCore : MonoBehaviour
{
    // ============================================
    // COMPONENTES
    // ============================================
    [Header("Componentes principales")]
    public Rigidbody2D rb;
    public Animator anim;
    public SpriteRenderer spriteRenderer;
    public Transform player;

    // ============================================
    // ESTADO DEL JEFE
    // ============================================
    [Header("Estado general")]
    public bool IsDead = false;
    public bool IsAttacking = false;
    public bool IsVulnerable = true;
    public bool CanMove = true;

    // ============================================
    // PROPIEDADES DE ACCESO RÁPIDO
    // ============================================
    public Vector2 PlayerPosition
    {
        get
        {
            return player != null ? (Vector2)player.position : Vector2.zero;
        }
    }

    // ============================================
    // INICIALIZACIÓN
    // ============================================
    private void Awake()
    {
        // Buscar componentes locales
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Buscar al jugador por tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log($"✅ {gameObject.name}: Jugador encontrado");
        }
        else
        {
            Debug.LogError($"❌ {gameObject.name}: No se encontró jugador con tag 'Player'");
        }
    }

    // ============================================
    // MÉTODOS ÚTILES
    // ============================================
    public float DistanceToPlayer()
    {
        if (player == null) return Mathf.Infinity;
        return Vector2.Distance(transform.position, player.position);
    }

    public Vector2 DirectionToPlayer()
    {
        if (player == null) return Vector2.zero;
        return (player.position - transform.position).normalized;
    }

    public void FacePlayer()
    {
        if (player == null) return;

        // Mirar hacia el jugador según su posición
        if (player.position.x < transform.position.x)
            transform.localScale = new Vector3(-1, 1, 1);
        else
            transform.localScale = new Vector3(1, 1, 1);
    }

    // ============================================
    // CONTROL DE MOVIMIENTO / ACCIÓN
    // ============================================
    public void SetCanMove(bool state)
    {
        CanMove = state;
    }

    public void StopMovement()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // ============================================
    // UTILIDAD PARA DEBUG
    // ============================================
    private void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }

}
