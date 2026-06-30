# NL2SQL and LINQ Generator POC

This is an AI-powered Command Line Interface (CLI) designed to translate natural language questions into valid MySQL queries or Entity Framework Core LINQ code. The project is built with .NET 9 and utilizes the OpenRouter API for large language model processing.

## Features

* **Dual Output Modes**: Supports generation of both raw MySQL SQL and C# EF Core LINQ.
* **Auto-Schema Discovery**: Includes a source code parser that extracts database structures directly from a provided DbContext .cs file.
* **Demo Mode**: Built-in fallback schema for testing without providing external source files.
* **Interactive UI**: Thread-safe loading animation and formatted console output.
* **Security First**: Implements a SqlValidator that blocks DML/DDL commands (INSERT, DELETE, DROP, etc.).
* **Smart Exit**: Graceful shutdown with proper OS exit codes.
* **Help System**: Built-in command documentation accessible via help, /help, or ?.

## Tech Stack

* **Runtime**: .NET 9
* **AI Gateway**: OpenRouter
* **Libraries**:
    * Microsoft.Extensions.Hosting: Application lifecycle and Dependency Injection.
    * Microsoft.Extensions.Http: Optimized and resilient HTTP requests.
    * System.Text.Json: High-performance JSON serialization.

## Security and API Key Handling

To prevent accidental leaking of API keys to public repositories, this project utilizes a Template Strategy.

### 1. Local Configuration
The actual appsettings.json file is ignored by Git via .gitignore. You must create this file locally to run the application.

### 2. The Template
A file named appsettings.Example.json is provided in the repository. It illustrates the required structure without containing active secrets.

### 3. Setup Instructions
1. Copy appsettings.Example.json to a new file named appsettings.json.
2. Insert your actual OpenRouter API key into appsettings.json.
3. Do not commit appsettings.json to version control.

## Configuration (appsettings.json)

```json
{
  "OpenRouter": {
    "ApiKey": "your-actual-key-here",
    "Model": "openai/gpt-3.5-turbo" 
  }
}
```

## Getting Started

1. **Clone the repository**:
   ```bash
   git clone https://github.com/youruser/NL2SQL.POC.Console.git
   cd NL2SQL.POC.Console
   ```

2. **Setup Secrets**:
   Create the local appsettings.json file based on the example provided.

3. **Restore and Build**:
   ```bash
   dotnet build
   ```

4. **Run the application**:
   ```bash
   dotnet run
   ```

## Usage

Upon startup, the application follows a configuration flow:

### 1. Schema Configuration
* Provide a full local path to an AppDbContext.cs file to analyze a custom project.
* Press Enter to utilize the built-in Demo Mode (Cities and WeatherRecords).

### 2. Mode Selection
* Select 1 for MySQL SQL output.
* Select 2 for EF Core LINQ output.

### Commands
* help or ?: Show the command help menu.
* /switch: Toggle between SQL and LINQ modes.
* exit or quit: Safely close the application.

## Schema Discovery Logic

The SourceCodeSchemaService performs static analysis on the target .cs file. It utilizes regular expressions to identify DbSet properties, extracting entity names and table names. This metadata, along with the raw source code of the DbContext, is provided to the AI to ensure the generated queries remain in sync with the actual source code structure.

## Security Logic (SQL Validator)

The SqlValidator ensures the AI only returns read-only queries by applying the following steps:
1. **Comment Stripping**: Removes -- and /* */ blocks to prevent bypasses.
2. **Whitelist Checking**: Verifies that queries start with SELECT or WITH.
3. **Blacklist Scanning**: Blocks keywords like DROP, DELETE, TRUNCATE, and EXEC using whole-word regex matching.

## Project Structure

```text
NL2SQL.POC.Console/
├── Models/
│   ├── OpenRouterModels.cs   # API DTOs
│   └── OutputMode.cs         # Mode Enums
├── Security/
│   └── SqlValidator.cs       # SQL Sanitization logic
├── Services/
│   ├── ConsoleSpinner.cs     # Background loading animation
│   └── SqlGeneratorService.cs# AI Logic
├── appsettings.json          # LOCAL ONLY (Real Keys)
├── appsettings.Example.json  # GITHUB READY (Template)
├── .gitignore                # Prevents secrets from being pushed
├── Program.cs                # Main Entry Point
└── NL2SQL.POC.Console.csproj 
```

## License

This project is licensed under the MIT License.
