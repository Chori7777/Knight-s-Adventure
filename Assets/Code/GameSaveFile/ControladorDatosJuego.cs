using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ControladorDatosJuego : MonoBehaviour
{
    public GameObject player;
    public string archivoDeGuardado;
    public DatosJuego datosjuego = new DatosJuego();

    private static ControladorDatosJuego instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        archivoDeGuardado = Application.persistentDataPath + "/datosjuego.json";
        CargarDatos();
    }

    public static ControladorDatosJuego Instance => instance;

    // ============================================
    // 📥 CARGAR DATOS
    // ============================================
    public void CargarDatos()
    {
        if (File.Exists(archivoDeGuardado))
        {
            string contenido = File.ReadAllText(archivoDeGuardado);
            datosjuego = JsonUtility.FromJson<DatosJuego>(contenido);
            Debug.Log("✅ Datos cargados correctamente.");
            Debug.Log($"Vida: {datosjuego.vidaActual}/{datosjuego.vidaMaxima}");
            Debug.Log($"Pociones: {datosjuego.cantidadpociones}");

            // Aplicar datos al jugador después de un frame (para que exista en escena)
            Invoke("AplicarDatosAlJugador", 0.1f);
        }
        else
        {
            Debug.Log(" No se encontró archivo de guardado");
            datosjuego = new DatosJuego();
        }
    }

    // ============================================
    // 💾 GUARDAR DATOS
    // ============================================
    public void GuardarDatos()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        if (player != null)
        {
            // Capturar todos los valores del jugador
            CapturarDatosDelJugador();

            // Guardar escena actual
            datosjuego.escenaActual = SceneManager.GetActiveScene().name;

            // Guardar posición de la cámara si existe
            Camera cam = Camera.main;
            if (cam != null)
            {
                datosjuego.posicionCamara = cam.transform.position;
            }

            // Convertir a JSON y guardar
            string cadenaJSON = JsonUtility.ToJson(datosjuego, true);
            File.WriteAllText(archivoDeGuardado, cadenaJSON);

            Debug.Log("💾 Guardado");
            Debug.Log($"Jugador: {datosjuego.posicion}, Cámara: {datosjuego.posicionCamara}");
            Debug.Log($"Vida: {datosjuego.vidaActual}/{datosjuego.vidaMaxima}");
            Debug.Log($"Pociones: {datosjuego.cantidadpociones}");
            Debug.Log($"Escena: {datosjuego.escenaActual}");
        }
        else
        {
            Debug.LogWarning("⚠ No se encontró el jugador para guardar");
        }
    }

    // ============================================
    // 🎯 CAPTURAR DATOS DEL JUGADOR
    // ============================================
    private void CapturarDatosDelJugador()
    {
        // ===== POSICIÓN =====
        datosjuego.posicion = player.transform.position;

        // ===== VIDA (playerLife) =====
        playerLife vidaScript = player.GetComponent<playerLife>();
        if (vidaScript != null)
        {
            datosjuego.vidaActual = vidaScript.health;
            datosjuego.vidaMaxima = vidaScript.maxHealth;
            datosjuego.cantidadpociones = vidaScript.botellaVida;

            if (!PlayerPrefs.HasKey("maxbotellaVida"))
                PlayerPrefs.SetInt("maxbotellaVida", vidaScript.maxbotellaVida);
        }

        // ===== MOVIMIENTO (PlayerMovement) =====
        PlayerMovement movScript = player.GetComponent<PlayerMovement>();
        if (movScript != null)
        {
            PlayerPrefs.SetInt("puedeDobleSalto", movScript.puedeDobleSalto ? 1 : 0);
            PlayerPrefs.SetInt("puedeDash", movScript.puedeDash ? 1 : 0);
            PlayerPrefs.SetInt("puedeWallCling", movScript.puedeWallCling ? 1 : 0);
            PlayerPrefs.SetInt("puedeLanzar", movScript.puedeLanzar ? 1 : 0);
        }

        Debug.Log("📌 Datos capturados del jugador");
    }

    // ============================================
    // 📤 APLICAR DATOS AL JUGADOR
    // ============================================
    private void AplicarDatosAlJugador()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogWarning("⚠️ Jugador no encontrado, reintentando...");
            Invoke("AplicarDatosAlJugador", 0.1f);
            return;
        }


        // ===== POSICIÓN =====
        player.transform.position = datosjuego.posicion;

        // ===== VIDA =====
        playerLife vidaScript = player.GetComponent<playerLife>();
        if (vidaScript != null)
        {
            vidaScript.health = datosjuego.vidaActual;
            vidaScript.maxHealth = datosjuego.vidaMaxima;
            vidaScript.botellaVida = datosjuego.cantidadpociones;

            if (PlayerPrefs.HasKey("maxbotellaVida"))
                vidaScript.maxbotellaVida = PlayerPrefs.GetInt("maxbotellaVida");

            vidaScript.ActualizarTexto();
        }

        // ===== MOVIMIENTO =====
        PlayerMovement movScript = player.GetComponent<PlayerMovement>();
        if (movScript != null)
        {
            if (PlayerPrefs.HasKey("puedeDobleSalto"))
                movScript.puedeDobleSalto = PlayerPrefs.GetInt("puedeDobleSalto") == 1;
            if (PlayerPrefs.HasKey("puedeDash"))
                movScript.puedeDash = PlayerPrefs.GetInt("puedeDash") == 1;
            if (PlayerPrefs.HasKey("puedeWallCling"))
                movScript.puedeWallCling = PlayerPrefs.GetInt("puedeWallCling") == 1;
            if (PlayerPrefs.HasKey("puedeLanzar"))
                movScript.puedeLanzar = PlayerPrefs.GetInt("puedeLanzar") == 1;
        }

        // ===== POSICIÓN DE LA CÁMARA =====
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = new Vector3(
                datosjuego.posicionCamara.x,   // X guardado
                datosjuego.posicionCamara.y,   // Y guardado
                cam.transform.position.z       // Z fijo para no perder los tiles
            );
        }
    }

    // ============================================
    // 📍 GUARDAR CHECKPOINT
    // ============================================
    public void GuardarCheckpoint(Vector3 posicionJugador, Vector3 posicionCamara)
    {
        datosjuego.posicion = posicionJugador;
        datosjuego.posicionCamara = posicionCamara;
        datosjuego.escenaActual = SceneManager.GetActiveScene().name;

        GuardarDatos();
        Debug.Log($"Checkpoint guardado: jugador={posicionJugador}, cámara={posicionCamara}");
    }

    // ============================================
    // 🔄 RESET DATOS
    // ============================================
    public void ResetearDatos()
    {
        datosjuego = new DatosJuego();

        PlayerPrefs.DeleteKey("puedeDobleSalto");
        PlayerPrefs.DeleteKey("puedeDash");
        PlayerPrefs.DeleteKey("puedeWallCling");
        PlayerPrefs.DeleteKey("puedeLanzar");
        PlayerPrefs.DeleteKey("maxbotellaVida");

        Debug.Log("Datos reseteados");
    }
}
