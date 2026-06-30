# NL2SQL & LINQ Generator POC

An AI-powered Command Line Interface (CLI) that translates natural language questions into valid **MySQL queries** or **Entity Framework Core LINQ** code. Built with .NET 9 and powered by the OpenRouter API.

## Features

- **Dual Output Modes**: Switch between raw MySQL SQL and C# EF Core LINQ.
- **Interactive UI**: Thread-safe loading spinner and color-coded console output.
- **Security First**: Built-in `SqlValidator` that blocks DML/DDL commands (INSERT, DELETE, DROP, etc.).
- **Smart Exit**: Graceful shutdown with proper OS exit codes.
- **Help System**: Built-in command documentation (`help`, `/help`, `?`).

## Tech Stack

- **Runtime**: .NET 9
- **AI Gateway**: OpenRouter
- **Libraries**:
    - `Microsoft.Extensions.Hosting`: Application lifecycle and DI.
    - `Microsoft.Extensions.Http`: Optimized HTTP requests.
    - `System.Text.Json`: High-performance JSON serialization.

## Security & API Key Handling

To prevent accidental leaking of API keys to public repositories, this project uses a **Template Strategy**.

### 1. Local Configuration
The real `appsettings.json` is ignored by Git (via `.gitignore`). You must create this file locally to run the app.

### 2. The Template
`appsettings.Example.json` is provided in the repository. It shows the structure required without containing real secrets.

### 3. Setup Instructions
1.  Copy `appsettings.Example.json` to a new file named `appsettings.json`.
2.  Paste your actual API key into `appsettings.json`.
3.  **Never** commit `appsettings.json` to Git.

## Configuration (`appsettings.json`)

```json
{
  "OpenRouter": {
    "ApiKey": "sk-or-v1-your-actual-key-here",
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
   Create `appsettings.json` based on the example provided.

3. **Restore and Build**:
   ```bash
   dotnet build
   ```

4. **Run the application**:
   ```bash
   dotnet run
   ```

## Usage

Upon startup, select your mode (1 for SQL, 2 for LINQ).

### Commands
- `help` or `?`: Show the command help menu.
- `/switch`: Toggle between SQL and LINQ modes.
- `exit` or `quit`: Safely close the application.

### Example Questions
- **SQL Mode**: "Show me the top 3 cities with the highest humidity."
- **LINQ Mode**: "Get all weather records for the city of London."

## Security Logic (SQL Validator)

The `SqlValidator` ensures the AI only returns read-only queries by:
1. **Comment Stripping**: Removes `--` and `/* */` blocks to prevent bypasses.
2. **Whitelist Checking**: Ensures queries start with `SELECT` or `WITH`.
3. **Blacklist Scanning**: Blocks keywords like `DROP`, `DELETE`, `TRUNCATE`, and `EXEC` using whole-word regex matching.

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