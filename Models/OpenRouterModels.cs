namespace NL2SQL.POC.Console.Models;

public record OpenRouterRequest(
    string model, 
    OpenRouterMessage[] messages
);

public record OpenRouterMessage(
    string role, 
    string content
);

public record Message(string Role, string Content);

public record OpenRouterResponse
{
    public Choice[] Choices { get; init; } = [];
}

public record Choice
{
    public Message Message { get; init; } = null!;
}
