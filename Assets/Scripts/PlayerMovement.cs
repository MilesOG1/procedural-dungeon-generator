using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    private Vector2 movement;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        Vector2 movement = Vector2.zero; // Get raw WASD input

        if (Input.GetKey(KeyCode.W))
            movement.y += 1f;
        if (Input.GetKey(KeyCode.S))
            movement.y -= 1f;
        if (Input.GetKey(KeyCode.A))
            movement.x -= 1f;
        if (Input.GetKey(KeyCode.D))
            movement.x += 1f;

        movement = movement.normalized;

        rb.MovePosition(rb.position + movement * moveSpeed * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = movement * moveSpeed;
    }

}
