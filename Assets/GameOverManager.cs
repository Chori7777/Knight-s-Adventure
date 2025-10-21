using TMPro;
using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textoMuerte; // Asignalo desde el Inspector
    public void Reintentar()
    {
        // Restaurar datos del último checkpoint
        if (ControladorDatosJuego.Instance != null)
        {
            // Cargar el archivo de guardado
            ControladorDatosJuego.Instance.CargarDatos();

            // Cambiar la escena a la guardada
            string escena = ControladorDatosJuego.Instance.datosjuego.escenaActual;
            if (!string.IsNullOrEmpty(escena))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(escena);
            }
        }
    }
    private string[] frasesMuerte = new string[]
   {
        "Decepcionante",
        "Es esto lo que eres ahora?",
        "...",
        "Hace frio...",
        "Fue intencional?",
        "No importa cuantas veces mueras, siempre volveras",

   };

    void Start()
    {
        MostrarTextoMuerte();
    }

    public void MostrarTextoMuerte()
    {
        int randomIndex = Random.Range(0, frasesMuerte.Length);
        textoMuerte.text = frasesMuerte[randomIndex];
    }



}
