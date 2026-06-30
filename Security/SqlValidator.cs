using System.Text;
using System.Text.RegularExpressions;

namespace NL2SQL.POC.Console.Security;

public static class SqlValidator {
    private static readonly string[] BlockedKeywords = 
        ["INSERT", "UPDATE", "DELETE", "DROP", "TRUNCATE", "ALTER", "CREATE", "EXEC", "REPLACE"];

    public record ValidationResult(bool IsValid, string? Reason = null);

    public static ValidationResult Validate(string? sql) {
        if (string.IsNullOrWhiteSpace(sql)) return new(false, "SQL is empty.");

        var cleanSql = StripComments(sql).Trim();
        var upper = cleanSql.ToUpperInvariant();

        if (!upper.StartsWith("SELECT") && !upper.StartsWith("WITH"))
            return new(false, "Only SELECT or CTE queries are allowed.");

        foreach (var keyword in BlockedKeywords) {
            if (Regex.IsMatch(upper, $@"\b{keyword}\b"))
                return new(false, $"Disallowed keyword detected: {keyword}");
        }
        return new(true);
    }

    private static string StripComments(string sql) {
        // Remove multi-line /* */ and single-line --
        var blockComments = @"/\*.*?\*/";
        var lineComments = @"--.*?\r?\n";
        return Regex.Replace(sql, $"{blockComments}|{lineComments}", "", RegexOptions.Singleline);
    }
}