using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class playerLife : MonoBehaviour
{
    public int health = 5;
    public int maxHealth = 5;
    public bool recibiendoDmg;
    public int botellaVida = 3;
    public int maxbotellaVida = 5;

    public TextMeshProUGUI cantidadVida;
    Animator anim;

    [Header("Cooldown")]
    [SerializeField] private float invencibilityCooldown = 1f;
    private float lastDamageTime = -1f;

    [Header("Pociones")]
    [SerializeField] private float potionCooldown = 0.5f;
    private float lastPotionTime = -1f;

    [Header("UI - Espada")]
    public Image swordHandle;
    public Image[] swordMiddle;
    public Image swordTip;
    public Image knightHead;

    [Header("UI - Caballero")]
    public Image knightSprite;
    [SerializeField] private Sprite knight5Health;
    [SerializeField] private Sprite knight4Health;
    [SerializeField] private Sprite knight3Health;
    [SerializeField] private Sprite knight2Health;
    [SerializeField] private Sprite knight1Health;

    [Header("Posición del Caballero")]
    [SerializeField] private float knightMoveDistance = 50f;

    [Header("Posición de la Cabeza")]
    [SerializeField] private float headOffsetX = 60f;
    [SerializeField] private float headOffsetY = 0f;

    [Header("Sprites - Espada Llena")]
    [SerializeField] private Sprite handleFull;
    [SerializeField] private Sprite middleFull;
    [SerializeField] private Sprite tipFull;

    [Header("Sprites - Espada Vacía")]
    [SerializeField] private Sprite handleEmpty;
    [SerializeField] private Sprite middleEmpty;
    [SerializeField] private Sprite tipEmpty;

    private PlayerMovement controlador;

    void Awake()
    {
        health = maxHealth;
        UpdateHealthUI();
        controlador = GetComponent<PlayerMovement>();
    }

    void Start()
    {
        anim = GetComponent<Animator>();
        ActualizarTexto();
    }

    private void Update()
    {
        // Actualizar UI de pociones cada frame
        ActualizarTexto();

        // Detectar presion de tecla E para usar pocion (GetKeyDown = solo una vez)
        if (Input.GetKeyDown(KeyCode.R))
        {
            UsarPocion();
        }
    }

    // Usar una pocion cuando presione E
    private void UsarPocion()
    {
        // Verificar cooldown
        if (Time.time - lastPotionTime < potionCooldown)
        {
            Debug.Log("Pocion en cooldown");
            return;
        }

        // Verificar si tiene pociones
        if (botellaVida <= 0)
        {
            Debug.Log("No tienes pociones");
            return;
        }

        // Verificar si ya tiene salud maxima
        if (health >= maxHealth)
        {
            Debug.Log("Ya tienes salud maxima");
            return;
        }

        // Usar pocion
        botellaVida--;
        health++;
        lastPotionTime = Time.time;

        Debug.Log("Pocion usada. Salud: " + health + " | Pociones: " + botellaVida);
        UpdateHealthUI();
    }

    // Actualizar texto de pociones en UI
    public void ActualizarTexto()
    {
        if (cantidadVida != null)
        {
            cantidadVida.text = botellaVida.ToString() + "/" + maxbotellaVida.ToString();
        }

        // Limitar pociones al maximo
        if (botellaVida > maxbotellaVida)
        {
            botellaVida = maxbotellaVida;
        }
    }

    public void RecibeDano(Vector2 atacantePosicion, int damage)
    {
        // Verificar si está en cooldown de invencibilidad
        if (Time.time - lastDamageTime < invencibilityCooldown)
        {
            Debug.Log("En cooldown de invencibilidad");
            return;
        }

        if (health <= 0) return;

        lastDamageTime = Time.time;
        health -= damage;
        if (health < 0) health = 0;
        Debug.Log("player health: " + health);
        UpdateHealthUI();

        if (controlador != null)
        {
            controlador.RecibirDaño(atacantePosicion);
        }

        anim.SetBool("damage", true);
        recibiendoDmg = true;

        if (health <= 0)
        {
            anim.SetBool("damage", false);
            anim.SetTrigger("Death");
            if (controlador != null)
            {
                controlador.puedeMoverse = false;
                controlador.puedeSaltar = false;
                controlador.puedeAtacar = false;
                controlador.puedeDash = false;
                controlador.puedeWallCling = false;
                controlador.puedeBloquear = false;
            }
        }
    }

    public void Die()
    {
        Destroy(gameObject);
        SceneManager.LoadScene("GameOver");
    }

    public void StopDmg()
    {
        anim.SetBool("damage", false);
        if (controlador != null)
            controlador.recibiodaño = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Colisioné con: " + collision.gameObject.name + " - Tag: " + collision.tag);

        if (collision.CompareTag("Consumable"))
        {
            Destroy(collision.gameObject);
            UpdateHealthUI();
        }

        if (collision.CompareTag("HealthPotion"))
        {
            botellaVida = Mathf.Min(botellaVida + 1, maxbotellaVida);
            Destroy(collision.gameObject);
            ActualizarTexto();
            Debug.Log("Pocion recogida. Total: " + botellaVida);
        }
    }

    void UpdateHealthUI()
    {
        // Determinar estado del caballero
        KnightState state = GetKnightState();

        // Actualizar sprite del caballero
        UpdateKnightSprite(state);

        // Actualizar sprite de la cabeza del caballero
        UpdateKnightHead(state);

        // Actualizar sprites de la espada
        UpdateSwordSprites(state);

        // Actualizar posición de la cabeza
        UpdateKnightHeadPosition();

        // Actualizar posición del caballero en el HUD
        UpdateKnightPosition();
    }

    private KnightState GetKnightState()
    {
        switch (health)
        {
            case 5:
                return KnightState.Health5;
            case 4:
                return KnightState.Health4;
            case 3:
                return KnightState.Health3;
            case 2:
                return KnightState.Health2;
            case 1:
                return KnightState.Health1;
            default:
                return KnightState.Health1;
        }
    }

    private void UpdateKnightSprite(KnightState state)
    {
        if (knightSprite != null)
        {
            switch (state)
            {
                case KnightState.Health5:
                    knightSprite.sprite = knight5Health;
                    break;
                case KnightState.Health4:
                    knightSprite.sprite = knight4Health;
                    break;
                case KnightState.Health3:
                    knightSprite.sprite = knight3Health;
                    break;
                case KnightState.Health2:
                    knightSprite.sprite = knight2Health;
                    break;
                case KnightState.Health1:
                    knightSprite.sprite = knight1Health;
                    break;
            }
        }
    }

    private void UpdateKnightHead(KnightState state)
    {
        if (knightHead != null)
        {
            switch (state)
            {
                case KnightState.Health5:
                    knightHead.sprite = knight5Health;
                    break;
                case KnightState.Health4:
                    knightHead.sprite = knight4Health;
                    break;
                case KnightState.Health3:
                    knightHead.sprite = knight3Health;
                    break;
                case KnightState.Health2:
                    knightHead.sprite = knight2Health;
                    break;
                case KnightState.Health1:
                    knightHead.sprite = knight1Health;
                    break;
            }
        }
    }

    private void UpdateSwordSprites(KnightState state)
    {
        // Calcular cuántos segmentos activos hay
        int activeParts = health;

        // Actualizar punta (desaparece primero)
        if (swordTip != null)
        {
            if (health >= maxHealth)
            {
                swordTip.enabled = true;
                swordTip.sprite = tipFull;
            }
            else
            {
                swordTip.enabled = true;
                swordTip.sprite = tipEmpty;
            }
        }

        // Actualizar partes del medio
        if (swordMiddle != null && swordMiddle.Length > 0)
        {
            for (int i = 0; i < swordMiddle.Length; i++)
            {
                if (swordMiddle[i] != null)
                {
                    int threshold = maxHealth - 2 - i;

                    if (health > threshold)
                    {
                        swordMiddle[i].enabled = true;
                        swordMiddle[i].sprite = middleFull;
                    }
                    else
                    {
                        swordMiddle[i].enabled = true;
                        swordMiddle[i].sprite = middleEmpty;
                    }
                }
            }
        }

        // Actualizar mango (desaparece al final)
        if (swordHandle != null)
        {
            if (health > 0)
            {
                swordHandle.enabled = true;
                swordHandle.sprite = handleFull;
            }
            else
            {
                swordHandle.enabled = true;
                swordHandle.sprite = handleEmpty;
            }
        }
    }

    private void UpdateKnightHeadPosition()
    {
        if (knightHead != null && swordMiddle != null && swordMiddle.Length > 0)
        {
            int segmentIndex = (maxHealth - health) - 2;

            RectTransform headRect = knightHead.GetComponent<RectTransform>();
            if (headRect != null)
            {
                if (segmentIndex >= 0 && segmentIndex < swordMiddle.Length && swordMiddle[segmentIndex] != null)
                {
                    RectTransform segmentRect = swordMiddle[segmentIndex].GetComponent<RectTransform>();
                    if (segmentRect != null)
                    {
                        headRect.position = segmentRect.position;
                    }
                }
                else if (health > 0 && swordTip != null)
                {
                    RectTransform tipRect = swordTip.GetComponent<RectTransform>();
                    if (tipRect != null)
                    {
                        headRect.position = tipRect.position + new Vector3(headOffsetX, headOffsetY, 0);
                    }
                }
            }
        }
    }

    private void UpdateKnightPosition()
    {
        if (knightSprite != null)
        {
            RectTransform knightRect = knightSprite.GetComponent<RectTransform>();
            if (knightRect != null)
            {
                float healthLost = maxHealth - health;
                float moveAmount = healthLost * knightMoveDistance;

                knightRect.anchoredPosition = new Vector2(-moveAmount, knightRect.anchoredPosition.y);
            }
        }
    }

    private enum KnightState
    {
        Health5,
        Health4,
        Health3,
        Health2,
        Health1
    }

}