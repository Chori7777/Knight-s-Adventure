using UnityEngine;

public class FondoMovimiento : MonoBehaviour
{
    [SerializeField] private Vector2 velocidadMovimiento;

    private Vector2 offset;
    private Material material;
    private Rigidbody2D jugadorRB;

    private void Awake()
    {
        material = GetComponent<SpriteRenderer>().material;

        // Mejor manera de encontrar el jugador
        GameObject jugador = GameObject.FindGameObjectWithTag("Player");
        if (jugador != null)
        {
            jugadorRB = jugador.GetComponent<Rigidbody2D>();
        }
        else
        {
            Debug.LogError("No se encontró objeto con tag 'Player'");
        }
    }

    private void Update()
    {
        // Verificación de seguridad antes de usar jugadorRB
        if (jugadorRB != null)
        {
            offset = (jugadorRB.linearVelocity.x * 0.1f) * velocidadMovimiento * Time.deltaTime;
            material.mainTextureOffset += offset;
        }
    }
}