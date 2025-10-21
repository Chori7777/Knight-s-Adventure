using System.Collections;
using UnityEngine;

public class BossScriptAttacks : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform jugador;
    [SerializeField] private GameObject proyectilPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject alertaGO; // <- Objeto vacío con animación de alerta

    [Header("Stats")]
    [SerializeField] private float velocidadMovimiento = 3f;
    [SerializeField] private float distanciaGolpe = 2f;
    [SerializeField] private float tiempoEntreAtaques = 4f;
    [SerializeField] private int piedrasPorLluvia = 5;

    private Rigidbody2D rb;
    private Animator anim;
    private bool atacando = false;
    private bool moviendoAleatorio = false;
    [SerializeField] private Animator alertaAnimator;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        alertaGO.SetActive(false);

        // 🎮 Buscar al jugador automáticamente por tag
        if (jugador == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                jugador = playerObj.transform;
                Debug.Log("Jugador encontrado automáticamente: " + jugador.gameObject.name);
            }
            else
            {
                Debug.LogError("No se encontró al jugador. Asegúrate de que tenga el tag 'Player'");
                return;
            }
        }

        StartCoroutine(PatronIA());
    }

    private void Update()
    {
        if (jugador == null) return; // Seguridad

        // Si no está atacando, se mueve de forma aleatoria estratégica
        if (!atacando)
        {
            if (!moviendoAleatorio)
                StartCoroutine(MovimientoEstrategico());
        }

        // Golpe cuerpo a cuerpo si el jugador está cerca
        float distancia = Vector2.Distance(transform.position, jugador.position);
        if (distancia <= distanciaGolpe && !atacando)
        {
            StartCoroutine(AtaqueGolpe());
        }

        // Orientar sprite hacia el jugador
        if (jugador.position.x < transform.position.x)
            transform.localScale = new Vector3(-1, 1, 1);
        else
            transform.localScale = new Vector3(1, 1, 1);
    }

    // 🧭 Movimiento "aleatorio" entre acercarse o alejarse
    private IEnumerator MovimientoEstrategico()
    {
        moviendoAleatorio = true;
        float duracion = Random.Range(1f, 2f);
        float tiempo = 0f;

        Vector2 direccion = (jugador.position - transform.position).normalized;
        bool acercarse = Random.value > 0.5f;

        while (tiempo < duracion && !atacando)
        {
            tiempo += Time.deltaTime;
            float dirX = acercarse ? direccion.x : -direccion.x;
            rb.linearVelocity = new Vector2(dirX * velocidadMovimiento, rb.linearVelocity.y);
            yield return null;
        }

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        moviendoAleatorio = false;
    }

    // 🧠 Control de IA por patrones
    private IEnumerator PatronIA()
    {
        while (true)
        {
            yield return new WaitForSeconds(tiempoEntreAtaques);

            if (atacando) continue;

            int ataque = Random.Range(0, 2); // 0 = lluvia / 1 = correr (melee es condicional)
            switch (ataque)
            {
                case 0:
                    StartCoroutine(AtaqueLluvia());
                    break;
                case 1:
                    StartCoroutine(AtaqueCorrer());
                    break;
            }
        }
    }

    // ⚠️ Mostrar alerta antes de cualquier ataque
    private IEnumerator MostrarAlerta(float duracion = 0.8f)
    {
        alertaGO.SetActive(true);
        alertaAnimator.SetTrigger("Alert");
        yield return new WaitForSeconds(duracion);
        alertaGO.SetActive(false);
    }

    // 🪨 Ataque 1: Salta y hace lluvia de piedras
    private IEnumerator AtaqueLluvia()
    {
        atacando = true;
        rb.linearVelocity = Vector2.zero;

        yield return StartCoroutine(MostrarAlerta());

        // Salto + lluvia
        rb.AddForce(Vector2.up * 5f, ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < piedrasPorLluvia; i++)
        {
            float x = Random.Range(spawnPoint.position.x - 8f, spawnPoint.position.x + 8f);
            float y = spawnPoint.position.y + 10f;
            Instantiate(proyectilPrefab, new Vector3(x, y, 0f), Quaternion.identity);
            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(0.5f);
        atacando = false;
    }

    // 🏃‍♂️ Ataque 2: Corre hacia el jugador
    private IEnumerator AtaqueCorrer()
    {
        atacando = true;
        rb.linearVelocity = Vector2.zero;

        yield return StartCoroutine(MostrarAlerta());
        anim.SetTrigger("Run");
        Vector2 direccion = (jugador.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direccion.x * velocidadMovimiento * 2f, rb.linearVelocity.y);

        float tiempo = 0f;
        while (tiempo < 2f)
        {
            tiempo += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;

        // Al "chocar", genera piedras
        for (int i = 0; i < piedrasPorLluvia; i++)
        {
            float x = Random.Range(spawnPoint.position.x - 8f, spawnPoint.position.x + 8f);
            float y = spawnPoint.position.y + 10f;
            Instantiate(proyectilPrefab, new Vector3(x, y, 0f), Quaternion.identity);
            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(0.5f);
        atacando = false;
    }

    // 👊 Ataque 3: Golpe cuerpo a cuerpo si el jugador está cerca
    private IEnumerator AtaqueGolpe()
    {
        atacando = true;
        rb.linearVelocity = Vector2.zero;

        yield return StartCoroutine(MostrarAlerta(0.5f));

        anim.SetTrigger("Attack");
        Debug.Log("MeleeAttack");
        yield return new WaitForSeconds(1.5f); // duración del ataque

        atacando = false;
    }
}