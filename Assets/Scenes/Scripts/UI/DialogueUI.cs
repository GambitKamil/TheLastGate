using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject dialoguePanel;

    [Header("Text & Input Fields")]
    [SerializeField] private TextMeshProUGUI dialogueContentText;
    [SerializeField] private TMP_InputField playerInputField;
    [SerializeField] private Button sendButton;

    [Header("Services")]
    [SerializeField] private OllamaService llmService;

    public bool IsDialogueOpen => dialoguePanel != null && dialoguePanel.activeSelf;

    private void Start()
    {
        HideDialogue();

        // 1. Tell Unity: When sendButton is clicked, run OnSendClicked()!
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(OnSendClicked);
        }
    }

    private void Update()
    {
        if (IsDialogueOpen)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HideDialogue();
            }

            // Also allow pressing Enter on the keyboard to send
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OnSendClicked();
            }
        }
    }

    public void ShowDialogue()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
            playerInputField.text = "";
            playerInputField.ActivateInputField(); // Focuses keyboard cursor inside the text box
        }
    }

    public void HideDialogue()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    private void OnSendClicked()
    {
        string message = playerInputField.text.Trim();

        // Don't send empty messages
        if (string.IsNullOrEmpty(message)) return;

        Debug.Log($"[DialogueUI] Sending message: {message}");

        // 2. Clear input and show the "Thinking..." state
        playerInputField.text = "";
        dialogueContentText.text = "<i>The Gatekeeper is pondering your words...</i>";

        // Disable button while waiting so player doesn't spam requests
        sendButton.interactable = false;

        // 3. Launch the asynchronous request!
        StartCoroutine(llmService.SendMessage(
            message,
            onSuccess: (response) =>
            {
                // This code runs when the 1.5s is up and response arrives:
                dialogueContentText.text = response.reply;
                Debug.Log($"[DialogueUI] Gatekeeper responded: {response.reply} (Decision: {response.decision})");

                sendButton.interactable = true;
                playerInputField.ActivateInputField();
            },
            onError: (error) =>
            {
                // This code runs if there was an error:
                dialogueContentText.text = $"<color=red>Error: {error}</color>";
                Debug.LogError($"[DialogueUI] {error}");

                sendButton.interactable = true;
            }
        ));
    }
}