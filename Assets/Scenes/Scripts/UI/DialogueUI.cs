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

    [Header("Error Popup")]
    [SerializeField] private GameObject errorPanel;
    [SerializeField] private TextMeshProUGUI errorMessageText;
    [SerializeField] private Button errorDismissButton;


    [Header("Services")]
    [SerializeField] private OllamaService llmService;

    [Header("Environment")]
    [SerializeField] private GateController gateController;
    private bool isGateUnlocked = false;

    public bool IsDialogueOpen => dialoguePanel != null && dialoguePanel.activeSelf;

    private void Start()
    {
        HideDialogue();
        HideError();

        // Wire up the Send button
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(OnSendClicked);
        }

        // Wire up the Error Dismiss button
        if (errorDismissButton != null)
        {
            errorDismissButton.onClick.AddListener(HideError);
        }
    }

    private void Update()
    {
        if (IsDialogueOpen)
        {
            // If the gate is already unlocked, ANY key closes dialogue so player can walk!
            if (isGateUnlocked && Input.anyKeyDown)
            {
                HideDialogue();
                return;
            }

            // Escape closes dialogue (unless error popup is showing)
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (errorPanel != null && errorPanel.activeSelf)
                {
                    HideError();
                }
                else
                {
                    HideDialogue();
                }
            }

            // Enter key sends the message (only if gate is not yet unlocked)
            if (!isGateUnlocked && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
            {
                if (errorPanel == null || !errorPanel.activeSelf)
                {
                    OnSendClicked();
                }
            }
        }
    }

    public void ShowDialogue()
    {
        if (isGateUnlocked)
        {
            // If gate is already open, just show a friendly reminder!
            dialoguePanel.SetActive(true);
            dialogueContentText.text = "The Gatekeeper smiles: <i>\"The gate is already open, traveler. Go forth!\"</i>\n\n<color=#A0AEC0><i>[Press any key to proceed]</i></color>";
            playerInputField.gameObject.SetActive(false);
            sendButton.gameObject.SetActive(false);
            return;
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
            playerInputField.text = "";
            playerInputField.ActivateInputField();
        }
    }
    public void HideDialogue()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    public void ShowError(string friendlyMessage)
    {
        if (errorMessageText != null)
        {
            errorMessageText.text = friendlyMessage;
        }

        if (errorPanel != null)
        {
            errorPanel.SetActive(true);
        }

        // Restore send button so player can try again
        sendButton.interactable = true;
    }

    public void HideError()
    {
        if (errorPanel != null)
        {
            errorPanel.SetActive(false);
        }

        // Re-focus the input box so the player can keep typing
        playerInputField.ActivateInputField();
    }

    private void OnSendClicked()
    {
        string message = playerInputField.text.Trim();
        if (string.IsNullOrEmpty(message)) return;

        Debug.Log($"[DialogueUI] Sending message: {message}");

        playerInputField.text = "";
        dialogueContentText.text = "<i>Kapı muhafızı düşünüyor...</i>";
        sendButton.interactable = false;

        StartCoroutine(llmService.SendMessage(
            message,
            onSuccess: (response) =>
            {
                Debug.Log($"[DialogueUI] Gatekeeper responded: {response.reply} (Decision: {response.decision})");
                if (response.decision == "OPEN_GATE")
                {
                    isGateUnlocked = true;
                    // Display the Gatekeeper's parting words + instruction
                    dialogueContentText.text = $"{response.reply}\n\n<color=#A0AEC0><i>[Press any key to proceed]</i></color>";
                    // Lock the inputs so the AI cannot re-lock the gate!
                    playerInputField.interactable = false;
                    sendButton.interactable = false;
                    // Open the physical gate
                    if (gateController != null)
                    {
                        gateController.OpenGate();
                    }
                }
                else
                {
                    // Normal conversation continues
                    dialogueContentText.text = response.reply;
                    sendButton.interactable = true;
                    playerInputField.ActivateInputField();
                }
            },
            onError: (error) =>
            {
                Debug.LogError($"[DialogueUI] {error}");

                // Reset dialogue text back to normal
                dialogueContentText.text = "The Gatekeeper stares at you in silence.";

                // Pop up our clean error dialog!
                ShowError("Muhafıza ulaşılamadı. Yerel Ollama sunucusunun çalıştığını kontrol edip tekrar deneyin.");
            }
        ));
    }
}