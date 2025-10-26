using UnityEngine;
using DG.Tweening;

/// <summary>
/// Sistema de ascensor con DOTween
/// Se activa cuando el jugador sube o presiona un botón
/// </summary>
public class ElevatorSystem : MonoBehaviour
{
    [Header("Puntos de Movimiento")]
    [SerializeField] private Transform pointA; // Punto inferior
    [SerializeField] private Transform pointB; // Punto superior
    [SerializeField] private bool startAtPointA = true; // Dónde empieza

    [Header("Configuración")]
    [SerializeField] private float moveSpeed = 2f; // Velocidad en unidades/segundo
    [SerializeField] private Ease easeType = Ease.InOutSine; // Tipo de interpolación
    [SerializeField] private float waitTimeAtFloor = 1f; // Tiempo de espera en cada piso

    [Header("Activación")]
    [SerializeField] private bool moveOnPlayerEnter = true; // Moverse cuando jugador sube
    [SerializeField] private bool canCallWithButton = true; // Puede llamarse con botón
    [SerializeField] private LayerMask playerLayer; // Layer del jugador

    [Header("Audio (Opcional)")]
    [SerializeField] private AudioClip moveSound;
    [SerializeField] private AudioClip arriveSound;
    private AudioSource audioSource;

    // Estado
    private bool isAtPointA = true;
    private bool isMoving = false;
    private bool playerOnElevator = false;
    private Tween currentTween;

    // Referencias
    private Transform playerTransform;
    private Vector3 lastElevatorPosition;

    private void Start()
    {
        // Posicionar ascensor en punto inicial
        if (startAtPointA && pointA != null)
        {
            transform.position = pointA.position;
            isAtPointA = true;
        }
        else if (!startAtPointA && pointB != null)
        {
            transform.position = pointB.position;
            isAtPointA = false;
        }

        // Audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (moveSound != null || arriveSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        lastElevatorPosition = transform.position;

        // Crear puntos si no existen
        CreatePointsIfNeeded();
    }

    private void CreatePointsIfNeeded()
    {
        if (pointA == null)
        {
            GameObject pointAObj = new GameObject("PointA_Bottom");
            pointAObj.transform.position = transform.position;
            pointAObj.transform.SetParent(transform.parent);
            pointA = pointAObj.transform;
        }

        if (pointB == null)
        {
            GameObject pointBObj = new GameObject("PointB_Top");
            pointBObj.transform.position = transform.position + Vector3.up * 5f;
            pointBObj.transform.SetParent(transform.parent);
            pointB = pointBObj.transform;
        }
    }

    private void Update()
    {
        // Mover jugador junto con el ascensor
        if (playerOnElevator && playerTransform != null)
        {
            Vector3 elevatorMovement = transform.position - lastElevatorPosition;
            playerTransform.position += elevatorMovement;
        }

        lastElevatorPosition = transform.position;
    }

    // ============================================
    // DETECCIÓN DEL JUGADOR
    // ============================================
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            // Verificar que el jugador esté encima (contacto desde arriba)
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f) // El jugador está encima
                {
                    playerOnElevator = true;
                    playerTransform = collision.transform;

                    Debug.Log("?? Jugador subió al ascensor");

                    // Mover automáticamente si está configurado
                    if (moveOnPlayerEnter && !isMoving)
                    {
                        MoveToOppositeFloor();
                    }

                    break;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform == playerTransform)
        {
            playerOnElevator = false;
            playerTransform = null;
            Debug.Log("?? Jugador bajó del ascensor");
        }
    }

    // ============================================
    // MOVIMIENTO DEL ASCENSOR
    // ============================================
    public void MoveToOppositeFloor()
    {
        if (isMoving) return;

        if (isAtPointA)
        {
            MoveToPointB();
        }
        else
        {
            MoveToPointA();
        }
    }

    public void MoveToPointA()
    {
        if (isMoving || isAtPointA) return;
        StartMove(pointA.position, true);
    }

    public void MoveToPointB()
    {
        if (isMoving || !isAtPointA) return;
        StartMove(pointB.position, false);
    }

    private void StartMove(Vector3 targetPosition, bool movingToA)
    {
        isMoving = true;

        // Calcular duración basada en distancia y velocidad
        float distance = Vector3.Distance(transform.position, targetPosition);
        float duration = distance / moveSpeed;

        // Reproducir sonido de movimiento
        if (audioSource != null && moveSound != null)
        {
            audioSource.PlayOneShot(moveSound);
        }

        // Animar con DOTween
        currentTween = transform.DOMove(targetPosition, duration)
            .SetEase(easeType)
            .OnComplete(() => OnArriveAtFloor(movingToA));

        Debug.Log($"?? Ascensor moviéndose hacia {(movingToA ? "Piso A (abajo)" : "Piso B (arriba)")}");
    }

    private void OnArriveAtFloor(bool arrivedAtA)
    {
        isMoving = false;
        isAtPointA = arrivedAtA;

        Debug.Log($"? Ascensor llegó a {(arrivedAtA ? "Piso A" : "Piso B")}");

        // Reproducir sonido de llegada
        if (audioSource != null && arriveSound != null)
        {
            audioSource.PlayOneShot(arriveSound);
        }

        // Esperar un momento en el piso
        DOVirtual.DelayedCall(waitTimeAtFloor, () =>
        {
            Debug.Log("?? Ascensor listo para moverse de nuevo");
        });
    }

    // ============================================
    // BOTÓN DE LLAMADA (llamado desde ElevatorButton)
    // ============================================
    public void CallElevator(bool callToPointA)
    {
        if (!canCallWithButton)
        {
            Debug.LogWarning("?? El ascensor no puede ser llamado con botones");
            return;
        }

        if (isMoving)
        {
            Debug.Log("? Ascensor ya está en movimiento");
            return;
        }

        // Si el ascensor ya está en ese piso, no hacer nada
        if (callToPointA && isAtPointA)
        {
            Debug.Log("? El ascensor ya está en el Piso A");
            return;
        }
        if (!callToPointA && !isAtPointA)
        {
            Debug.Log("? El ascensor ya está en el Piso B");
            return;
        }

        // Mover al piso solicitado
        if (callToPointA)
        {
            MoveToPointA();
        }
        else
        {
            MoveToPointB();
        }
    }

    // ============================================
    // CONTROL MANUAL (para testing)
    // ============================================
    private void OnValidate()
    {
        // Actualizar posición en el editor cuando cambien los puntos
        if (!Application.isPlaying && pointA != null && pointB != null)
        {
            if (startAtPointA)
            {
                transform.position = pointA.position;
            }
            else
            {
                transform.position = pointB.position;
            }
        }
    }

    private void OnDestroy()
    {
        // Cancelar tween al destruir
        if (currentTween != null && currentTween.IsActive())
        {
            currentTween.Kill();
        }
    }

    // ============================================
    // GIZMOS
    // ============================================
    private void OnDrawGizmos()
    {
        if (pointA == null || pointB == null) return;

        // Línea del recorrido
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(pointA.position, pointB.position);

        // Punto A (inferior)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(pointA.position, 0.5f);
        Gizmos.DrawWireCube(pointA.position, Vector3.one * 0.3f);

        // Punto B (superior)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(pointB.position, 0.5f);
        Gizmos.DrawWireCube(pointB.position, Vector3.one * 0.3f);

        // Posición actual del ascensor
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }

    // ============================================
    // PROPIEDADES PÚBLICAS
    // ============================================
    public bool IsMoving => isMoving;
    public bool IsAtPointA => isAtPointA;
    public bool PlayerOnElevator => playerOnElevator;
}