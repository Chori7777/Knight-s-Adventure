using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class NextLevel : MonoBehaviour
{
    public KeyCode interactionKey = KeyCode.E;
    public Animator doorAnimator;
    public string triggerName = "Open";

    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactionKey))
        {
            Debug.Log("Se detecto colision con puerta");
            StartCoroutine(LevelTransition());
        }
    }

    IEnumerator LevelTransition()
    {
        // Ocultar todos los objetos con tag Player
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            player.SetActive(false);
        }
        Debug.Log("Jugador ocultado");

        // Activar animación con trigger
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger(triggerName);
            Debug.Log("Trigger '" + triggerName + "' activado en la puerta");

            // Esperar a que la animación se reproduzca
            yield return new WaitForSeconds(2f);
        }

        // Cambiar de escena
        Debug.Log("Cargando escena: Victory");
   
    }

    // ✅ CAMBIADO: Ahora usa Collider2D
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Jugador entró en el área de la puerta - Presiona " + interactionKey);
        }
    }
    public void LoadVictoryScene()
    {
        SceneManager.LoadScene("Victory");
    }

    // ✅ CAMBIADO: Ahora usa Collider2D
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("Jugador salió del área de la puerta");
        }
    }
}