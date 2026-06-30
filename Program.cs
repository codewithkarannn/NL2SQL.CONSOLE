using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NL2SQL.POC.Console.Models;
using NL2SQL.POC.Console.Security;
using NL2SQL.POC.Console.Services;

var builder = Host.CreateApplicationBuilder(args);

// Register all services
builder.Services.AddHttpClient<ISqlGeneratorService, SqlGeneratorService>();
builder.Services.AddSingleton<ISourceCodeSchemaService, SourceCodeSchemaService>();
builder.Services.AddTransient<ConsoleSpinner>();

using var host = builder.Build();

// --- Header ---
Console.Clear();
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("--- NL2SQL & LINQ AI Interface v2.0 ---");
Console.WriteLine("Type 'help' for commands, 'exit' to close.");
Console.ResetColor();

// --- 1. Schema Configuration (The Discovery Step) ---
Console.WriteLine("\n[CONFIG] Setup Database Schema:");
Console.WriteLine(" > Enter path to your AppDbContext.cs file");
Console.WriteLine(" > Or press [ENTER] to use the built-in Demo Schema");
Console.Write("\nPath: ");

var path = Console.ReadLine()?.Trim();
string activeSchema;
bool isDemoMode = string.IsNullOrEmpty(path);

if (isDemoMode)
{
    activeSchema = DemoData.DefaultSchema;
    Console.ForegroundColor = ConsoleColor.Magenta;
    Console.WriteLine("🚀 Demo Mode activated (Cities & WeatherRecords).");
}
else
{
    try
    {
        var discovery = host.Services.GetRequiredService<ISourceCodeSchemaService>();
        activeSchema = discovery.ExtractSchemaFromLocalFile(path!);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✅ Custom Schema loaded from: {Path.GetFileName(path)}");
    }
    catch (Exception ex)
    {
        activeSchema = DemoData.DefaultSchema;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"⚠️ Error: {ex.Message}. Falling back to Demo Mode.");
    }
}
Console.ResetColor();

// --- 2. Initial Mode Selection ---
Console.WriteLine("\nSelect Output Mode:");
Console.WriteLine("1. MySQL Query");
Console.WriteLine("2. EF Core LINQ (C#)");
var choice = Console.ReadLine();
var currentMode = choice == "2" ? OutputMode.Linq : OutputMode.Sql;

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine($"\n⭐ Mode set to: {currentMode.ToString().ToUpper()}");
Console.ResetColor();

// --- 3. Main Application Loop ---
while (true)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write($"\n[{currentMode}] Ask a question > ");
    Console.ResetColor();
    
    var question = Console.ReadLine()?.Trim();

    // Command: Exit
    if (string.IsNullOrWhiteSpace(question) || 
        question.Equals("exit", StringComparison.OrdinalIgnoreCase) || 
        question.Equals("quit", StringComparison.OrdinalIgnoreCase))
    {
        break; 
    }

    // Command: Help
    if (question.Equals("help", StringComparison.OrdinalIgnoreCase) || 
        question.Equals("/help", StringComparison.OrdinalIgnoreCase) || 
        question.Equals("?", StringComparison.OrdinalIgnoreCase))
    {
        ShowHelp(currentMode, isDemoMode);
        continue;
    }

    // Command: Switch
    if (question.Equals("/switch", StringComparison.OrdinalIgnoreCase))
    {
        currentMode = currentMode == OutputMode.Sql ? OutputMode.Linq : OutputMode.Sql;
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"🔄 Switched mode to: {currentMode}");
        Console.ResetColor();
        continue;
    }

    // Process AI Request
    var sqlService = host.Services.GetRequiredService<ISqlGeneratorService>();
    using var spinner = host.Services.GetRequiredService<ConsoleSpinner>();

    try
    {
        spinner.Start("AI is generating code");
        // Pass the dynamically discovered schema here
        var result = await sqlService.GenerateQueryAsync(question, currentMode, activeSchema);
        spinner.Stop();

        if (string.IsNullOrEmpty(result))
        {
            Console.WriteLine("⚠️ AI returned an empty response. Please try rephrasing.");
            continue;
        }

        // Security Validation for SQL only
        if (currentMode == OutputMode.Sql)
        {
            var validation = SqlValidator.Validate(result);
            if (!validation.IsValid)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Security Block: {validation.Reason}");
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

// Helper method for the Help Menu
static void ShowHelp(OutputMode currentMode, bool isDemo)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("\n---------------- COMMAND HELP ----------------");
    Console.WriteLine($" Current Mode: {currentMode}");
    Console.WriteLine($" Active Schema: {(isDemo ? "DEMO (Cities/Weather)" : "CUSTOM (Parsed from file)")}");
    Console.WriteLine(" ---------------------------------------------");
    Console.WriteLine("  help / ?   - Show this information");
    Console.WriteLine("  /switch    - Toggle between SQL and EF LINQ");
    Console.WriteLine("  exit       - Close the application");
    Console.WriteLine("\n TIPS:");
    if (isDemo)
        Console.WriteLine(" - Use tables: Cities, WeatherRecords");
    else
        Console.WriteLine(" - Use the Entity/Table names from your DbContext file.");
    
    Console.WriteLine(" - Be specific about filters (e.g., 'top 5', 'yesterday').");
    Console.WriteLine("----------------------------------------------");
    Console.ResetColor();
}