using UnityEngine;

public class damageCollider : MonoBehaviour
{
    public int damage = 1; // Cantidad de daño que el enemigo inflige
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player")) // Usa gameObject para acceder a CompareTag
        {
            playerLife playerHealth = other.gameObject.GetComponent<playerLife>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(transform.position, damage);

            }
        }
    }
}
