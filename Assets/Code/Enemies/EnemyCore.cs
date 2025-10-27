using UnityEngine;

/// <summary>
/// Núcleo del enemigo - Gestiona referencias y estados globales
/// </summary>
public class EnemyCore : MonoBehaviour
{
    // ============================================
    // COMPONENTES
    // ============================================
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Animator anim;
    [HideInInspector] public SpriteRenderer spriteRenderer;

    // Módulos opcionales
    [HideInInspector] public EnemyAnimationController animController;
    [HideInInspector] public EnemyMovement movement;
    [HideInInspector] public EnemyLife life;
    [HideInInspector] public EnemyMeleeAttack meleeAttack;
    [HideInInspector] public EnemyRangedAttack rangedAttack;
    [HideInInspector] public EnemyFlying flying;

    // ============================================
    // ESTADO
    // ============================================
    public bool IsDead { get; private set; }
    public bool IsTakingDamage { get; private set; }
    public bool IsAttacking { get; private set; }
    public bool CanMove => !IsDead && !IsTakingDamage && !IsAttacking;

    // ============================================
    // REFERENCIAS EXTERNAS
    // ============================================
    [Header("Referencias")]
    public Transform player;

    // ============================================
    // ORIENTACIÓN
    // ============================================
    public bool FacingRight { get; private set; } = true;

    public Vector2 FacingDirection => FacingRight ? Vector2.right : Vector2.left;

    // ============================================
    // INICIALIZACIÓN
    // ============================================
    private void Awake()
    {
        InitializeComponents();
        FindPlayer();

        // Inicializar movimiento correctamente
        if (movement != null)
            movement.Initialize(this); // ya usa la variable de clase
    }

    private void InitializeComponents()
    {
        // Componentes básicos
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Módulos opcionales
        animController = GetComponent<EnemyAnimationController>();
        movement = GetComponent<EnemyMovement>();
        life = GetComponent<EnemyLife>();
        meleeAttack = GetComponent<EnemyMeleeAttack>();
        rangedAttack = GetComponent<EnemyRangedAttack>();
        flying = GetComponent<EnemyFlying>();

        // Inicializar controlador de animaciones
        if (animController != null)
        {
            animController.Initialize(this);
        }

        // Inicializar módulos
        if (movement != null) movement.Initialize(this);
        if (life != null) life.Initialize(this);
        if (meleeAttack != null) meleeAttack.Initialize(this);
        if (rangedAttack != null) rangedAttack.Initialize(this);
        if (flying != null) flying.Initialize(this);
    }

    public void FindPlayer()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    // ============================================
    // CONTROL DE ORIENTACIÓN
    // ============================================
    public void FaceDirection(Vector2 direction)
    {
        if (direction.x > 0 && !FacingRight)
        {
            Flip();
        }
        else if (direction.x < 0 && FacingRight)
        {
            Flip();
        }
    }

    public void FaceTarget(Transform target)
    {
        if (target == null) return;
        Vector2 direction = (target.position - transform.position).normalized;
        FaceDirection(direction);
    }

    private void Flip()
    {
        FacingRight = !FacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // ============================================
    // CONTROL DE ESTADOS
    // ============================================
    public void SetDead(bool value)
    {
        IsDead = value;
    }

    public void SetTakingDamage(bool value)
    {
        IsTakingDamage = value;
    }

    public void SetAttacking(bool value)
    {
        IsAttacking = value;
    }

    // ============================================
    // UTILIDADES
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
}