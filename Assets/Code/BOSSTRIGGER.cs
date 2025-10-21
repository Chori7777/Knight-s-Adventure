using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    [SerializeField] private GameObject bossPrefab; // Jefe
    [SerializeField] private Vector3 bossSpawnPosition = Vector3.zero; // Pos Jefe
    [SerializeField] private BossDoor[] doorsToClose; // Puertas
    [SerializeField] private float cooldownTiempo = 1f;
    [SerializeField] private GameObject player;

    private bool enCooldown = false;
    private bool enPelea = false;

    private void Start()
    {

    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !enCooldown && !enPelea)
        {
            IniciarBatalla();
            StartCoroutine(ActivarCooldown());
        }
    }

    private void IniciarBatalla()
    {
        Debug.Log("Jefe spawn");
        enPelea = true;

        // Genera
        if (bossPrefab != null)
        {
            Instantiate(bossPrefab, bossSpawnPosition, Quaternion.identity);
        }

        // Cierra
        CerrarPuertas();
    }

    private void CerrarPuertas()
    {

        foreach (BossDoor puerta in doorsToClose)
        {
            if (puerta != null)
            {
                puerta.CerrarPuerta();
                Debug.Log("Puerta cerrada: " + puerta.gameObject.name);
            }
        }
    }

    public void JefeDerotado()
    {
        Debug.Log("Muelto");
        enPelea = false;
        AbrirPuertas();
    }

    private void AbrirPuertas()
    {
        foreach (BossDoor puerta in doorsToClose)
        {
            if (puerta != null)
            {
                puerta.AbrirPuerta();
                Debug.Log("Puerta abierta: " + puerta.gameObject.name);
            }
        }
    }

    private System.Collections.IEnumerator ActivarCooldown()
    {
        enCooldown = true;
        yield return new WaitForSeconds(cooldownTiempo);
        enCooldown = false;
    }

    void OnDrawGizmosSelected()
    {
        // Dibuja la posición donde aparecerá el jefe
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(bossSpawnPosition, 0.5f);

        // Dibuja el trigger
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, boxCollider.size);
        }
    }
}
