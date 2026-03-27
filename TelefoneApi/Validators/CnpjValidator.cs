using System.Text.RegularExpressions;

namespace TelefoneApi.Validators;

public static class CnpjValidator
{
    private static readonly Regex NonDigits = new(@"\D+", RegexOptions.Compiled);

    public static string Strip(string cnpj) => NonDigits.Replace(cnpj, "");

    public static string Format(string cnpj)
    {
        if (cnpj.Length != 14) return cnpj;
        return $"{cnpj[..2]}.{cnpj[2..5]}.{cnpj[5..8]}/{cnpj[8..12]}-{cnpj[12..14]}";
    }

    public static bool Validate(string cnpj)
    {
        if (cnpj.Length != 14) return false;
        if (cnpj.Distinct().Count() == 1) return false;

        return CheckDigit(cnpj, [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2], 12)
            && CheckDigit(cnpj, [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2], 13);
    }

    private static bool CheckDigit(string cnpj, int[] weights, int position)
    {
        var sum = 0;
        for (var i = 0; i < weights.Length; i++)
            sum += (cnpj[i] - '0') * weights[i];

        var remainder = sum % 11;
        var expected = remainder < 2 ? 0 : 11 - remainder;
        return (cnpj[position] - '0') == expected;
    }
}
