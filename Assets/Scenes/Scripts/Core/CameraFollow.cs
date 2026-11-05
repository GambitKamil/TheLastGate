using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target to Follow")]
    [SerializeField] private Transform target;

    [Header("Follow Settings")]
    [Range(1f, 15f)]
    [SerializeField] private float smoothSpeed = 5f; // How smoothly camera catches up
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f); // Keeps camera at Z = -10


    private void LateUpdate()
    {
        if (target == null) return;

        // 1. Calculate where the camera should be
        Vector3 desiredPosition = target.position + offset;

        // 2. Smoothly slide from current position to desired position
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );
    }
}