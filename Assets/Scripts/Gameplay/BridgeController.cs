
using UnityEngine;

public class BridgeController : MonoBehaviour
{
    [Header("Bridge Settings")]
    [SerializeField] private GameObject bridgeVisual;

    [Tooltip("Separate collider that blocks the crossing until activation.")]
    [SerializeField] private Collider2D bridgeBlocker;

    private bool isActivated;

    private void Start()
    {
        ApplyState();
    }

    public void ActivateBridge()
    {
        if (isActivated)
            return;

        isActivated = true;
        ApplyState();

        Debug.Log("Bridge activated!");
    }

    public void ResetBridge()
    {
        isActivated = false;
        ApplyState();

        Debug.Log("Bridge reset!");
    }

    private void ApplyState()
    {
        if (bridgeVisual != null)
        {
            bridgeVisual.SetActive(isActivated);
        }

        if (bridgeBlocker != null)
        {
            bridgeBlocker.enabled = !isActivated;
        }
    }

    public bool IsActivated()
    {
        return isActivated;
    }
}