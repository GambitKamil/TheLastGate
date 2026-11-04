using UnityEngine;

public class DialogueUI : MonoBehaviour
{
    // A slot in the Inspector to plug in the DialoguePanel
    [SerializeField] private GameObject dialoguePanel;

    private void Start()
    {
        // When the game starts, make sure the panel is hidden
        HideDialogue();
    }

    // Public so NPCController can call it
    public void ShowDialogue()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
    }

    // Public so PlayerController or OnTriggerExit2D can call it
    public void HideDialogue()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }
}