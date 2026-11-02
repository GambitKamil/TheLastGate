using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

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
    }

    private void FixedUpdate()
    {
        // 4. Move the physics body at a constant rate
        rb.linearVelocity = moveInput * moveSpeed;

    }
}