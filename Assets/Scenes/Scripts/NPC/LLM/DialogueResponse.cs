using System;

// [Serializable] tells Unity: "You are allowed to convert JSON text into this C# object"
[Serializable]
public class DialogueResponse
{
    public string reply;
    public string decision;
}