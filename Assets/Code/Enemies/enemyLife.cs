using UnityEngine;


public class enemyLife : MonoBehaviour
{
    public int health = 3;
    public int maxHealth = 3;
    public bool Boss = false;
    public bool isDead = false;
    Animator anim;

    void Awake()
    {
        health = maxHealth;
    }

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health < 0) health = 0;
        anim.SetBool("damage", true);
        Debug.Log("enemy health: " + health);

        if (health <= 0)
        {
            anim.SetBool("damage", false);
            anim.SetBool("Death", true);

            // Si es un jefe, abrimos las puertas
            if (Boss)
            {
                AbrirPuertasDelJefe();
            }

            // Destruir el objeto después de 1 segundo para que se vea la animación de muerte
            Destroy(gameObject, 1f);
            Debug.Log("enemigo muerto");
        }
    }

    private void AbrirPuertasDelJefe()
    {
        Debug.Log("🔍 Buscando BossTrigger...");

        // Buscamos el BossTrigger en la escena
        BossTrigger bossTrigger = Object.FindFirstObjectByType<BossTrigger>();

        if (bossTrigger != null)
        {
            Debug.Log("✅ BossTrigger encontrado");
            bossTrigger.JefeDerotado();
            Debug.Log("¡Jefe derrotado! Puertas abiertas");
        }
        else
        {
            Debug.LogError("❌ No se encontró BossTrigger en la escena");
        }
    }

    public void Die()
    {
        anim.SetBool("Death", false);
    }

    public void StopDmg()
    {
        anim.SetBool("damage", false);
    }
}