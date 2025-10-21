using UnityEngine;

public class CameraBackTrigger : MonoBehaviour
{
    [SerializeField] private float zoomAnterior = 5f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CameraManager.instance.RetrocederCamara();
            CameraManager.instance.SetCameraSize(zoomAnterior);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box) Gizmos.DrawWireCube(transform.position, box.size);
    }
}
