using UnityEngine;

public class ParabolicBulletScript : MonoBehaviour
{
    public int damage = 1;
    public float lifetime = 4f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // No asignar velocidad aqu�, ya viene del enemigo
        if (rb == null)
        {
            Debug.LogError("No hay Rigidbody2D en el proyectil par�bola");
        }
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<playerLife>();
            if (player != null)
            {
                player.RecibeDano(transform.position, damage);
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("Suelo"))
        {
            Destroy(gameObject);
        }
    }
}