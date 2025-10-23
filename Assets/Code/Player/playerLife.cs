using UnityEngine;
using UnityEngine.SceneManagement;

// ============================================
// SISTEMA DE VIDA DEL JUGADOR
// ============================================

public class playerLife : MonoBehaviour
{
    // ============================================
    // COMPONENTES
    // ============================================
    private Animator anim;
    private PlayerMovement controller;
    private PlayerHealthUI healthUI;

    // ============================================
    // VIDA
    // ============================================
    [Header("Vida")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private int currentHealth = 5;

    public int Health => currentHealth;
    public int MaxHealth => maxHealth;

    // ============================================
    // POCIONES
    // ============================================
    [Header("Pociones")]
    [SerializeField] private int maxPotions = 5;
    [SerializeField] private int currentPotions = 3;
    [SerializeField] private int potionHealAmount = 1;
    [SerializeField] private float potionCooldown = 0.5f;

    private float lastPotionTime = -1f;

    public int Potions => currentPotions;
    public int MaxPotions => maxPotions;

    // ============================================
    // DAÑO
    // ============================================
    [Header("Sistema de Daño")]
    [SerializeField] private float invincibilityDuration = 1f;

    private float lastDamageTime = -1f;
    private bool isTakingDamage;

    public bool IsTakingDamage => isTakingDamage;

    // ============================================
    // TECLAS
    // ============================================
    [Header("Controles")]
    [SerializeField] private KeyCode usePotionKey = KeyCode.R;

    // ============================================
    // INICIALIZACION
    // ============================================

    private void Awake()
    {
        InitializeComponents();
        InitializeHealth();
    }

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void InitializeComponents()
    {
        controller = GetComponent<PlayerMovement>();
        healthUI = GetComponent<PlayerHealthUI>();

        if (healthUI == null)
        {
            healthUI = gameObject.AddComponent<PlayerHealthUI>();
        }

        healthUI.Initialize(this);
    }

    private void InitializeHealth()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    // ============================================
    // UPDATE
    // ============================================

    private void Update()
    {
        HandlePotionInput();
    }

    // ============================================
    // INPUT
    // ============================================

    private void HandlePotionInput()
    {
        if (Input.GetKeyDown(usePotionKey))
        {
            TryUsePotion();
        }
    }

    // ============================================
    // SISTEMA DE POCIONES
    // ============================================

    private void TryUsePotion()
    {
        if (!CanUsePotion())
        {
            return;
        }

        UsePotion();
    }

    private bool CanUsePotion()
    {
        if (Time.time - lastPotionTime < potionCooldown)
        {
            Debug.Log("⏱️ Poción en cooldown");
            return false;
        }

        if (currentPotions <= 0)
        {
            Debug.Log("❌ No tienes pociones");
            return false;
        }

        if (currentHealth >= maxHealth)
        {
            Debug.Log("💚 Ya tienes salud máxima");
            return false;
        }

        return true;
    }

    private void UsePotion()
    {
        currentPotions--;
        currentHealth = Mathf.Min(currentHealth + potionHealAmount, maxHealth);
        lastPotionTime = Time.time;

        UpdateUI();
        Debug.Log($"🧪 Poción usada | Salud: {currentHealth}/{maxHealth} | Pociones: {currentPotions}/{maxPotions}");
    }

    public void AddPotion(int amount = 1)
    {
        currentPotions = Mathf.Min(currentPotions + amount, maxPotions);
        UpdateUI();
        Debug.Log($"🧪 Poción recogida | Total: {currentPotions}/{maxPotions}");
    }

    // ============================================
    // SISTEMA DE DAÑO
    // ============================================

    public void TakeDamage(Vector2 attackerPosition, int damage)
    {
        if (!CanTakeDamage())
        {
            return;
        }

        ApplyDamage(damage);
        ApplyKnockback(attackerPosition);
        PlayDamageAnimation();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private bool CanTakeDamage()
    {
        if (Time.time - lastDamageTime < invincibilityDuration)
        {
            Debug.Log(" Invencibilidad activa");
            return false;
        }

        if (currentHealth <= 0)
        {
            return false;
        }

        return true;
    }

    private void ApplyDamage(int damage)
    {
        lastDamageTime = Time.time;
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        UpdateUI();
        Debug.Log($" Daño recibido: {currentHealth}");
    }

    private void ApplyKnockback(Vector2 attackerPosition)
    {
        if (controller != null)
        {
            controller.TakeDamage(attackerPosition);
        }
    }

    private void PlayDamageAnimation()
    {
        anim.SetBool("damage", true);
        isTakingDamage = true;
    }

    private void Die()
    {
        anim.SetBool("damage", false);
        anim.SetTrigger("Death");
        DisableAllControls();
        Debug.Log(" Jugador muerto");
    }

    private void DisableAllControls()
    {
        if (controller != null)
        {
            controller.canMove = false;
            controller.canJump = false;
            controller.canAttack = false;
            controller.canDash = false;
            controller.canWallCling = false;
            controller.canBlock = false;
        }
    }

    public void OnDeathAnimationComplete()
    {
        Destroy(gameObject);
        SceneManager.LoadScene("GameOver");
    }

    public void StopDamageAnimation()
    {
        anim.SetBool("damage", false);
        isTakingDamage = false;
    }

    // ============================================
    // CURACION
    // ============================================

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateUI();
        Debug.Log($" Curación {currentHealth}");
    }

    public void HealFull()
    {
        currentHealth = maxHealth;
        UpdateUI();
        Debug.Log($"✨ Curación completa | Salud: {currentHealth}/{maxHealth}");
    }

    // ============================================
    // COLISIONES
    // ============================================

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

    // ============================================
    // UI
    // ============================================

    private void UpdateUI()
    {
        if (healthUI != null)
        {
            healthUI.UpdateDisplay();
        }
    }

    // ============================================
    // METODOS PUBLICOS PARA GUARDADO
    // ============================================

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