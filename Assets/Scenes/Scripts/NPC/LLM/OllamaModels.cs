using System;
using System.Collections.Generic;

[Serializable]
public class OllamaChatMessage
{
    public string role;
    public string content;
}

[Serializable]
public class OllamaChatRequest
{
    public string model;
    public bool stream;
    public string format;
    public List<OllamaChatMessage> messages;
}

[Serializable]
public class OllamaChatResponse
{
    public string model;
    public OllamaChatMessage message;
    public bool done;
}