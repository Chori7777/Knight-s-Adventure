using UnityEngine;

[System.Serializable]
public class DatosJuego
{
    public int vidaActual = 5;
    public int vidaMaxima = 5;
    public int nivelActualEspada = 1;
    public int cantidadsaltos = 2;
    public int cantidadMonedas = 0;
    public int cantidadpociones = 0;
    public int level = 1;
    public Vector3 posicion = Vector3.zero;        // Posición del jugador
    public Vector3 posicionCamara = Vector3.zero;  // NUEVO: posición de la cámara
    public string escenaActual = "";
}
