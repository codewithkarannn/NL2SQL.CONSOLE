using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using NL2SQL.POC.Console.Models;

namespace NL2SQL.POC.Console.Services;

public interface ISqlGeneratorService {
    Task<string?> GenerateQueryAsync(string question, OutputMode mode, string schema);
}

public class SqlGeneratorService(HttpClient httpClient, IConfiguration configuration) : ISqlGeneratorService {
    public async Task<string?> GenerateQueryAsync(string question, OutputMode mode, string schema) {
        var apiKey = configuration["OpenRouter:ApiKey"] ?? throw new Exception("API Key Missing");
        var model = configuration["OpenRouter:Model"] ?? "openai/gpt-3.5-turbo";

        var prompt = $@"You are a technical expert. 
            SCHEMA: {schema}
            TASK: Generate ONLY {(mode == OutputMode.Sql ? "MySQL SQL" : "C# LINQ using '_context'")}
            QUESTION: ""{question}""
            RULE: Return ONLY code. No explanations.";

        var request = new OpenRouterRequest(model, [new OpenRouterMessage("user", prompt)]);
        httpClient.DefaultRequestHeaders.Authorization = new("Bearer", apiKey);
        
        var response = await httpClient.PostAsJsonAsync("https://openrouter.ai/api/v1/chat/completions", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<OpenRouterResponse>();
        
        return result?.Choices.FirstOrDefault()?.Message.Content
            .Replace("```sql", "").Replace("```csharp", "").Replace("```", "").Trim();
    }
}