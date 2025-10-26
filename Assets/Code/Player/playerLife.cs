using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// ============================================
// SISTEMA DE VIDA DEL JUGADOR (con muerte segura)
// ============================================
public class playerLife : MonoBehaviour
{
    // COMPONENTES
    private PlayerMovement controller;
    private PlayerHealthUI healthUI;
    private PlayerAnimationController animController;
    private Animator fallbackAnimator; // fallback por si no hay animController

    // VIDA
    [Header("Vida")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private int currentHealth = 5;
    public int Health => currentHealth;
    public int MaxHealth => maxHealth;

    // POCIONES
    [Header("Pociones")]
    [SerializeField] private int maxPotions = 5;
    [SerializeField] private int currentPotions = 3;
    [SerializeField] private int potionHealAmount = 1;
    [SerializeField] private float potionCooldown = 0.5f;
    private float lastPotionTime = -10f;
    public int Potions => currentPotions;
    public int MaxPotions => maxPotions;

    // DAÑO / INVENCIBILIDAD
    [Header("Sistema de Daño")]
    [SerializeField] private float invincibilityDuration = 1f;
    private float lastDamageTime = -10f;
    private bool isTakingDamage = false;
    public bool IsTakingDamage => isTakingDamage;

    // CONTROLES
    [Header("Controles")]
    [SerializeField] private KeyCode usePotionKey = KeyCode.R;

    // MUERTE
    private bool isDead = false;
    [Header("Muerte")]
    [Tooltip("Nombre del estado/clip de animación de muerte (si usás fallback por duración).")]
    [SerializeField] private string deathAnimationName = "Death";
    [Tooltip("Duración por defecto si no se encuentra la animación (fallback).")]
    [SerializeField] private float deathFallbackDuration = 1f;

    private void Awake()
    {
        controller = GetComponent<PlayerMovement>();
        healthUI = GetComponent<PlayerHealthUI>();
        animController = GetComponent<PlayerAnimationController>();
        fallbackAnimator = GetComponent<Animator>();

        if (healthUI == null)
            healthUI = gameObject.AddComponent<PlayerHealthUI>();

        // Inicializar UI/anim (si el controller necesita inicializar con PlayerMovement)
        if (animController != null && controller != null)
            animController.Initialize(controller);

        healthUI.Initialize(this);

        // iniciar vida
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();
    }

    private void Update()
    {
        HandlePotionInput();
    }

    // ---------- Pociones ----------
    private void HandlePotionInput()
    {
        if (Input.GetKeyDown(usePotionKey))
            TryUsePotion();
    }

    private void TryUsePotion()
    {
        if (!CanUsePotion()) return;

        currentPotions--;
        currentHealth = Mathf.Min(currentHealth + potionHealAmount, maxHealth);
        lastPotionTime = Time.time;
        UpdateUI();
        Debug.Log($"🧪 Poción usada | Salud: {currentHealth}/{maxHealth} | Pociones: {currentPotions}/{maxPotions}");
    }

    private bool CanUsePotion()
    {
        if (Time.time - lastPotionTime < potionCooldown) return false;
        if (currentPotions <= 0) return false;
        if (currentHealth >= maxHealth) return false;
        return true;
    }

    public void AddPotion(int amount = 1)
    {
        currentPotions = Mathf.Min(currentPotions + amount, maxPotions);
        UpdateUI();
    }

    // ---------- Daño ----------
    public void TakeDamage(Vector2 attackerPosition, int damage)
    {
        if (!CanTakeDamage()) return;

        // marque daño ahora
        lastDamageTime = Time.time;

        // aplicar daño
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        UpdateUI();

        // aplicar knockback/efecto en el movimiento
        if (controller != null)
            controller.TakeDamage(attackerPosition);

        // activar animación de daño
        if (animController != null)
            animController.TriggerDamage();
        else if (fallbackAnimator != null)
            fallbackAnimator.SetBool("damage", true);

        isTakingDamage = true;

        // check muerte
        if (currentHealth <= 0)
        {
            StartCoroutine(HandleDeathSequence());
        }
    }

    private bool CanTakeDamage()
    {
        if (isDead) return false;
        if (currentHealth <= 0) return false;
        if (Time.time - lastDamageTime < invincibilityDuration) return false;
        return true;
    }

    public void StopDamageAnimation()
    {
        if (animController != null)
            animController.StopDamage();
        else if (fallbackAnimator != null)
            fallbackAnimator.SetBool("damage", false);

        isTakingDamage = false;
    }

    // ---------- Muerte segura ----------
    private IEnumerator HandleDeathSequence()
    {
        if (isDead) yield break;
        isDead = true;

        // Desactivar controles inmediatamente
        DisableAllControls();

        // Asegurar que el flag damage quede desactivado y que el Animator no interponga la transición
        StopDamageAnimation();

        // Esperar un frame para que el Animator "procese" el cambio de bools
        yield return null;

        // Pedir al animController que dispare la muerte, o al fallback Animator
        if (animController != null)
        {
            animController.TriggerDeath();

            // Si el animController tiene evento OnDeathAnimationEnd llamará a OnDeathAnimationEnd().
            // Si no hay evento, usamos la duración del clip como fallback.

        }
        else if (fallbackAnimator != null)
        {
            // fallback: trigger/flag
            fallbackAnimator.ResetTrigger("DoubleJump");
            fallbackAnimator.ResetTrigger("Throw");
            fallbackAnimator.SetBool("damage", false);
            fallbackAnimator.SetTrigger("Death");

            // intentar obtener duración del clip desde runtimeAnimatorController
            float clipLength = deathFallbackDuration;
            var rac = fallbackAnimator.runtimeAnimatorController;
            if (rac != null)
            {
                foreach (var clip in rac.animationClips)
                {
                    if (clip.name == deathAnimationName)
                    {
                        clipLength = clip.length;
                        break;
                    }
                }
            }
            yield return new WaitForSeconds(Mathf.Max(0.01f, clipLength));
        }
        else
        {
            // ni animController ni Animator están presentes: fallback corto
            yield return new WaitForSeconds(deathFallbackDuration);
        }

        // Todo lo que quieras hacer después de la animación:
        OnDeathComplete();
    }

    // Este método puede ser llamado también desde un evento de animación al final del clip de muerte
    public void OnDeathAnimationEnd()
    {
        // Si el método es llamado por anim event, asegúrate de no ejecutar dos veces:
        if (!isDead) isDead = true;
        // Llamar a la rutina final
        OnDeathComplete();
    }

    private void OnDeathComplete()
    {
        Debug.Log("☠️ Muerte completa: cargando GameOver");
        // aquí no destruimos el GameObject por si tenés algún audio/efecto; simplemente cargamos la escena
        SceneManager.LoadScene("GameOver");
    }

    private void DisableAllControls()
    {
        if (controller == null) return;

        // fijate que PlayerMovement tenga estas propiedades públicas
        controller.canMove = false;
        controller.canJump = false;
        controller.canAttack = false;
        controller.canDash = false;
        controller.canWallCling = false;
        controller.canBlock = false;
    }

    // ---------- Curación y utilidades ----------
    public void Heal(int amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateUI();
    }

    public void HealFull()
    {
        if (isDead) return;
        currentHealth = maxHealth;
        UpdateUI();
    }

    // ---------- Colisiones ----------
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("HealthPotion"))
        {
            AddPotion();
            Destroy(collision.gameObject);
        }

        if (collision.CompareTag("Consumable"))
        {
            Destroy(collision.gameObject);
            UpdateUI();
        }
    }

    // ---------- UI ----------
    private void UpdateUI()
    {
        if (healthUI != null)
            healthUI.UpdateDisplay();
    }

    // ---------- Métodos para guardado ----------
    public void SetHealth(int health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        UpdateUI();
    }

    public void SetMaxHealth(int max)
    {
        maxHealth = max;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        UpdateUI();
    }

    public void SetPotions(int potions)
    {
        currentPotions = Mathf.Clamp(potions, 0, maxPotions);
        UpdateUI();
    }

    public void SetMaxPotions(int max)
    {
        maxPotions = max;
        currentPotions = Mathf.Min(currentPotions, maxPotions);
        UpdateUI();
    }
}
