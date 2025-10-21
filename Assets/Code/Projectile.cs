using UnityEngine;

// SCRIPT PARA EL PROYECTIL (Adjunta esto al prefab de proyectil)
public class Projectile : MonoBehaviour
{
    [SerializeField] private float velocidad = 5f;
    [SerializeField] private float tiempoDeVida = 10f;

    private Vector3 direccion;
    private Transform objetivo;
    private int tipo; // 0: recto, 1: sigue jugador, 2: patr�n, 3: rebota
    private float tiempoTranscurrido = 0f;
    private int rebotes = 0;
    private int maxRebotes = 3;

    void Start()
    {
        Destroy(gameObject, tiempoDeVida);
    }

    void Update()
    {
        tiempoTranscurrido += Time.deltaTime;

        switch (tipo)
        {
            case 0:
                Recto();
                break;
            case 1:
                SigueJugador();
                break;
            case 2:
                Patron();
                break;
            case 3:
                Rebota();
                break;
        }
    }

    // TIPO 0: Movimiento recto
    private void Recto()
    {
        transform.Translate(direccion * velocidad * Time.deltaTime);
    }

    // TIPO 1: Sigue al jugador
    private void SigueJugador()
    {
        if (objetivo == null) return;

        Vector3 direccionAlJugador = (objetivo.position - transform.position).normalized;
        transform.Translate(direccionAlJugador * velocidad * Time.deltaTime);

        // Rota hacia el jugador
        float angulo = Mathf.Atan2(direccionAlJugador.y, direccionAlJugador.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angulo, Vector3.forward);
    }

    // TIPO 2: Patr�n (c�rculo, espiral, onda)
    private void Patron()
    {
        // Patr�n circular
        float x = Mathf.Cos(tiempoTranscurrido * 3f) * 3f;
        float y = Mathf.Sin(tiempoTranscurrido * 3f) * 3f;

        Vector3 offset = new Vector3(x, y, 0f);
        transform.position += direccion * velocidad * Time.deltaTime;
        transform.position += offset * 0.1f;
    }

    // TIPO 3: Rebota en los bordes
    private void Rebota()
    {
        transform.Translate(direccion * velocidad * Time.deltaTime);

        // L�mites de rebote (ajusta seg�n tu escena)
        float minX = -10f;
        float maxX = 10f;
        float minY = -5f;
        float maxY = 15f;

        if (transform.position.x < minX || transform.position.x > maxX)
        {
            direccion.x *= -1;
            rebotes++;
        }

        if (transform.position.y < minY || transform.position.y > maxY)
        {
            direccion.y *= -1;
            rebotes++;
        }

        // Se destruye despu�s de X rebotes
        if (rebotes > maxRebotes)
        {
            Destroy(gameObject);
        }
    }

    public void Inicializar(Vector3 dir, int tipoAtaque, Transform objetivoJugador = null)
    {
        direccion = dir.normalized;
        tipo = tipoAtaque;
        objetivo = objetivoJugador;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("�Projectil golpe� al jugador!");
            Destroy(gameObject);
        }
    }
}