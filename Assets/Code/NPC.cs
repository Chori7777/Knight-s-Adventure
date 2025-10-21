// NPC.cs - Coloca en el NPC
using UnityEngine;

public class NPC : MonoBehaviour
{
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private string[] dialogueLines;
    private Transform player;
    private int currentDialogueLine = 0;
    private bool dialogueActive = false;
    private bool isInRange = false;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogError("No se encontró Player");
            return;
        }
        player = playerObj.transform;

    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        isInRange = distance <= interactionDistance;

        // Presionar E para iniciar el diálogo
        if (isInRange && Input.GetKeyDown(KeyCode.E) && !dialogueActive)
        {
            StartDialogue();

        }

        // Presionar E para avanzar en el diálogo
        if (dialogueActive && Input.GetKeyDown(KeyCode.E))
        {
            NextDialogue();
        }
        seePlayer();
        if(!isInRange && dialogueActive)
        {
            EndDialogue();
        }
    }
    private void seePlayer()
    {
    if(player.transform.position.x < transform.position.x)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }

    } 
    private void StartDialogue()
    {
        dialogueActive = true;
        currentDialogueLine = 0;
        ShowCurrentLine();
      
    }

    private void NextDialogue()
    {
        currentDialogueLine++;

        // Si hay más líneas, las mostramos
        if (currentDialogueLine < dialogueLines.Length)
        {
            ShowCurrentLine();
        }
        else
        {
            // Si no hay más líneas, cerramos el diálogo
            EndDialogue();
        }
    }

    private void ShowCurrentLine()
    {
        DialogueManager.Instance.ShowDialogue(dialogueLines[currentDialogueLine]);
    }

    private void EndDialogue()
    {
        dialogueActive = false;
        currentDialogueLine = 0;
        DialogueManager.Instance.CloseDialogue();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}