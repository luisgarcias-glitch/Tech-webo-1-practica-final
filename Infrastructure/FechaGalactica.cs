using System.Text.RegularExpressions;

namespace CatalogoGalactico.Infrastructure;


public static class FechaGalactica
{
    private static readonly Regex Patron =
        new(@"^\s*(\d{1,6})\s*(BBY|ABY)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static int Parse(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            throw new ReglaNegocioException("La fecha es obligatoria (ej.: '10 BBY', '3 ABY' o 'Batalla de Yavin').");

        var t = texto.Trim();
        if (t.Equals("Batalla de Yavin", StringComparison.OrdinalIgnoreCase)) return 0;

        var m = Patron.Match(t);
        if (m.Success)
        {
            var n = int.Parse(m.Groups[1].Value);
            return m.Groups[2].Value.Equals("BBY", StringComparison.OrdinalIgnoreCase) ? -n : n;
        }

        if (int.TryParse(t, out var anio) && anio is >= -999_999 and <= 999_999) return anio;

        throw new ReglaNegocioException(
            $"Fecha inválida: '{texto}'. Use '10 BBY', '3 ABY', 'Batalla de Yavin' o un entero (-10, 0, 3).");
    }

    public static string Format(int anio) =>
        anio < 0 ? $"{-anio} BBY" : anio == 0 ? "Batalla de Yavin" : $"{anio} ABY";
}
