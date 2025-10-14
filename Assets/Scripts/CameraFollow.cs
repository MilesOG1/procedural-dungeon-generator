using UnityEngine;

/// <summary>
/// Simple camera follow script for 2D games.
/// Attach to Main Camera. Follows the Player smoothly.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target to Follow")]
    public Transform target; // The player

    [Header("Follow Settings")]
    public float smoothSpeed = 5f; // Higher = faster following
    public Vector3 offset = new Vector3(0f, 0f, -10f); // Keep camera behind the scene

    private void LateUpdate()
    {
        if (target == null) return; // No target, do nothing

        // Desired position is player's position + offset
        Vector3 desiredPosition = target.position + offset;

        // Smoothly move the camera toward that position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        transform.position = smoothedPosition;
    }
}

