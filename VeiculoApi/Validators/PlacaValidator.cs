using System.Text.RegularExpressions;

namespace VeiculoApi.Validators;

public static class PlacaValidator
{
    /// <summary>Formato antigo: 3 letras + 4 dígitos (ex: ABC1234).</summary>
    private static readonly Regex OldFormat = new(@"^[A-Z]{3}[0-9]{4}$", RegexOptions.Compiled);

    /// <summary>Formato Mercosul: 3 letras + 1 dígito + 1 letra + 2 dígitos (ex: ABC1D23).</summary>
    private static readonly Regex MercosulFormat = new(@"^[A-Z]{3}[0-9][A-Z][0-9]{2}$", RegexOptions.Compiled);

    /// <summary>Remove hífens e espaços e converte para maiúsculo.</summary>
    public static string Strip(string placa) =>
        Regex.Replace(placa ?? string.Empty, @"[-\s]", "").ToUpperInvariant();

    /// <summary>Formata para "ABC-1234" ou "ABC-1D23".</summary>
    public static string Format(string placa)
    {
        var s = Strip(placa);
        return s.Length == 7 ? $"{s[..3]}-{s[3..]}" : placa;
    }

    /// <summary>Valida placa no formato antigo ou Mercosul.</summary>
    public static bool Validate(string placa)
    {
        var s = Strip(placa);
        return OldFormat.IsMatch(s) || MercosulFormat.IsMatch(s);
    }
}
