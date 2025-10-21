using UnityEngine;

public class BossDoor : MonoBehaviour
{
    private Animator animator;
    private bool estaAbierta = true;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("La puerta no tiene Animator");
        }
    }

    public void AbrirPuerta()
    {
        if (!estaAbierta)
        {
            animator.SetTrigger("DoorOpen");
            estaAbierta = true;
            Debug.Log("Puerta abiéndose");
        }
    }

    public void CerrarPuerta()
    {
        if (estaAbierta)
        {
            animator.SetTrigger("DoorClosed");
            estaAbierta = false;
            Debug.Log("Puerta cerrándo");
        }
    }
}