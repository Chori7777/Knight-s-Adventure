using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

// ============================================
// CONTROLADOR DE GUARDADO DEL JUEGO
// ============================================

public class ControladorDatosJuego : MonoBehaviour
{
    // ============================================
    // SINGLETON
    // ============================================
    private static ControladorDatosJuego instance;
    public static ControladorDatosJuego Instance => instance;

    // ============================================
    // REFERENCIAS
    // ============================================
    [Header("Referencias")]
    public GameObject player;

    [Header("Datos")]
    public DatosJuego datosjuego = new DatosJuego();

    private string saveFilePath;

    // ============================================
    // CONSTANTES
    // ============================================
    private const string SAVE_FILE_NAME = "/datosjuego.json";
    private const float RETRY_DELAY = 0.1f;

    // PlayerPrefs Keys
    private const string KEY_DOUBLE_JUMP = "canDoubleJump";
    private const string KEY_DASH = "canDash";
    private const string KEY_WALL_CLING = "canWallCling";
    private const string KEY_THROW = "canThrowProjectile";
    private const string KEY_MAX_POTIONS = "maxPotions";

    // ============================================
    // INICIALIZACION
    // ============================================

    private void Awake()
    {
        InitializeSingleton();
        InitializeSaveSystem();
    }

    private void InitializeSingleton()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSaveSystem()
    {
        saveFilePath = Application.persistentDataPath + SAVE_FILE_NAME;
        Debug.Log($"📁 Ruta de guardado: {saveFilePath}");
        CargarDatos();
    }

    // ============================================
    // CARGAR DATOS
    // ============================================

    public void CargarDatos()
    {
        if (!SaveFileExists())
        {
            CreateNewSaveData();
            return;
        }

        LoadSaveData();
        ScheduleApplyData();
    }

    private bool SaveFileExists()
    {
        return File.Exists(saveFilePath);
    }

    private void CreateNewSaveData()
    {
        Debug.Log("⚠️ No se encontró archivo de guardado - Creando datos nuevos");
        datosjuego = new DatosJuego();
    }

    private void LoadSaveData()
    {
        try
        {
            string contenido = File.ReadAllText(saveFilePath);
            datosjuego = JsonUtility.FromJson<DatosJuego>(contenido);

            LogLoadedData();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error al cargar datos: {e.Message}");
            CreateNewSaveData();
        }
    }

    private void LogLoadedData()
    {
        Debug.Log("✅ Datos cargados correctamente");
        Debug.Log($"📍 Posición: {datosjuego.posicion}");
        Debug.Log($"💚 Vida: {datosjuego.vidaActual}/{datosjuego.vidaMaxima}");
        Debug.Log($"🧪 Pociones: {datosjuego.cantidadpociones}/{datosjuego.maxPotions}");
        Debug.Log($"🗺️ Escena: {datosjuego.escenaActual}");
    }

    private void ScheduleApplyData()
    {
        Invoke(nameof(AplicarDatosAlJugador), RETRY_DELAY);
    }

    // ============================================
    // GUARDAR DATOS
    // ============================================

    public void GuardarDatos()
    {
        if (!FindPlayer())
        {
            Debug.LogWarning("⚠️ No se encontró el jugador para guardar");
            return;
        }

        CaptureAllData();
        WriteSaveFile();
        LogSavedData();
    }

    private bool FindPlayer()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        return player != null;
    }

    private void CaptureAllData()
    {
        CapturePlayerData();
        CaptureSceneData();
        CaptureCameraData();
    }

    private void CapturePlayerData()
    {
        CapturePosition();
        CaptureHealthData();
        CaptureAbilitiesData();
    }

    private void CapturePosition()
    {
        datosjuego.posicion = player.transform.position;
    }

    private void CaptureHealthData()
    {
        playerLife vidaScript = player.GetComponent<playerLife>();
        if (vidaScript == null) return;

        // Usar las nuevas propiedades públicas
        datosjuego.vidaActual = vidaScript.Health;
        datosjuego.vidaMaxima = vidaScript.MaxHealth;
        datosjuego.cantidadpociones = vidaScript.Potions;
        datosjuego.maxPotions = vidaScript.MaxPotions; // ✅ NUEVO

        // Backup en PlayerPrefs
        PlayerPrefs.SetInt(KEY_MAX_POTIONS, vidaScript.MaxPotions);
    }

    private void CaptureAbilitiesData()
    {
        PlayerMovement movScript = player.GetComponent<PlayerMovement>();
        if (movScript == null) return;

        PlayerPrefs.SetInt(KEY_DOUBLE_JUMP, movScript.canDoubleJump ? 1 : 0);
        PlayerPrefs.SetInt(KEY_DASH, movScript.canDash ? 1 : 0);
        PlayerPrefs.SetInt(KEY_WALL_CLING, movScript.canWallCling ? 1 : 0);
        PlayerPrefs.SetInt(KEY_THROW, movScript.canThrowProjectile ? 1 : 0);
    }

    private void CaptureSceneData()
    {
        datosjuego.escenaActual = SceneManager.GetActiveScene().name;
    }

    private void CaptureCameraData()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            datosjuego.posicionCamara = cam.transform.position;
        }
    }

    private void WriteSaveFile()
    {
        try
        {
            string cadenaJSON = JsonUtility.ToJson(datosjuego, true);
            File.WriteAllText(saveFilePath, cadenaJSON);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error al guardar datos: {e.Message}");
        }
    }

    private void LogSavedData()
    {
        Debug.Log("💾 ========== DATOS GUARDADOS ==========");
        Debug.Log($"📍 Posición: {datosjuego.posicion}");
        Debug.Log($"📷 Cámara: {datosjuego.posicionCamara}");
        Debug.Log($"💚 Vida: {datosjuego.vidaActual}/{datosjuego.vidaMaxima}");
        Debug.Log($"🧪 Pociones: {datosjuego.cantidadpociones}/{datosjuego.maxPotions}");
        Debug.Log($"🗺️ Escena: {datosjuego.escenaActual}");
        Debug.Log("========================================");
    }

    // ============================================
    // APLICAR DATOS AL JUGADOR
    // ============================================

    private void AplicarDatosAlJugador()
    {
        if (!TryFindPlayer())
        {
            RetryApplyData();
            return;
        }

        ApplyAllData();
        Debug.Log("✅ Datos aplicados al jugador");
    }

    private bool TryFindPlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        return player != null;
    }

    private void RetryApplyData()
    {
        Debug.LogWarning("⚠️ Jugador no encontrado, reintentando...");
        Invoke(nameof(AplicarDatosAlJugador), RETRY_DELAY);
    }

    private void ApplyAllData()
    {
        ApplyPosition();
        ApplyHealthData();
        ApplyAbilitiesData();
        ApplyCameraData();
    }

    private void ApplyPosition()
    {
        player.transform.position = datosjuego.posicion;
    }

    private void ApplyHealthData()
    {
        playerLife vidaScript = player.GetComponent<playerLife>();
        if (vidaScript == null) return;

        // Usar los nuevos métodos públicos
        vidaScript.SetHealth(datosjuego.vidaActual);
        vidaScript.SetMaxHealth(datosjuego.vidaMaxima);
        vidaScript.SetPotions(datosjuego.cantidadpociones);

        // Restaurar pociones máximas desde datos o PlayerPrefs
        if (datosjuego.maxPotions > 0)
        {
            vidaScript.SetMaxPotions(datosjuego.maxPotions); // ✅ NUEVO
        }
        else if (PlayerPrefs.HasKey(KEY_MAX_POTIONS))
        {
            vidaScript.SetMaxPotions(PlayerPrefs.GetInt(KEY_MAX_POTIONS));
        }
    }

    private void ApplyAbilitiesData()
    {
        PlayerMovement movScript = player.GetComponent<PlayerMovement>();
        if (movScript == null) return;

        if (PlayerPrefs.HasKey(KEY_DOUBLE_JUMP))
            movScript.canDoubleJump = PlayerPrefs.GetInt(KEY_DOUBLE_JUMP) == 1;

        if (PlayerPrefs.HasKey(KEY_DASH))
            movScript.canDash = PlayerPrefs.GetInt(KEY_DASH) == 1;

        if (PlayerPrefs.HasKey(KEY_WALL_CLING))
            movScript.canWallCling = PlayerPrefs.GetInt(KEY_WALL_CLING) == 1;

        if (PlayerPrefs.HasKey(KEY_THROW))
            movScript.canThrowProjectile = PlayerPrefs.GetInt(KEY_THROW) == 1;
    }

    private void ApplyCameraData()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 newPosition = new Vector3(
            datosjuego.posicionCamara.x,
            datosjuego.posicionCamara.y,
            cam.transform.position.z  // Mantener Z original
        );

        cam.transform.position = newPosition;
    }

    // ============================================
    // CHECKPOINT
    // ============================================

    public void GuardarCheckpoint(Vector3 playerPosition)
    {
        if (!FindPlayer())
        {
            Debug.LogWarning("⚠️ No se encontró el jugador para checkpoint");
            return;
        }

        datosjuego.posicion = playerPosition;
        datosjuego.escenaActual = SceneManager.GetActiveScene().name;

        // Capturar posición de cámara
        Camera cam = Camera.main;
        if (cam != null)
        {
            datosjuego.posicionCamara = cam.transform.position;
        }

        GuardarDatos();
        Debug.Log($"🚩 Checkpoint guardado en: {playerPosition}");
    }

    // ============================================
    // RESETEAR
    // ============================================

    public void ResetearDatos()
    {
        datosjuego = new DatosJuego();
        ClearAllPlayerPrefs();

        Debug.Log("🔄 Datos reseteados a valores por defecto");
    }

    private void ClearAllPlayerPrefs()
    {
        PlayerPrefs.DeleteKey(KEY_DOUBLE_JUMP);
        PlayerPrefs.DeleteKey(KEY_DASH);
        PlayerPrefs.DeleteKey(KEY_WALL_CLING);
        PlayerPrefs.DeleteKey(KEY_THROW);
        PlayerPrefs.DeleteKey(KEY_MAX_POTIONS);
        PlayerPrefs.Save();
    }

    // ============================================
    // UTILIDADES
    // ============================================

    public bool ExistePartidaGuardada()
    {
        return SaveFileExists();
    }

    public void BorrarPartidaGuardada()
    {
        if (SaveFileExists())
        {
            File.Delete(saveFilePath);
            ResetearDatos();
            Debug.Log("🗑️ Partida guardada borrada");
        }
    }
}