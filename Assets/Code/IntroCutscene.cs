// IntroCutscene.cs - Coloca en una escena llamada "Intro"
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class IntroCutscene : MonoBehaviour
{
    [Header("Configuración de la Historia")]
    [SerializeField] private Sprite[] imagenes; // Array de imágenes de la historia
    [SerializeField] private float tiempoPorImagen = 3f; // Segundos que dura cada imagen
    [SerializeField] private string escenaDestino = "MainMenu"; // A dónde ir después

    [Header("Referencias UI")]
    [SerializeField] private Image imagenDisplay; // La imagen que se muestra
    [SerializeField] private GameObject botonSkip; // Botón para saltear

    [Header("Efectos de Transición")]
    [SerializeField] private float tiempoFade = 1f; // Duración del fade in/out
    [SerializeField] private CanvasGroup canvasGroup; // Para controlar el fade

    [SerializeField] private string[] textos; // Texto narrativo por imagen
    [SerializeField] private TextMeshProUGUI textoHistoria; // Referencia al texto en pantalla

    private int indiceActual = 0;
    private bool estaSaltando = false;

    void Start()
    {
        // Asegurar que empieza invisible
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        // Mostrar primera imagen
        if (imagenes.Length > 0)
        {
            StartCoroutine(ReproducirIntro());
        }
        else
        {
            Debug.LogError("❌ No hay imágenes asignadas en el array");
            IrAlMenu();
        }
    }

    void Update()
    {
        // Detectar tecla para skipear (Espacio o Escape)
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
        {
            SkipIntro();
        }
    }

    private IEnumerator ReproducirIntro()
    {
        // Recorrer todas las imágenes
        for (indiceActual = 0; indiceActual < imagenes.Length; indiceActual++)
        {
            if (estaSaltando) break;

            // Fade in
            yield return StartCoroutine(MostrarImagen(imagenes[indiceActual]));

            // Esperar
            yield return new WaitForSeconds(tiempoPorImagen);

            // Fade out (solo si no es la última imagen)
            if (indiceActual < imagenes.Length - 1)
            {
                yield return StartCoroutine(OcultarImagen());
            }
        }

        // Terminar intro
        if (!estaSaltando)
        {
            yield return StartCoroutine(OcultarImagen());
            IrAlMenu();
        }
    }

    private IEnumerator MostrarImagen(Sprite imagen)
    {
        imagenDisplay.sprite = imagen;

        // Mostrar texto si hay alguno asignado
        if (textoHistoria != null && textos.Length > indiceActual)
        {
            textoHistoria.text = textos[indiceActual];
        }

        float tiempo = 0f;
        while (tiempo < tiempoFade)
        {
            tiempo += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, tiempo / tiempoFade);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private IEnumerator OcultarImagen()
    {
        float tiempo = 0f;
        while (tiempo < tiempoFade)
        {
            tiempo += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, tiempo / tiempoFade);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }

    // Método público para el botón Skip
    public void SkipIntro()
    {
        if (!estaSaltando)
        {
            estaSaltando = true;
            StopAllCoroutines();
            StartCoroutine(TransicionAlMenu());
        }
    }

    private IEnumerator TransicionAlMenu()
    {
        // Fade out final
        yield return StartCoroutine(OcultarImagen());
        IrAlMenu();
    }

    private void IrAlMenu()
    {
        SceneManager.LoadScene(escenaDestino);
    }
}
