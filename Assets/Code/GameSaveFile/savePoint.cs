using UnityEngine;

public class savePoint : MonoBehaviour
{
    [SerializeField] private bool autoGuardar = true;
    [SerializeField] private bool curarAlGuardar = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && autoGuardar)
        {
            Vector3 posicionJugador = transform.position;
            Vector3 posicionCamara = Camera.main.transform.position;

            ControladorDatosJuego.Instance.GuardarCheckpoint(posicionJugador);
            Debug.Log(" Guardado en checkpoint");

            if (curarAlGuardar)
            {
                playerLife vida = collision.GetComponent<playerLife>();
                if (vida != null)
                {
                    vida.SetHealth(vida.MaxHealth); // 🩹 Cura al máximo
                    Debug.Log(" Vida restaurada al máximo");
                }
            }
        }
    }

    // Llamar manualmente (por ejemplo, desde un botón)
    public void GuardarManualmente(GameObject jugador)
    {
        Vector3 posicionJugador = transform.position;
        Vector3 posicionCamara = Camera.main.transform.position;

        ControladorDatosJuego.Instance.GuardarCheckpoint(posicionJugador);
        Debug.Log(" Guardado manual");

        if (curarAlGuardar)
        {
            playerLife vida = jugador.GetComponent<playerLife>();
            if (vida != null)
            {
                vida.SetHealth(vida.MaxHealth);
                Debug.Log(" Vida restaurada al máximo");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}
