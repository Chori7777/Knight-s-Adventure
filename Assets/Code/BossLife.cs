using UnityEngine;
using System.Collections;

public class BossLife : MonoBehaviour
    {
        [Header("Identificación")]
        public string bossID = "Boss1"; // ID único del jefe

        [Header("Vida")]
        public int health = 10;
        public int maxHealth = 10;
        private bool isDead = false;

        [Header("Componentes")]
        private Animator anim;
        private Rigidbody2D rb;

        [Header("Checkpoint")]
        [SerializeField] private GameObject savePointPrefab;
        [SerializeField] private Vector3 savePointSpawnPosition;
        [SerializeField] private bool spawnSavePointOnDeath = true;

        [Header("Knockback")]
        [SerializeField] private float knockbackForce = 6f;
        [SerializeField] private float knockbackRecoveryTime = 0.4f;
        private bool recibiendoDanio = false;

        // Referencia al trigger
        private BossTrigger bossTrigger;

        void Awake()
        {
            health = maxHealth;
            anim = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();

            // Si no hay posición de savepoint, usar la del jefe
            if (savePointSpawnPosition == Vector3.zero)
            {
                savePointSpawnPosition = transform.position;
            }
        }

        /// <summary>
        /// Asignar el trigger que spawneó al jefe
        /// </summary>
        public void SetBossTrigger(BossTrigger trigger)
        {
            bossTrigger = trigger;
            Debug.Log($"✅ BossTrigger asignado a {bossID}");
        }

        /// <summary>
        /// Recibir daño (llamado por enemyBasicMovement o desde colliders)
        /// </summary>
        public void TakeDamage(int damage)
        {
            if (isDead || recibiendoDanio) return;

            health -= damage;
            if (health < 0) health = 0;

            Debug.Log($"🗡️ {bossID} recibió {damage} de daño. Vida: {health}/{maxHealth}");

            // Animación de daño
            if (anim != null)
            {
                anim.SetBool("damage", true);
            }

            // Morir o aplicar knockback
            if (health <= 0)
            {
                Die();
            }
            else
            {
                StartCoroutine(RecuperarDeKnockback());
            }
        }

        /// <summary>
        /// Recibir daño con knockback (versión completa)
        /// </summary>
        public void RecibeDanio(Vector2 direccionAtaque, int cantDanio)
        {
            if (isDead || recibiendoDanio) return;

            health -= cantDanio;
            if (health < 0) health = 0;

            recibiendoDanio = true;

            Debug.Log($"🗡️ {bossID} recibió {cantDanio} de daño. Vida: {health}/{maxHealth}");

            // Animación de daño
            if (anim != null)
            {
                anim.SetBool("damage", true);
            }

            if (health <= 0)
            {
                Die();
            }
            else
            {
                // Aplicar knockback
                if (rb != null)
                {
                    Vector2 knockDir = ((Vector2)transform.position - direccionAtaque).normalized;
                    knockDir.y = Mathf.Clamp(knockDir.y + 0.5f, 0.5f, 1f);
                    rb.linearVelocity = Vector2.zero;
                    rb.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);
                }

                StartCoroutine(RecuperarDeKnockback());
            }
        }

        private IEnumerator RecuperarDeKnockback()
        {
            yield return new WaitForSeconds(knockbackRecoveryTime);
            recibiendoDanio = false;

            if (anim != null)
            {
                anim.SetBool("damage", false);
            }

            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }

        /// <summary>
        /// Morir
        /// </summary>
        private void Die()
        {
            if (isDead) return;

            isDead = true;
            recibiendoDanio = false;

            Debug.Log($"💀 {bossID} ha muerto.");

            // Animación de muerte
            if (anim != null)
            {
                anim.SetBool("damage", false);
                anim.SetBool("Death", true);
            }

            // Detener movimiento
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            // Desactivar colisiones
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            // Desactivar scripts de ataque/movimiento
            var attackScript = GetComponent<EnemyBasicAttack>();
            if (attackScript != null) attackScript.enabled = false;


            StartCoroutine(DeathSequence());
        }

        private IEnumerator DeathSequence()
        {
            // 1. Notificar al trigger PRIMERO
            if (bossTrigger != null)
            {
                Debug.Log("📢 Notificando al BossTrigger...");
                bossTrigger.JefeDerotado();
            }
            else
            {
                Debug.LogError("❌ No hay referencia al BossTrigger!");
            }

            // 2. Crear savepoint
            if (spawnSavePointOnDeath && savePointPrefab != null)
            {
                Vector3 spawnPos = (savePointSpawnPosition == Vector3.zero) ?
                                   transform.position : savePointSpawnPosition;
                Instantiate(savePointPrefab, spawnPos, Quaternion.identity);
                Debug.Log($"💾 SavePoint creado en: {spawnPos}");
            }

            // 3. Esperar animación de muerte
            yield return new WaitForSeconds(1.5f);

            // 4. Destruir el jefe
            Debug.Log($"🗑️ Destruyendo {bossID}");
            Destroy(gameObject);
        }

        // Método llamado por eventos de animación (opcional)
        public void StopDmg()
        {
            if (anim != null)
            {
                anim.SetBool("damage", false);
            }
        }

        // Gizmos para visualizar checkpoint
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Vector3 spawnPos = (savePointSpawnPosition == Vector3.zero) ?
                              transform.position : savePointSpawnPosition;
            Gizmos.DrawWireSphere(spawnPos, 1f);
            Gizmos.DrawLine(spawnPos, spawnPos + Vector3.up * 2f);
        }
    }