using UnityEngine;

/// <summary>
/// Simple top-down movement controller using WASD or arrow keys.
/// Uses Rigidbody2D for smooth physics-based motion.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f; // movement speed multiplier

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Get player input (WASD or Arrow keys)
        float moveX = Input.GetAxisRaw("Horizontal"); // A/D or ←/→
        float moveY = Input.GetAxisRaw("Vertical");   // W/S or ↑/↓

        // Combine inputs into a 2D direction vector
        moveInput = new Vector2(moveX, moveY).normalized;
    }

    private void FixedUpdate()
    {
        // Move the player based on physics timestep
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }
}
