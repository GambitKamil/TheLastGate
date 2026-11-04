using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

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
        }
    }
    private void Awake()
    {
        // 1. Cache the Rigidbody2D component attached to this GameObject
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // 2. Read Keyboard Inputs (-1, 0, or 1)
        float moveX = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        float moveY = Input.GetAxisRaw("Vertical");   // W/S or Up/Down

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