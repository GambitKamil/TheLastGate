using UnityEngine;


public class NPCController : MonoBehaviour, IInteractable
{
    [SerializeField] private DialogueUI dialogueUI;
    public void Interact()
    {
        Debug.Log("Guard: Halt! Who goes there? State your business!");
        // Open the dialogue panel
        if (dialogueUI != null)
        {
            dialogueUI.ShowDialogue();
        }
    }
}