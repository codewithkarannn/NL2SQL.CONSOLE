using System.Text;

namespace NL2SQL.POC.Console.Security;

public static class SqlValidator
{
    private static readonly string[] BlockedKeywords = 
        ["INSERT", "UPDATE", "DELETE", "DROP", "TRUNCATE", "ALTER", "CREATE", "EXEC", "GRANT"];

    public record ValidationResult(bool IsValid, string? Reason = null);

    public static ValidationResult Validate(string? sql)
    {
        if (string.IsNullOrWhiteSpace(sql)) return new(false, "SQL is empty.");

        var upper = sql.ToUpperInvariant();
        if (!upper.StartsWith("SELECT") && !upper.StartsWith("WITH"))
            return new(false, "Only SELECT/CTE queries allowed.");

        foreach (var keyword in BlockedKeywords)
        {
            if (ContainsWholeWord(upper, keyword))
                return new(false, $"Disallowed keyword detected: {keyword}");
        }

        return new(true);
    }

    private static bool ContainsWholeWord(string text, string word) =>
        System.Text.RegularExpressions.Regex.IsMatch(text, $@"\b{word}\b");
}