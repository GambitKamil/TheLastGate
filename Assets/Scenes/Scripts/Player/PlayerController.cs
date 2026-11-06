using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private DialogueUI dialogueUI;

    [Header("Visuals & Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    public static bool CanMove { get; set; } = true;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private IInteractable currentInteractable;

    private bool MovementBlocked =>
        !CanMove ||
        (dialogueUI != null && dialogueUI.IsDialogueOpen);

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        CanMove = true;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // This project starts each scene with an unlocked player.
        CanMove = true;
        moveInput = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
    }

    private void Update()
    {
        if (MovementBlocked)
        {
            moveInput = Vector2.zero;
            UpdateAnimation();
            return;
        }

        moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        UpdateAnimation();

        if (spriteRenderer != null)
        {
            if (moveInput.x > 0.01f)
                spriteRenderer.flipX = false;
            else if (moveInput.x < -0.01f)
                spriteRenderer.flipX = true;
        }

        if (Input.GetKeyDown(KeyCode.E) &&
            currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = MovementBlocked
            ? Vector2.zero
            : moveInput * moveSpeed;
    }

    private void UpdateAnimation()
    {
        if (animator != null)
        {
            animator.SetBool(
                "IsMoving",
                moveInput.sqrMagnitude > 0.01f
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IInteractable interactable =
            collision.GetComponent<IInteractable>();

        if (interactable != null)
        {
            currentInteractable = interactable;
            Debug.Log("Entered interaction zone. Press E.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        IInteractable interactable =
            collision.GetComponent<IInteractable>();

        if (interactable != null &&
            interactable == currentInteractable)
        {
            currentInteractable = null;

            if (dialogueUI != null)
            {
                dialogueUI.HideDialogue();
            }
        }
    }

    private void OnDisable()
    {
        moveInput = Vector2.zero;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}