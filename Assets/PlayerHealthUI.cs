using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ============================================
// UI DE SALUD Y POCIONES 
// ============================================

public class PlayerHealthUI : MonoBehaviour
{
    private playerLife player;

    // ============================================
    // TEXTO DE POCIONES
    // ============================================
    [Header("Texto de Pociones")]
    [SerializeField] private TextMeshProUGUI potionText;

    // ============================================
    // SPRITES DE LA ESPADA
    // ============================================
    [Header("Elementos de la Espada")]
    [SerializeField] private Image swordHandle;
    [SerializeField] private Image[] swordMiddleParts;
    [SerializeField] private Image swordTip;

    [Header("Sprites - Espada Llena")]
    [SerializeField] private Sprite handleFullSprite;
    [SerializeField] private Sprite middleFullSprite;
    [SerializeField] private Sprite tipFullSprite;

    [Header("Sprites - Espada Vacía")]
    [SerializeField] private Sprite handleEmptySprite;
    [SerializeField] private Sprite middleEmptySprite;
    [SerializeField] private Sprite tipEmptySprite;

    // ============================================
    // CABALLERO
    // ============================================
    [Header("Caballero")]
    [SerializeField] private Image knightImage;
    [SerializeField] private Image knightHeadImage;

    [Header("Sprites del Caballero por Vida")]
    [SerializeField] private Sprite knight5HealthSprite;
    [SerializeField] private Sprite knight4HealthSprite;
    [SerializeField] private Sprite knight3HealthSprite;
    [SerializeField] private Sprite knight2HealthSprite;
    [SerializeField] private Sprite knight1HealthSprite;

    // ============================================
    // POSICIONES Y MOVIMIENTO
    // ============================================
    [Header("Animación del Caballero")]
    [SerializeField] private float knightMoveDistancePerHealth = 50f;

    [Header("Offset de la Cabeza")]
    [SerializeField] private float headOffsetX = 60f;
    [SerializeField] private float headOffsetY = 0f;

    // ============================================
    // CONSTANTES
    // ============================================
    private const int HEAD_POSITION_OFFSET = 2;

    // ============================================
    // INICIALIZACION
    // ============================================

    public void Initialize(playerLife playerLife)
    {
        player = playerLife;
        UpdateDisplay();
    }

    // ============================================
    // ACTUALIZAR TODA LA UI
    // ============================================

    public void UpdateDisplay()
    {
        UpdatePotionText();
        UpdateHealthVisuals();
    }

    // ============================================
    // TEXTO DE POCIONES
    // ============================================

    private void UpdatePotionText()
    {
        if (potionText != null && player != null)
        {
            potionText.text = $"{player.Potions}/{player.MaxPotions}";
        }
    }

    // ============================================
    // VISUALES DE SALUD
    // ============================================

    private void UpdateHealthVisuals()
    {
        if (player == null) return;

        UpdateKnightSprite();
        UpdateKnightHeadSprite();
        UpdateSwordVisuals();
        UpdateKnightHeadPosition();
        UpdateKnightPosition();
    }

    // ============================================
    // CABALLERO - SPRITE
    // ============================================

    private void UpdateKnightSprite()
    {
        if (knightImage == null || player == null) return;

        knightImage.sprite = GetKnightSpriteForHealth(player.Health);
    }

    private void UpdateKnightHeadSprite()
    {
        if (knightHeadImage == null || player == null) return;

        knightHeadImage.sprite = GetKnightSpriteForHealth(player.Health);
    }

    private Sprite GetKnightSpriteForHealth(int health)
    {
        switch (health)
        {
            case 5: return knight5HealthSprite;
            case 4: return knight4HealthSprite;
            case 3: return knight3HealthSprite;
            case 2: return knight2HealthSprite;
            case 1: return knight1HealthSprite;
            default: return knight1HealthSprite;
        }
    }

    // ============================================
    // ESPADA - VISUALES
    // ============================================

    private void UpdateSwordVisuals()
    {
        if (player == null) return;

        UpdateSwordTip();
        UpdateSwordMiddleParts();
        UpdateSwordHandle();
    }

    private void UpdateSwordTip()
    {
        if (swordTip == null) return;

        swordTip.enabled = true;
        swordTip.sprite = (player.Health >= player.MaxHealth) ? tipFullSprite : tipEmptySprite;
    }

    private void UpdateSwordMiddleParts()
    {
        if (swordMiddleParts == null) return;

        for (int i = 0; i < swordMiddleParts.Length; i++)
        {
            if (swordMiddleParts[i] != null)
            {
                int healthThreshold = player.MaxHealth - HEAD_POSITION_OFFSET - i;

                swordMiddleParts[i].enabled = true;
                swordMiddleParts[i].sprite = (player.Health > healthThreshold)
                    ? middleFullSprite
                    : middleEmptySprite;
            }
        }
    }

    private void UpdateSwordHandle()
    {
        if (swordHandle == null) return;

        swordHandle.enabled = true;
        swordHandle.sprite = (player.Health > 0) ? handleFullSprite : handleEmptySprite;
    }

    // ============================================
    // POSICIONES
    // ============================================

    private void UpdateKnightHeadPosition()
    {
        if (knightHeadImage == null || swordMiddleParts == null || player == null)
            return;

        RectTransform headRect = knightHeadImage.GetComponent<RectTransform>();
        if (headRect == null) return;

        int segmentIndex = (player.MaxHealth - player.Health) - HEAD_POSITION_OFFSET;

        if (IsValidSegmentIndex(segmentIndex))
        {
            PositionHeadAtSegment(headRect, segmentIndex);
        }
        else if (player.Health > 0 && swordTip != null)
        {
            PositionHeadAtTip(headRect);
        }
    }

    private bool IsValidSegmentIndex(int index)
    {
        return index >= 0 && index < swordMiddleParts.Length && swordMiddleParts[index] != null;
    }

    private void PositionHeadAtSegment(RectTransform headRect, int segmentIndex)
    {
        RectTransform segmentRect = swordMiddleParts[segmentIndex].GetComponent<RectTransform>();
        if (segmentRect != null)
        {
            headRect.position = segmentRect.position;
        }
    }

    private void PositionHeadAtTip(RectTransform headRect)
    {
        RectTransform tipRect = swordTip.GetComponent<RectTransform>();
        if (tipRect != null)
        {
            Vector3 offset = new Vector3(headOffsetX, headOffsetY, 0);
            headRect.position = tipRect.position + offset;
        }
    }

    private void UpdateKnightPosition()
    {
        if (knightImage == null || player == null) return;

        RectTransform knightRect = knightImage.GetComponent<RectTransform>();
        if (knightRect == null) return;

        float healthLost = player.MaxHealth - player.Health;
        float moveAmount = healthLost * knightMoveDistancePerHealth;

        knightRect.anchoredPosition = new Vector2(-moveAmount, knightRect.anchoredPosition.y);
    }
}