using UnityEngine;

public class enemyLife : MonoBehaviour
{
    [Header("Vida del enemigo")]
    public int health = 3;
    public int maxHealth = 3;
    public bool isDead = false;

    private Animator anim;

    private void Awake()
    {
        health = maxHealth;
    }

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        health -= damage;
        if (health < 0) health = 0;

        anim.SetBool("damage", true);
        Debug.Log($"💢 Daño recibido. Vida restante: {health}");

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        anim.SetBool("damage", false);
        anim.SetBool("Death", true);

        Debug.Log("☠️ Enemigo muriendo...");
        Destroy(gameObject, 1f);
    }

    // Llamada desde animación
    public void StopDmg()
    {
        anim.SetBool("damage", false);
    }

    public void OnDeathAnimationEnd()
    {
        anim.SetBool("Death", false);
    }
}
