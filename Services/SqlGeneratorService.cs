using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using NL2SQL.POC.Console.Models;

namespace NL2SQL.POC.Console.Services;

public interface ISqlGeneratorService
{
    // Added outputMode parameter
    Task<string?> GenerateQueryAsync(string question, OutputMode mode);
}

public class SqlGeneratorService(HttpClient httpClient, IConfiguration configuration) : ISqlGeneratorService
{
    public async Task<string?> GenerateQueryAsync(string question, OutputMode mode)
    {
        var apiKey = configuration["OpenRouter:ApiKey"] ?? throw new Exception("API Key missing");
        var model = configuration["OpenRouter:Model"] ?? "openai/gpt-3.5-turbo";

        // Dynamic System Prompt based on Mode
        string systemContext = mode == OutputMode.Sql 
            ? "You are a MySQL expert. Generate ONLY valid SQL." 
            : "You are an Entity Framework Core expert. Generate ONLY C# LINQ code using a DbContext named '_context'.";

        string schemaContext = mode == OutputMode.Sql
            ? @"Schema:
                Cities (Id, Name, Region)
                WeatherRecords (Id, CityId, Temperature, Humidity, RecordedAt)"
            : @"Entities:
                public class City { int Id; string Name; string Region; List<WeatherRecord> WeatherRecords; }
                public class WeatherRecord { int Id; int CityId; double Temperature; double Humidity; DateTime RecordedAt; City City; }
                DbContext: _context.Cities, _context.WeatherRecords";

        var prompt = $@"
            {systemContext}
            {schemaContext}
            Question: {question}
            Result:";

        // ✅ Correct: Uses OpenRouterMessage for the array content
        var request = new OpenRouterRequest(model, [new OpenRouterMessage("user", prompt)]);

        httpClient.DefaultRequestHeaders.Authorization = new("Bearer", apiKey);
        
        var response = await httpClient.PostAsJsonAsync("https://openrouter.ai/api/v1/chat/completions", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OpenRouterResponse>();
        return CleanCode(result?.Choices.FirstOrDefault()?.Message.Content);
    }

    private static string? CleanCode(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        // Removes markdown blocks for SQL, CSharp, or generic code
        return raw.Replace("```sql", "").Replace("```csharp", "").Replace("```", "").Trim();
    }
}