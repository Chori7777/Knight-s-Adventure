using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class ChangeScene : MonoBehaviour
{
    public static int MainMenuVariation = 0;

    private void CargarEscena(string nombreEscena)
    {
        Debug.Log($"Cambiando a escena: {nombreEscena}");
        Time.timeScale = 1f;

        if (FadeController.Instance != null)
        {
            // Usa el fade universal
            FadeController.Instance.CambiarEscenaConFade(nombreEscena);
        }
        else
        {
            // Fallback sin fade
            Debug.LogWarning("cargando escena directamente.");
            SceneManager.LoadScene(nombreEscena);
        }
    }

    //  MÉTODOS PÚBLICOS

    public void LoadScene(string sceneName)
    {
        CargarEscena(sceneName);
    }

    public void pause()
    {
        Time.timeScale = 0f;
    }

    public void resume()
    {
        Time.timeScale = 1f;
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        CargarEscena(SceneManager.GetActiveScene().name);
    }

    public void ExitGame()
    {
        if (ControladorDatosJuego.Instance != null)
        {
            ControladorDatosJuego.Instance.GuardarDatos();
        }

        Debug.Log("Saliendo del juego");
        Application.Quit();
    }

    public void MainMenu()
    {
        MainMenuVariation = 0;
        CargarEscena("MainMenu");
    }


    // 🆕 NUEVA PARTIDA

    public void NewGame()
    {
        Debug.Log("🆕 Iniciando nueva partida...");
        Debug.Log("MainMenuVariation = " + MainMenuVariation);

        // 1️⃣ Borrar archivo de guardado existente
        BorrarPartidaGuardada();

        // 2️⃣ Resetear datos del juego en memoria
        if (ControladorDatosJuego.Instance != null)
        {
            // Reinicia todos los datos
            ControladorDatosJuego.Instance.ResetearDatos();

            // Limpia también la lista de jefes derrotados
            ControladorDatosJuego.Instance.datosjuego.jefesDerrotados.Clear();

            Debug.Log("🔁 Lista de jefes derrotados reiniciada");
        }
        else
        {
            Debug.LogWarning("⚠️ No se encontró el ControladorDatosJuego al reiniciar");
        }

        // 3️⃣ Elegir escena según variación de menú
        if (MainMenuVariation == 0)
        {
            Debug.Log("Cargando CharacterSelector");
            CargarEscena("CharacterSelector");
        }
        else if (MainMenuVariation == 1)
        {
            Debug.Log("Cargando CharacterSelectorAlternative");
            CargarEscena("CharacterSelectorAlternative");
        }
        else
        {
            Debug.LogWarning("MainMenuVariation tiene un valor inesperado: " + MainMenuVariation);
        }
    }



    //  CONTINUAR PARTIDA

    public void ContinueGame()
    {
        if (ExistePartidaGuardada())
        {
            Debug.Log(" Cargando partida guardada...");

            if (ControladorDatosJuego.Instance != null)
            {
                ControladorDatosJuego.Instance.CargarDatos();
                string escenaGuardada = ControladorDatosJuego.Instance.datosjuego.escenaActual;

                if (!string.IsNullOrEmpty(escenaGuardada))
                {
                    CargarEscena(escenaGuardada);
                }
                else
                {
                    Debug.LogWarning(" No hay escena guardada, cargando por defecto");
                    CargarEscena("CharacterSelector");
                }
            }
        }
        else
        {
            Debug.LogWarning(" No hay partida guardada");
        }
    }

    // ==========================
    // 💾 GUARDADO Y UTILIDADES
    // ==========================
    public bool ExistePartidaGuardada()
    {
        string archivo = Application.persistentDataPath + "/datosjuego.json";
        return File.Exists(archivo);
    }

    private void BorrarPartidaGuardada()
    {
        string archivo = Application.persistentDataPath + "/datosjuego.json";

        if (File.Exists(archivo))
        {
            File.Delete(archivo);
            Debug.Log("🗑️ Partida anterior borrada");
        }
    }

    public void SetMenuVariation(int variation)
    {
        MainMenuVariation = variation;
        Debug.Log($"Variación del menú establecida a: {MainMenuVariation}");
    }
}
