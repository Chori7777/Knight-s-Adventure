using UnityEngine;

public class swordDamageScript : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("enemy"))
        {
            Debug.Log("Hit enemy!");

            // Buscar el script EnemyLife en el enemigo que colisiona
            var life = other.GetComponent<enemyLife>();
            if (life != null)
            {
                life.TakeDamage(1); // Aplica 1 de daño
            }
        }
    }
}
