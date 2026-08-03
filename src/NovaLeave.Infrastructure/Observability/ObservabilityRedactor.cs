using System.Text.RegularExpressions;

namespace NovaLeave.Infrastructure.Observability;

public static partial class ObservabilityRedactor
{
    public static string Redact(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var redacted = SensitiveAssignmentRegex().Replace(value, match => $"{match.Groups[1].Value}[REDACTED]");
        return ConnectionStringRegex().Replace(redacted, "[REDACTED]");
    }

    public static string SafeLabel(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "unknown";
        }

        var redacted = Redact(value);
        return redacted.Length <= 80 ? redacted : redacted[..80];
    }

    [GeneratedRegex(@"(?i)(""?\b(reason|rejectionreason|password|passwd|pwd|token|secret|key|connectionstring)\b""?\s*[:=]\s*)(""[^""]*""|[^,;}\s]+|[^;}]*)")]
    private static partial Regex SensitiveAssignmentRegex();

    [GeneratedRegex(@"(?i)(server|data source)\s*=\s*[^;}\s]+(?:;[^}\r\n]*)?")]
    private static partial Regex ConnectionStringRegex();
}
