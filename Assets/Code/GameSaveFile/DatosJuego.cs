using System.Collections.Generic;
using UnityEngine;

// ============================================
// CLASE DE DATOS SERIALIZABLES
// ============================================

[System.Serializable]
public class DatosJuego
{
    // ============================================
    // SALUD
    // ============================================
    public int vidaActual = 5;
    public int vidaMaxima = 5;

    // ============================================
    // POCIONES
    // ============================================
    public int cantidadpociones = 3;
    public int maxPotions = 5;  // ✅ NUEVO - Pociones máximas

    // ============================================
    // HABILIDADES
    // ============================================
    public int nivelActualEspada = 1;
    public int cantidadsaltos = 2;

    // ============================================
    // ITEMS
    // ============================================
    public int cantidadMonedas = 0;

    // ============================================
    // PROGRESO
    // ============================================
    public int level = 1;

    // ============================================
    // POSICIONES
    // ============================================
    public Vector3 posicion = Vector3.zero;
    public Vector3 posicionCamara = Vector3.zero;

    // ============================================
    // ESCENA
    // ============================================
    public string escenaActual = "";

    public List<string> jefesDerrotados = new List<string>();

    // ============================================
    // CONSTRUCTOR (opcional - valores por defecto)
    // ============================================
    public DatosJuego()
    {
        // Valores por defecto ya están arriba
    }

    // ============================================
    // METODO DEBUG
    // ============================================
    public override string ToString()
    {
        return $"DatosJuego: Vida={vidaActual}/{vidaMaxima}, " +
               $"Pociones={cantidadpociones}/{maxPotions}, " +
               $"Escena={escenaActual}, " +
               $"Posicion={posicion}";
    }
}