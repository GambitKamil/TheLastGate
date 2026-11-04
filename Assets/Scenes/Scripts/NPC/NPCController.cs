using UnityEngine;


public class NPCController : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("Guard: Halt! Who goes there? State your business!");
    }
}