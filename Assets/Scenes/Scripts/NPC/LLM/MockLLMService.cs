using System;
using System.Collections;
using UnityEngine;

public class MockLLMService : MonoBehaviour, ILLMService
{
    public IEnumerator SendMessage(
        string playerMessage,
        Action<DialogueResponse> onSuccess,
        Action<string> onError)
    {
        Debug.Log($"[MockLLM] Received player message: \"{playerMessage}\". Simulating thinking...");

        // 1. Simulate 1.5 seconds of network delay (without freezing the game!)
        yield return new WaitForSeconds(1.5f);

        // 2. Simulate an error test if the user types "error"
        if (playerMessage.ToLower().Contains("error"))
        {
            onError?.Invoke("Mock Error: Failed to reach the Gatekeeper server!");
            yield break;
        }

        // 3. Create a fake AI response based on what was typed
        DialogueResponse mockResponse = new DialogueResponse();

        if (playerMessage.ToLower().Contains("open"))
        {
            mockResponse.reply = "Your words are honorable. I will open the gate for you.";
            mockResponse.decision = "OPEN_GATE";
        }
        else
        {
            mockResponse.reply = $"You say '{playerMessage}', but that does not convince me. State your true purpose!";
            mockResponse.decision = "CONTINUE";
        }

        // 4. Send the result back via the success callback!
        onSuccess?.Invoke(mockResponse);
    }
}