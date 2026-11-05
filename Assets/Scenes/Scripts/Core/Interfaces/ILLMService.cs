using System;
using System.Collections;

public interface ILLMService
{
    IEnumerator SendMessage(
        string playerMessage,
        Action<DialogueResponse> onSuccess,
        Action<string> onError
    );
}