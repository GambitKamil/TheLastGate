using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class OllamaService : MonoBehaviour, ILLMService
{
    [Header("Ollama Configuration")]
    [SerializeField] private string ollamaUrl = "http://localhost:11434/api/chat";
    [SerializeField] private string modelName = "llama3.2:1b";

    [TextArea(5, 10)]
    [SerializeField]
    private string systemPrompt =
        "You are the stern Gatekeeper of The Last Gate. The gate is currently locked. " +
        "Listen to the player's request. " +
        "If the player provides a noble, convincing reason, or flatters your honor: set decision to 'OPEN_GATE'. " +
        "If the player is rude, insulting, or gives an obvious lie: set decision to 'DENY'. " +
        "If the conversation should continue: set decision to 'CONTINUE'. " +
        "You MUST respond ONLY in valid JSON matching this exact structure: " +
        "{\"reply\": \"Your dialogue text here\", \"decision\": \"OPEN_GATE\" | \"DENY\" | \"CONTINUE\"}";

    public IEnumerator SendMessage(
        string playerMessage,
        Action<DialogueResponse> onSuccess,
        Action<string> onError)
    {
        // 1. Build the Ollama request payload
        OllamaChatRequest requestData = new OllamaChatRequest
        {
            model = modelName,
            stream = false,
            format = "json",
            messages = new List<OllamaChatMessage>
            {
                new OllamaChatMessage { role = "system", content = systemPrompt },
                new OllamaChatMessage { role = "user", content = playerMessage }
            }
        };

        string jsonPayload = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        // 2. Create the UnityWebRequest POST
        using (UnityWebRequest webRequest = new UnityWebRequest(ollamaUrl, "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.timeout = 60; // 60-second safety timeout

            Debug.Log("[OllamaService] Sending request to local Ollama server...");

            // 3. Asynchronously wait for the server response without freezing the game!
            yield return webRequest.SendWebRequest();

            // 4. Handle connection / network errors
            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                string errorMsg = $"Ollama Error: {webRequest.error} (Is Ollama running on localhost:11434?)";
                Debug.LogError(errorMsg);
                onError?.Invoke(errorMsg);
                yield break;
            }

            // 5. Parse the Ollama response envelope
            string responseText = webRequest.downloadHandler.text;
            Debug.Log($"[OllamaService] Raw Response: {responseText}");

            OllamaChatResponse chatResponse = null;
            try
            {
                chatResponse = JsonUtility.FromJson<OllamaChatResponse>(responseText);
            }
            catch (Exception ex)
            {
                onError?.Invoke($"Failed to parse Ollama envelope: {ex.Message}");
                yield break;
            }

            if (chatResponse == null || chatResponse.message == null || string.IsNullOrEmpty(chatResponse.message.content))
            {
                onError?.Invoke("Received empty message from Ollama.");
                yield break;
            }

            // 6. Parse the inner JSON containing reply and decision
            try
            {
                DialogueResponse decisionResponse = JsonUtility.FromJson<DialogueResponse>(chatResponse.message.content);
                onSuccess?.Invoke(decisionResponse);
            }
            catch (Exception ex)
            {
                onError?.Invoke($"Malformed JSON in Gatekeeper response: {ex.Message}\nRaw: {chatResponse.message.content}");
            }
        }
    }
}