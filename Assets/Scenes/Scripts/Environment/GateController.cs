using System.Collections;
using UnityEngine;

public class GateController : MonoBehaviour
{
    [Header("Gate Settings")]
    [SerializeField] private float slideDistance = 4f; // How high the gate lifts
    [SerializeField] private float openSpeed = 2f;      // Speed of lifting

    private BoxCollider2D gateCollider;
    private bool isOpen = false;

    private void Awake()
    {
        gateCollider = GetComponent<BoxCollider2D>();
    }

    /// <summary>
    /// Opens the gate smoothly and removes the solid collision.
    /// </summary>
    public void OpenGate()
    {
        if (isOpen) return;
        isOpen = true;

        Debug.Log("[GateController] The Gate is opening!");

        // Disable solid collision so player can pass freely
        if (gateCollider != null)
        {
            gateCollider.enabled = false;
        }

        // Smoothly slide the gate upwards
        StartCoroutine(SlideOpenRoutine());
    }

    private IEnumerator SlideOpenRoutine()
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + new Vector3(0, slideDistance, 0);

        while (Vector3.Distance(transform.position, targetPosition) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                openSpeed * Time.deltaTime
            );
            yield return null; // Wait for the next frame
        }

        transform.position = targetPosition;
        Debug.Log("[GateController] Gate is fully open!");
    }
}