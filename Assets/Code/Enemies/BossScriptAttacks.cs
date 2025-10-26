﻿using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Sistema de ataques del Golem de Piedra (VERSIÓN SIMPLE)
/// </summary>
public class StoneGolemAttacks : MonoBehaviour
{
    [Header("Referencias")]
    private bossCore core;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject alertaPrefab;

    [Header("Configuración General")]
    [SerializeField] private float timeBetweenAttacks = 3f;
    [SerializeField] private float minDistanceForMelee = 2f;
    [SerializeField] private float alertDuration = 0.8f;

    [Header("Ataque: Lluvia de Piedras")]
    [SerializeField] private int stonesPerRain = 5;
    [SerializeField] private float rainSpread = 8f;
    [SerializeField] private float rainHeight = 10f;
    [SerializeField] private float timeBetweenStones = 0.2f;
    [SerializeField] private float jumpForce = 5f;

    [Header("Ataque: Embestida")]
    [SerializeField] private float chargeSpeedMultiplier = 2f;
    [SerializeField] private float chargeDuration = 2f;

    [Header("Ataque: Golpe Melee")]
    [SerializeField] private float meleeDuration = 1.5f;

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float moveDuration = 1.5f;

    private GameObject currentAlert;
    private bool isMoving = false;

    // ============================================
    // INICIALIZACIÓN
    // ============================================
    private void Start()
    {
        // Buscar BossCore
        core = GetComponent<bossCore>();

        if (core == null)
        {
            Debug.LogError($"❌ {gameObject.name}: No se encontró BossCore!");
            enabled = false;
            return;
        }

        // Crear alerta si existe
        if (alertaPrefab != null)
        {
            currentAlert = Instantiate(alertaPrefab, transform);
            currentAlert.SetActive(false);
        }

        // Crear spawn point si no existe
        if (spawnPoint == null)
        {
            GameObject spawnObj = new GameObject("ProjectileSpawnPoint");
            spawnObj.transform.SetParent(transform);
            spawnObj.transform.localPosition = new Vector3(0, 2f, 0);
            spawnPoint = spawnObj.transform;
        }

        Debug.Log($"✅ {gameObject.name}: Sistema de ataques iniciado");

        // Iniciar loop de ataques
        StartCoroutine(AttackLoop());
    }

    // ============================================
    // LOOP PRINCIPAL
    // ============================================
    private IEnumerator AttackLoop()
    {
        Debug.Log($"🔥 {gameObject.name}: Loop de ataques iniciado");

        yield return new WaitForSeconds(1f); // Espera inicial

        int attackCount = 0;

        while (!core.IsDead)
        {
            attackCount++;
            Debug.Log($"⚔️ Ataque #{attackCount} - Distancia al jugador: {core.DistanceToPlayer():F2}");

            if (!core.IsAttacking && core.player != null)
            {
                // Decidir qué ataque hacer
                float distance = core.DistanceToPlayer();

                if (distance <= minDistanceForMelee)
                {
                    Debug.Log("👊 Ejecutando: Golpe Melee");
                    yield return StartCoroutine(MeleeAttack());
                }
                else
                {
                    int attackChoice = Random.Range(0, 2);

                    if (attackChoice == 0)
                    {
                        Debug.Log("🪨 Ejecutando: Lluvia de Piedras");
                        yield return StartCoroutine(StoneRainAttack());
                    }
                    else
                    {
                        Debug.Log("🏃 Ejecutando: Embestida");
                        yield return StartCoroutine(ChargeAttack());
                    }
                }
            }

            Debug.Log($"⏳ Esperando {timeBetweenAttacks}s...");
            yield return new WaitForSeconds(timeBetweenAttacks);
        }
        Debug.Log($"💀 {gameObject.name}: Loop terminado (jefe muerto)");
    }

    // ============================================
    // MOVIMIENTO (cuando no ataca)
    // ============================================
    private void Update()
    {
        if (core == null || core.IsDead) return;

        // Mirar al jugador
        core.FacePlayer();

        // Movimiento estratégico
        if (!core.IsAttacking && !isMoving)
        {
            StartCoroutine(StrategicMovement());
        }
    }

    private IEnumerator StrategicMovement()
    {
        isMoving = true;

        float duration = Random.Range(1f, moveDuration);
        float elapsed = 0f;

        bool shouldApproach = Random.value > 0.5f;
        Vector2 direction = core.DirectionToPlayer();

        if (!shouldApproach) direction = -direction;

        while (elapsed < duration && !core.IsAttacking)
        {
            core.rb.linearVelocity = new Vector2(direction.x * moveSpeed, core.rb.linearVelocity.y);
            elapsed += Time.deltaTime;
            yield return null;
        }

        core.rb.linearVelocity = new Vector2(0, core.rb.linearVelocity.y);
        isMoving = false;
    }

    // ============================================
    // ATAQUE 1: LLUVIA DE PIEDRAS
    // ============================================
    private IEnumerator StoneRainAttack()
    {
        core.IsAttacking = true;
        core.rb.linearVelocity = Vector2.zero;

        // Alerta
        yield return StartCoroutine(ShowAlert());

        // Salto
        core.rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.5f);

        // Generar piedras
        for (int i = 0; i < stonesPerRain; i++)
        {
            SpawnStone();
            yield return new WaitForSeconds(timeBetweenStones);
        }

        yield return new WaitForSeconds(0.5f);
        core.IsAttacking = false;
    }

    private void SpawnStone()
    {
        if (projectilePrefab == null || spawnPoint == null)
        {
            Debug.LogWarning("⚠️ Falta projectilePrefab o spawnPoint");
            return;
        }

        Vector3 spawnPosition = spawnPoint.position;
        float randomX = Random.Range(-rainSpread, rainSpread);
        spawnPosition.x += randomX;
        spawnPosition.y += rainHeight;

        Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
    }

    // ============================================
    // ATAQUE 2: EMBESTIDA
    // ============================================
    private IEnumerator ChargeAttack()
    {
        core.IsAttacking = true;
        core.rb.linearVelocity = Vector2.zero;

        // Alerta
        yield return StartCoroutine(ShowAlert());

        // Animación
        if (core.anim != null)
        {
            core.anim.SetTrigger("Run");
        }

        // Cargar
        Vector2 chargeDirection = core.DirectionToPlayer();
        float chargeSpeed = moveSpeed * chargeSpeedMultiplier;

        float elapsedTime = 0f;
        while (elapsedTime < chargeDuration)
        {
            core.rb.linearVelocity = new Vector2(chargeDirection.x * chargeSpeed, core.rb.linearVelocity.y);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        core.rb.linearVelocity = Vector2.zero;

        // Piedras al final
        for (int i = 0; i < stonesPerRain; i++)
        {
            SpawnStone();
            yield return new WaitForSeconds(timeBetweenStones);
        }

        yield return new WaitForSeconds(0.5f);
        core.IsAttacking = false;
    }

    // ============================================
    // ATAQUE 3: GOLPE MELEE
    // ============================================
    private IEnumerator MeleeAttack()
    {
        core.IsAttacking = true;
        core.rb.linearVelocity = Vector2.zero;

        // Alerta corta
        yield return StartCoroutine(ShowAlert(0.5f));

        // Animación
        if (core.anim != null)
        {
            core.anim.SetTrigger("Attack");
        }

        yield return new WaitForSeconds(meleeDuration);

        core.IsAttacking = false;
    }

    // ============================================
    // ALERTA
    // ============================================
    private IEnumerator ShowAlert(float duration = -1)
    {
        if (duration < 0) duration = alertDuration;

        if (currentAlert != null)
        {
            currentAlert.SetActive(true);
            Animator alertAnim = currentAlert.GetComponent<Animator>();
            if (alertAnim != null)
            {
                alertAnim.SetTrigger("Alert");
            }
        }

        yield return new WaitForSeconds(duration);

        if (currentAlert != null)
        {
            currentAlert.SetActive(false);
        }
    }
}