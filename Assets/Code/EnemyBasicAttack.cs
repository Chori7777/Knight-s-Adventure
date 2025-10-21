using UnityEngine;

public class EnemyBasicAttack : MonoBehaviour
{
    public Transform player;
    public GameObject projectilePrefab1;
    public GameObject projectilePrefab2;
    public Transform firePoint1;
    public Transform firePoint2;
    public float projectileSpeed = 10f;
    public float attackCooldown = 2f;
    public float attackRange = 3f;
    public float detectionRange = 8f;
    private float lastAttackTime = -Mathf.Infinity;
    private SpriteRenderer spriteRenderer;
    private Animator anim;
    private Rigidbody2D rb;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // ?? Buscar al jugador autom�ticamente si no est� asignado
        if (player == null)
        {
            GameObject playerObj = Object.FindFirstObjectByType<playerLife>()?.gameObject;
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log("Jugador encontrado autom�ticamente: " + player.gameObject.name);
            }
            else
            {
                Debug.LogError("? No se encontr� al jugador. Aseg�rate de que tenga el script 'playerLife'");
            }
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool playerDetected = distanceToPlayer <= detectionRange;

        // Girar hacia el jugador si lo detecta
        if (playerDetected)
        {
            OrientarHaciaJugador();
        }

        // Si el jugador est� en rango de ataque y pas� el cooldown
        if (playerDetected && distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            Atacar();
        }

        // Actualizar animaci�n de movimiento basado en la velocidad
        if (anim != null)
        {
            bool estaMoviendo = Mathf.Abs(rb.linearVelocity.x) > 0.1f;
            anim.SetBool("isMoving", estaMoviendo);
        }
    }

    void OrientarHaciaJugador()
    {
        if (player.position.x > transform.position.x)
        {
            spriteRenderer.flipX = false; // Mirar derecha
        }
        else
        {
            spriteRenderer.flipX = true; // Mirar izquierda
        }
    }

    void Atacar()
    {
        lastAttackTime = Time.time;
        Debug.Log("�Enemigo atacando!");

        // Trigger de ataque
        if (anim != null)
            anim.SetTrigger("Attack");

        // Disparar
        Disparar();
    }

    void Disparar()
    {
        Debug.Log("Disparando proyectiles...");

        // Proyectil 1 (arriba)
        if (projectilePrefab1 != null && firePoint1 != null)
        {
            GameObject proj1 = Instantiate(projectilePrefab1, firePoint1.position, Quaternion.identity);
            Rigidbody2D rb1 = proj1.GetComponent<Rigidbody2D>();
            if (rb1 != null)
            {
                Vector2 direccion = (player.position - firePoint1.position).normalized;
                rb1.linearVelocity = direccion * projectileSpeed;
                Debug.Log("Proyectil 1 disparado");
            }
            else
            {
                Debug.LogWarning("?? Proyectil 1 no tiene Rigidbody2D");
            }
        }
        else
        {
            Debug.LogWarning("?? Falta proyectilPrefab1 o firePoint1");
        }

        // Proyectil 2 (medio)
        if (projectilePrefab2 != null && firePoint2 != null)
        {
            GameObject proj2 = Instantiate(projectilePrefab2, firePoint2.position, Quaternion.identity);
            Rigidbody2D rb2 = proj2.GetComponent<Rigidbody2D>();
            if (rb2 != null)
            {
                Vector2 direccion = (player.position - firePoint2.position).normalized;
                rb2.linearVelocity = direccion * projectileSpeed;
                Debug.Log("Proyectil 2 disparado");
            }
            else
            {
                Debug.LogWarning("?? Proyectil 2 no tiene Rigidbody2D");
            }
        }
        else
        {
            Debug.LogWarning("?? Falta proyectilPrefab2 o firePoint2");
        }
    }

    // Recibir da�o
    public void RecibirDano()
    {
        if (anim != null)
            anim.SetBool("damage", true);
        // Desactivar da�o despu�s de un tiempo
        Invoke("DesactivarDano", 0.5f);
    }

    void DesactivarDano()
    {
        if (anim != null)
            anim.SetBool("damage", false);
    }

    // Morir
    public void Morir()
    {
        if (anim != null)
            anim.SetBool("Death", true);
        // Desactivar el enemigo
        enabled = false;
        Destroy(gameObject, 2f);
    }

    void OnDrawGizmosSelected()
    {
        // Esfera de detecci�n (rojo)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        // Esfera de ataque (azul)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}