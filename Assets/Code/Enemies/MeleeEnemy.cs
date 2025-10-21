using System.Collections;
using UnityEngine;

public class MeleeEnemy : MonoBehaviour
{
    [Header("Detección")]
    public float detectionRange = 2f;
    public LayerMask playerLayer;
    public Transform rayOrigin; 

    [Header("Ataque")]
    public float attackCooldown = 1f;


    private Animator anim;
    private bool isAttacking = false;
    private bool canAttack = true;
    private Vector2 facingDirection
    {
        get
        {
            // Si el enemigo mira a la derecha (scale.x positivo), raycast a la derecha. Si no, a la izquierda
            return transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        }
    }


    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        DetectPlayer();
    }

    void DetectPlayer()
    {
        // Tira un rayo al frente
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin.position, facingDirection, detectionRange, playerLayer);


        if (hit.collider != null && !isAttacking && canAttack)
        {
            // Si el jugador está en rango, iniciar ataque
            StartCoroutine(AttackRoutine(hit.collider.gameObject));
        }
    }

    IEnumerator AttackRoutine(GameObject player)
    {
        isAttacking = true;
        canAttack = false;

        //detener su movimiento
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        anim.SetTrigger("Attack");

        // Esperar cooldown
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
        canAttack = true;
    }

    void OnDrawGizmosSelected()
    {
        if (rayOrigin == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(rayOrigin.position, rayOrigin.position + (Vector3)facingDirection * detectionRange);
    }
}
