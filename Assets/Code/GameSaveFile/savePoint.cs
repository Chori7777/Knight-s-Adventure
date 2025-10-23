using UnityEngine;

public class savePoint : MonoBehaviour
{
    [SerializeField] private bool autoGuardar = true; // Si se guarda automáticamente al tocar

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && autoGuardar)
        {
            Vector3 posicionJugador = transform.position;
            Vector3 posicionCamara = Camera.main.transform.position;

            ControladorDatosJuego.Instance.GuardarCheckpoint(posicionJugador);
            Debug.Log(" Guardado en checkpoint");
        }
    }

    // Llamar manualmente
    public void GuardarManualmente()
    {
        Vector3 posicionJugador = transform.position;
        Vector3 posicionCamara = Camera.main.transform.position;

        ControladorDatosJuego.Instance.GuardarCheckpoint(posicionJugador);
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}