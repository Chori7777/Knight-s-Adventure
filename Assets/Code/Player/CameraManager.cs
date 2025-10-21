using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance; // Singleton

    private Camera mainCamera;
    public float cameraSpeed = 5f;

    public Transform[] checkpoints; // Array de GameObjects
    [SerializeField] private int startingCheckpoint = 1; // AQUÍ: Checkpoint inicial (0 = Sala 1, 1 = Sala 2, etc)
    private int currentCameraIndex = 0;
    private bool isMoving = false;
    private Vector3 targetPosition;
    private float cameraZ = -10f; // Z fijo para la cámara
    public float cameraSize = 5f; // Tamaño de la cámara (zoom)

    void Awake()
    {
        // Singleton
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        mainCamera.orthographicSize = cameraSize; // Aplicar zoom inicial

        // Si no hay checkpoints asignados, buscarlos automáticamente por tag
        if (checkpoints == null || checkpoints.Length == 0)
        {
            GameObject[] checkpointObjects = GameObject.FindGameObjectsWithTag("CameraCheckpoint");
            checkpoints = new Transform[checkpointObjects.Length];
            for (int i = 0; i < checkpointObjects.Length; i++)
            {
                checkpoints[i] = checkpointObjects[i].transform;
            }
        }

        if (checkpoints.Length > 0)
        {
            // Usar el checkpoint inicial especificado
            currentCameraIndex = startingCheckpoint;

            // Posicionar en el checkpoint inicial manteniendo Z fijo
            Vector3 startPos = checkpoints[currentCameraIndex].position;
            startPos.z = cameraZ;
            transform.position = startPos;
            targetPosition = startPos;
            isMoving = false;

            Debug.Log("Cámara inicializada en checkpoint " + currentCameraIndex + " con " + checkpoints.Length + " checkpoints totales");
        }
    }

    void Update()
    {
        if (isMoving)
        {
            MoveCamera();
        }
    }

    private void MoveCamera()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, cameraSpeed * Time.deltaTime);

        // Detectar si llegó al destino
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            transform.position = targetPosition;
            isMoving = false;
        }
    }

    // Llamar cuando el jugador pase un trigger
    public void AvanzarCamara()
    {
        if (currentCameraIndex < checkpoints.Length - 1)
        {
            currentCameraIndex++;
            Vector3 newPos = checkpoints[currentCameraIndex].position;
            newPos.z = cameraZ; // Mantener Z fijo
            targetPosition = newPos;
            isMoving = true;
            Debug.Log("Cámara avanzó al checkpoint " + currentCameraIndex);
        }
    }

    // Retroceder cámara (opcional)
    public void RetrocederCamara()
    {
        if (currentCameraIndex > 0)
        {
            currentCameraIndex--;
            Vector3 newPos = checkpoints[currentCameraIndex].position;
            newPos.z = cameraZ; // Mantener Z fijo
            targetPosition = newPos;
            isMoving = true;
            Debug.Log("Cámara retrocedió al checkpoint " + currentCameraIndex);
        }
    }

    // Ir a una posición específica
    public void IrAlCheckpoint(int index)
    {
        if (index >= 0 && index < checkpoints.Length)
        {
            currentCameraIndex = index;
            Vector3 newPos = checkpoints[index].position;
            newPos.z = cameraZ; // Mantener Z fijo
            targetPosition = newPos;
            isMoving = true;
            Debug.Log("Cámara se movió al checkpoint " + index);
        }
    }

    // Añadir nuevas posiciones de cámara dinámicamente
    public void AgregarCheckpoint(Vector3 posicion)
    {
        System.Array.Resize(ref checkpoints, checkpoints.Length + 1);
        GameObject newCheckpoint = new GameObject("Checkpoint_" + checkpoints.Length);
        newCheckpoint.transform.position = posicion;
        checkpoints[checkpoints.Length - 1] = newCheckpoint.transform;
    }

    // Resetear la cámara al checkpoint inicial (llamar cuando cambias de escena)
    public void ResetearCamara()
    {
        currentCameraIndex = startingCheckpoint;
        if (checkpoints.Length > 0)
        {
            Vector3 startPos = checkpoints[startingCheckpoint].position;
            startPos.z = cameraZ;
            transform.position = startPos;
            targetPosition = startPos;
            isMoving = false;
        }
    }

    // Cambiar el zoom de la cámara
    public void SetCameraSize(float newSize)
    {
        cameraSize = newSize;
        mainCamera.orthographicSize = cameraSize;
    }

    // Obtener el tamaño actual de la cámara (zoom actual)
    public float GetCameraSize()
    {
        return mainCamera.orthographicSize;
    }

    // Alejar la cámara (zoom out)
    public void AlejarCamara(float cantidad = 2f)
    {
        SetCameraSize(cameraSize + cantidad);
    }

    // Acercar la cámara (zoom in)
    public void AcercarCamara(float cantidad = 2f)
    {
        SetCameraSize(Mathf.Max(1f, cameraSize - cantidad));
    }

    // Obtener el checkpoint actual
    public int GetCurrentCheckpoint()
    {
        return currentCameraIndex;
    }
}