using System.Collections;
using UnityEngine;

/// <summary>
/// Ataques y comportamiento del Gnomo Boss
/// </summary>
public class MiniBossGnome : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    public Rigidbody2D rb;
    public BossAnimationController animController;

    [Header("Movimiento")]
    public Transform pointA;
    public Transform pointB;
    private Transform currentTarget;
    public float moveSpeed = 2f;
    private bool isMoving = false;

    [Header("Invocación de Orugas")]
    public Transform spawnOrugaA;
    public Transform spawnOrugaB;
    public GameObject orugaPrefab;
    public float orugaSpawnRate = 5f;

    [Header("Lanzamiento de Frascos")]
    public GameObject potionPrefab;
    public Transform throwPoint;
    public float throwCooldown = 3f;
    public float throwForce = 5f;
    private bool canThrow = true;

    [Header("Otros")]
    public bool isDead = false;
    public float alertDuration = 0.5f;
    private Animator anim;
    private void Start()
    {
        rb = rb ?? GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        currentTarget = pointA;

        if (spawnOrugaA != null || spawnOrugaB != null)
            StartCoroutine(SpawnOrugasLoop());

        StartCoroutine(AttackLoop());
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        // Movimiento entre plataformas
        if (!isMoving)
            StartCoroutine(MoveBetweenPoints());
    }

    // ===========================
    // MOVIMIENTO ENTRE PLATAFORMAS
    // ===========================
    private IEnumerator MoveBetweenPoints()
    {
        isMoving = true;

        while (!isDead)
        {
            if (currentTarget == null) break;

            Vector2 direction = (currentTarget.position - transform.position).normalized;
            rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

            // Cambiar objetivo cuando llega al punto
            if (Vector2.Distance(transform.position, currentTarget.position) < 0.1f)
            {
                currentTarget = currentTarget == pointA ? pointB : pointA;
                break; // salir del loop para dejar que vuelva FixedUpdate
            }

            yield return new WaitForFixedUpdate();
        }

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        isMoving = false;
    }

    // ===========================
    // INVOCACIÓN DE ORUGAS
    // ===========================
    private IEnumerator SpawnOrugasLoop()
    {
        while (!isDead)
        {
            if (spawnOrugaA != null)
                Instantiate(orugaPrefab, spawnOrugaA.position, Quaternion.identity);
            if (spawnOrugaB != null)
                Instantiate(orugaPrefab, spawnOrugaB.position, Quaternion.identity);

            yield return new WaitForSeconds(orugaSpawnRate);
        }
    }

    // ===========================
    // LANZAMIENTO DE FRASCOS
    // ===========================
    private IEnumerator AttackLoop()
    {
        while (!isDead)
        {
            // Esperar a que haya jugador
            if (player != null && throwPoint != null)
            {
                yield return StartCoroutine(ThrowPotion());
            }
            else
            {
                yield return null;
            }
        }
    }

    private IEnumerator ThrowPotion()
    {
        canThrow = false;

        // Trigger animación
        if (anim != null)
            anim.SetTrigger("Attack");

        // Espera para que se vea la animación
        yield return new WaitForSeconds(alertDuration);

        // Lanza el frasco
        if (potionPrefab != null && throwPoint != null && player != null)
        {
            GameObject potion = Instantiate(potionPrefab, throwPoint.position, Quaternion.identity);
            Rigidbody2D potionRb = potion.GetComponent<Rigidbody2D>();
            if (potionRb != null)
            {
                Vector2 direction = (player.position - throwPoint.position).normalized;
                potionRb.AddForce(new Vector2(direction.x, 1) * throwForce, ForceMode2D.Impulse);
            }
        }

        // Esperar cooldown antes del próximo ataque
        yield return new WaitForSeconds(throwCooldown);
        canThrow = true;
    }
}