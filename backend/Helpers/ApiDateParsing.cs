using System.Globalization;

namespace Backend.Helpers;

/// <summary>
/// Interpreta datas enviadas pelo front (JSON) como string — evita falhas de binding em DateTime/DateTimeOffset.
/// </summary>
public static class ApiDateParsing
{
    public static bool TryParseUtc(string? value, out DateTime utc, out string? error)
    {
        utc = default;
        error = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            error = "Data vazia.";
            return false;
        }

        var t = value.Trim();

        if (DateTimeOffset.TryParse(t, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dto))
        {
            utc = DateTime.SpecifyKind(dto.UtcDateTime, DateTimeKind.Utc);
            return ValidateNotSentinel(utc, out error);
        }

        if (DateTimeOffset.TryParse(
                t,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out dto))
        {
            utc = DateTime.SpecifyKind(dto.UtcDateTime, DateTimeKind.Utc);
            return ValidateNotSentinel(utc, out error);
        }

        if (DateTime.TryParse(
                t,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var dt))
        {
            utc = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            return ValidateNotSentinel(utc, out error);
        }

        error = "Formato de data não reconhecido. Use ISO-8601 (ex.: 2026-04-08T12:00:00.000Z).";
        return false;
    }

    private static bool ValidateNotSentinel(DateTime utc, out string? error)
    {
        error = null;
        if (utc.Year < 1900)
        {
            error = "Informe uma data com ano válido (1900 ou posterior).";
            return false;
        }

        return true;
    }
}
