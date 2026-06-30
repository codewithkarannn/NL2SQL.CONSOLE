using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using NL2SQL.POC.Console.Models;

namespace NL2SQL.POC.Console.Services;

// --- 1. Schema Discovery ---
public interface ISourceCodeSchemaService {
    string ExtractSchemaFromLocalFile(string contextFilePath);
}

public class SourceCodeSchemaService : ISourceCodeSchemaService {
    public string ExtractSchemaFromLocalFile(string contextFilePath) {
        if (!File.Exists(contextFilePath)) throw new FileNotFoundException("File not found.");
        string content = File.ReadAllText(contextFilePath);
        var matches = Regex.Matches(content, @"DbSet<(?<Entity>\w+)>\s+(?<Table>\w+)");
        
        var sb = new System.Text.StringBuilder("Parsed Schema from Source:\n");
        foreach (Match m in matches) 
            sb.AppendLine($"- Table: {m.Groups["Table"].Value} (Entity: {m.Groups["Entity"].Value})");
        
        sb.AppendLine("\nFull Context Code for AI Analysis:\n" + content);
        return sb.ToString();
    }
}


