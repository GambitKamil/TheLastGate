using TMPro;
using UnityEngine;

public class CompanionCommand : MonoBehaviour
{
    [SerializeField] private CompanionAgent companion;
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private TMP_Text hintText;

    [SerializeField, Min(0.1f)] private float commandDistance = 2f;
    [SerializeField] private KeyCode commandKey = KeyCode.C;

    private bool commandSent;

    private void Update()
    {
        bool dialogueOpen =
            dialogueUI != null && dialogueUI.IsDialogueOpen;

        bool canCommand =
            companion != null &&
            companion.isActiveAndEnabled &&
            !commandSent &&
            !companion.TargetReached &&
            !dialogueOpen &&
            Vector2.Distance(
                transform.position,
                companion.transform.position
            ) <= commandDistance;

        SetHint(canCommand);

        if (canCommand && Input.GetKeyDown(commandKey))
        {
            companion.StartNavigation();
            commandSent = true;
            SetHint(false);
        }
    }

    private void SetHint(bool visible)
    {
        if (hintText == null)
            return;

        hintText.text =
            $"[{commandKey}] Yoldaşına şaltere gitmesini söyle";

        if (hintText.gameObject.activeSelf != visible)
        {
            hintText.gameObject.SetActive(visible);
        }
    }

    private void OnDisable()
    {
        SetHint(false);
    }
}