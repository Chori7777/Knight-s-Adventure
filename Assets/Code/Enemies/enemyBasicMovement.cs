using UnityEngine;
using System.Collections;

public class enemyBasicMovement : MonoBehaviour
{
    [Header("Patrulla")]
    public float speed = 2f;
    public Transform groundCheck;
    public float checkDistance = 0.5f;
    public LayerMask sueloLayer; 

    [Header("Player")]
    public Transform player;
    public float detectionRadius = 5f;

    [Header("Combate")]
    public int vida = 3;
    public float fuerzaKnockback = 6f;

    private Rigidbody2D rb;
    private Animator anim;
    private bool movingRight = true;
    private bool muerto = false;
    private bool recibiendoDanio = false;
    public bool puedeMoverse = true;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    void Update()
    {
        if (!muerto && !recibiendoDanio && puedeMoverse)
        {
            //seguir al jugador si esta cerca
            if (player != null)
            {
                float distToPlayer = Vector2.Distance(transform.position, player.position);

                if (distToPlayer < detectionRadius)
                {
                    SeguirJugador();
                    return;
                }
            }

            Patrullar();
        }
    }

    void SeguirJugador()
    {
        Vector2 direction = (player.position - transform.position).normalized;

        // Voltear
        if (direction.x > 0 && !movingRight) Flip();
        else if (direction.x < 0 && movingRight) Flip();

        // Mover
        rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocity.y);
    }

    void Patrullar()
    {
        // Verifica si hay suelo adelante
        Vector2 rayOrigin = groundCheck.position + (movingRight ? Vector3.right * 0.3f : Vector3.left * 0.3f);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, checkDistance, sueloLayer);
        bool haySueloAdelante = hit.collider != null;

        if (!haySueloAdelante)
        {
            Flip();
        }

        // Moverse
        float direccion = movingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(direccion * speed, rb.linearVelocity.y);
    }

    void Flip()
    {
        movingRight = !movingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !muerto)
        {
            // Dañar al jugador
            playerLife playerLife = collision.gameObject.GetComponent<playerLife>();
            if (playerLife != null)
            {
                Vector2 direccionDanio = new Vector2(transform.position.x, 0);
                playerLife.TakeDamage(direccionDanio, 1);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Espada") && !muerto)
        {
            Vector2 direccionDanio = new Vector2(collision.transform.position.x, 0);
            RecibeDanio(direccionDanio, 1);
        }
    }

    public void RecibeDanio(Vector2 direccion, int cantDanio)
    {
        if (!recibiendoDanio && !muerto)
        {
            vida -= cantDanio;
            recibiendoDanio = true;

            if (vida <= 0)
            {
                Morir();
            }
            else
            {
                // Aplicar knockback
                Vector2 knockDir = ((Vector2)transform.position - direccion).normalized;
                knockDir.y = Mathf.Clamp(knockDir.y + 0.5f, 0.5f, 1f);
                rb.linearVelocity = Vector2.zero;
                rb.AddForce(knockDir * fuerzaKnockback, ForceMode2D.Impulse);

                StartCoroutine(RecuperarDeKnockback());
            }
        }
    }

    void Morir()
    {
        muerto = true;
        rb.linearVelocity = Vector2.zero;

        // Desactivar colisiones
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Debug.Log("Muelto");

        Destroy(gameObject, 1.5f);
    }

    IEnumerator RecuperarDeKnockback()
    {
        yield return new WaitForSeconds(0.4f);
        recibiendoDanio = false;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    void OnDrawGizmosSelected()
    {
        // Gizmo de detección de jugador
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Gizmo de detección de suelo
        if (groundCheck != null)
        {
            Gizmos.color = Color.blue;
            Vector3 offset = movingRight ? Vector3.right * 0.3f : Vector3.left * 0.3f;
            Vector3 rayOrigin = groundCheck.position + offset;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, checkDistance, sueloLayer);
            if (hit.collider != null)
            {
                Gizmos.DrawLine(rayOrigin, hit.point);
            }
            else
            {
                Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * checkDistance);
            }
        }
    }
}
