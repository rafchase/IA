using System.Text.RegularExpressions;

namespace CpfApi.Validators;

public static class CpfValidator
{
    private static readonly Regex NonDigits = new(@"\D", RegexOptions.Compiled);

    /// <summary>
    /// Remove dots and dashes from a CPF string.
    /// </summary>
    public static string Strip(string cpf) => NonDigits.Replace(cpf, "");

    /// <summary>
    /// Format a stripped CPF to "000.000.000-00".
    /// </summary>
    public static string Format(string cpf)
    {
        var d = Strip(cpf);
        if (d.Length != 11) return cpf;
        return $"{d[..3]}.{d[3..6]}.{d[6..9]}-{d[9..11]}";
    }

    /// <summary>
    /// Validates a CPF using the mod-11 check digit algorithm.
    /// Accepts formatted ("000.000.000-00") or unformatted (11 digits).
    /// Returns true if valid.
    /// </summary>
    public static bool Validate(string cpf)
    {
        var digits = Strip(cpf);

        if (digits.Length != 11)
            return false;

        // Reject all-same-digit CPFs (e.g., 111.111.111-11)
        if (digits.Distinct().Count() == 1)
            return false;

        // Validate first check digit
        if (!ValidateDigit(digits, 9))
            return false;

        // Validate second check digit
        if (!ValidateDigit(digits, 10))
            return false;

        return true;
    }

    private static bool ValidateDigit(string digits, int position)
    {
        int sum = 0;
        int weight = position + 1;

        for (int i = 0; i < position; i++)
        {
            sum += (digits[i] - '0') * weight;
            weight--;
        }

        int remainder = sum % 11;
        int expected = remainder < 2 ? 0 : 11 - remainder;
        int actual = digits[position] - '0';

        return actual == expected;
    }
}
