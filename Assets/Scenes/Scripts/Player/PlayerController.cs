using UnityEngine;

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IInteractable interactable = collision.GetComponent<IInteractable>();
        if (interactable != null)
        {
            currentInteractable = interactable;
            Debug.Log("Entered interaction zone. Press E to interact!");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // If we step away from that interactable, clear it
        if (collision.GetComponent<IInteractable>() == currentInteractable)
        {
            currentInteractable = null;
            Debug.Log("Exited interaction zone.");
            // Automatically close dialogue when walking away!
            if (dialogueUI != null)
            {
                dialogueUI.HideDialogue();
            }
        }
    }
    private void Awake()
    {
        // 1. Cache the Rigidbody2D component attached to this GameObject
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {

        // If CanMove is false or dialogue is open, freeze player completely!
        if (!CanMove || (dialogueUI != null && dialogueUI.IsDialogueOpen))
        {
            moveInput = Vector2.zero; // Stops physics movement
            return;                   // Skips reading WASD and E
        }
        // 2. Read Keyboard Inputs (-1, 0, or 1)
        float moveX = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        float moveY = Input.GetAxisRaw("Vertical");   // W/S or Up/Down

        // Update animation state (true if player is moving in any direction)
        bool isMoving = moveInput.sqrMagnitude > 0.01f;
        if (animator != null)
        {
            animator.SetBool("IsMoving", isMoving);
        }

        // Flip sprite horizontally when moving left or right
        if (moveX > 0.01f && spriteRenderer != null)
        {
            spriteRenderer.flipX = false; // Facing Right
        }
        else if (moveX < -0.01f && spriteRenderer != null)
        {
            spriteRenderer.flipX = true;  // Facing Left
        }

        // 3. Normalize so diagonal walking isn't 41% faster!
        moveInput = new Vector2(moveX, moveY).normalized;

        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    private void FixedUpdate()
    {
        // 4. Move the physics body at a constant rate
        rb.linearVelocity = moveInput * moveSpeed;

    }
}