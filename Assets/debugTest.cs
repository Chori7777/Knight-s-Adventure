using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Script de prueba simple para verificar que el jefe funciona
/// INSTRUCCIONES: Agregá este script al jefe (temporalmente) y mirá la consola
/// </summary>
public class BossDebugTest : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("====== TEST DEL JEFE ======");
        StartCoroutine(RunDiagnostics());
    }

    private IEnumerator RunDiagnostics()
    {
        yield return new WaitForSeconds(0.5f);

        // Test 1: Componentes básicos
        Debug.Log("--- TEST 1: Componentes Básicos ---");
        var rb = GetComponent<Rigidbody2D>();
        var anim = GetComponent<Animator>();
        var col = GetComponent<Collider2D>();

        Debug.Log($"Rigidbody2D: {(rb != null ? "✅" : "❌")}");
        Debug.Log($"Animator: {(anim != null ? "✅" : "❌")}");
        Debug.Log($"Collider2D: {(col != null ? "✅" : "❌")}");

        // Test 2: Scripts del sistema
        Debug.Log("--- TEST 2: Scripts del Sistema ---");
        var core = GetComponent<bossCore>();
        var life = GetComponent<BossLife>();
        var pattern = GetComponent<BossAttackPattern>();

        Debug.Log($"BossCore: {(core != null ? "✅" : "❌ FALTA")}");
        Debug.Log($"BossLife: {(life != null ? "✅" : "❌ FALTA")}");
        Debug.Log($"BossAttackPattern: {(pattern != null ? "✅" : "❌ FALTA")}");

        // Test 3: Referencias de BossCore
        if (core != null)
        {
            Debug.Log("--- TEST 3: Referencias de BossCore ---");
            Debug.Log($"core.rb: {(core.rb != null ? "✅" : "❌")}");
            Debug.Log($"core.anim: {(core.anim != null ? "✅" : "❌")}");
            Debug.Log($"core.player: {(core.player != null ? "✅" : "❌")}");

            if (core.player != null)
            {
                Debug.Log($"Distancia al jugador: {Vector2.Distance(transform.position, core.player.position)}");
            }
        }

        // Test 4: Jugador
        Debug.Log("--- TEST 4: Buscar Jugador ---");
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Debug.Log($"Jugador encontrado: {(player != null ? "✅ " + player.name : "❌ NO ENCONTRADO")}");

        // Test 5: Ataque de prueba
        if (core != null && pattern != null)
        {
            Debug.Log("--- TEST 5: Intentar Ataque ---");
            Debug.Log($"IsAttacking: {core.IsAttacking}");
            Debug.Log($"IsDead: {core.IsDead}");

        }

        Debug.Log("====== FIN DEL TEST ======");
    }
}