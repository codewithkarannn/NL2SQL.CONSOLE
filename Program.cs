using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NL2SQL.POC.Console.Models;
using NL2SQL.POC.Console.Security;
using NL2SQL.POC.Console.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHttpClient<ISqlGeneratorService, SqlGeneratorService>();
builder.Services.AddTransient<ConsoleSpinner>();
using var host = builder.Build();

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("--- NL2SQL & LINQ AI Interface ---");
Console.WriteLine("Type 'help' for commands, 'exit' to close.");
Console.ResetColor();

// 1. Choose Mode
Console.WriteLine("\nSelect Output Mode:");
Console.WriteLine("1. MySQL Query");
Console.WriteLine("2. EF Core LINQ (C#)");
var choice = Console.ReadLine();
var currentMode = choice == "2" ? OutputMode.Linq : OutputMode.Sql;

Console.WriteLine($"\n Mode set to: {currentMode.ToString().ToUpper()}");

// Main Application Loop
while (true)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write($"\n[{currentMode}] Ask a question > ");
    Console.ResetColor();
    
    var question = Console.ReadLine()?.Trim();

    // 1. Check for Exit
    if (string.IsNullOrWhiteSpace(question) || 
        question.Equals("exit", StringComparison.OrdinalIgnoreCase) || 
        question.Equals("quit", StringComparison.OrdinalIgnoreCase))
    {
        break; 
    }

    // 2. Check for Help
    if (question.Equals("help", StringComparison.OrdinalIgnoreCase) || 
        question.Equals("/help", StringComparison.OrdinalIgnoreCase) || 
        question.Equals("?", StringComparison.OrdinalIgnoreCase))
    {
        ShowHelp(currentMode);
        continue;
    }

    // 3. Check for Switch
    if (question.Equals("/switch", StringComparison.OrdinalIgnoreCase))
    {
        currentMode = currentMode == OutputMode.Sql ? OutputMode.Linq : OutputMode.Sql;
        Console.WriteLine($" Switched mode to: {currentMode}");
        continue;
    }

    // 4. Process AI Request
    var sqlService = host.Services.GetRequiredService<ISqlGeneratorService>();
    using var spinner = host.Services.GetRequiredService<ConsoleSpinner>();

    try
    {
        spinner.Start("AI is generating code");
        var result = await sqlService.GenerateQueryAsync(question, currentMode);
        spinner.Stop();

        if (string.IsNullOrEmpty(result))
        {
            Console.WriteLine("AI returned an empty response. Please try rephrasing.");
            continue;
        }

        // Security Validation for SQL only
        if (currentMode == OutputMode.Sql)
        {
            var validation = SqlValidator.Validate(result);
            if (!validation.IsValid)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Security Block: {validation.Reason}");
                continue;
            }
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n--- Generated {currentMode} ---");
        Console.ResetColor();
        Console.WriteLine(result);
    }
    catch (Exception ex)
    {
        spinner.Stop();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"System Error: {ex.Message}");
    }
    finally
    {
        Console.ResetColor();
    }
}

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("\nShutting down... Goodbye!");
Console.ResetColor();
Environment.Exit(0);

// Helper method to keep the main loop clean
static void ShowHelp(OutputMode currentMode)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("\n---------------- COMMAND HELP ----------------");
    Console.WriteLine($" Current Mode: {currentMode}");
    Console.WriteLine(" ---------------------------------------------");
    Console.WriteLine("  help / ?   - Show this information");
    Console.WriteLine("  /switch    - Toggle between SQL and EF LINQ");
    Console.WriteLine("  exit       - Close the application");
    Console.WriteLine("\n TIPS:");
    Console.WriteLine(" - For SQL: Ask for tables 'Cities' or 'WeatherRecords'.");
    Console.WriteLine(" - For LINQ: Ask for counts, filters, or averages.");
    Console.WriteLine("----------------------------------------------");
    Console.ResetColor();
}