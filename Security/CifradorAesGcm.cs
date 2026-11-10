using System.Security.Cryptography;
using System.Text;

namespace CatalogoGalactico.Security;


public sealed class CifradorAesGcm : ICifrador
{
    private const int TamanoNonce = 12;
    private const int TamanoTag = 16;
    private readonly byte[] _clave;

    public CifradorAesGcm(IConfiguration configuracion)
    {
        var base64 = configuracion["Cifrado:Clave"]
            ?? throw new InvalidOperationException(
                "Falta el secreto 'Cifrado:Clave'. Configúralo con: dotnet user-secrets set \"Cifrado:Clave\" \"<clave base64 de 32 bytes>\"");

        try { _clave = Convert.FromBase64String(base64); }
        catch (FormatException)
        {
            throw new InvalidOperationException("El secreto 'Cifrado:Clave' no es un Base64 válido.");
        }

        if (_clave.Length != 32)
            throw new InvalidOperationException("El secreto 'Cifrado:Clave' debe ser de 32 bytes (256 bits) en Base64.");
    }

    public string Cifrar(string textoPlano)
    {
        var nonce = RandomNumberGenerator.GetBytes(TamanoNonce);
        var plano = Encoding.UTF8.GetBytes(textoPlano);
        var cifrado = new byte[plano.Length];
        var tag = new byte[TamanoTag];

        using var aes = new AesGcm(_clave, TamanoTag);
        aes.Encrypt(nonce, plano, cifrado, tag);

        var resultado = new byte[TamanoNonce + TamanoTag + cifrado.Length];
        nonce.CopyTo(resultado, 0);
        tag.CopyTo(resultado, TamanoNonce);
        cifrado.CopyTo(resultado, TamanoNonce + TamanoTag);
        return Convert.ToBase64String(resultado);
    }

    public string Descifrar(string textoCifrado)
    {
        var datos = Convert.FromBase64String(textoCifrado);
        var nonce = datos[..TamanoNonce];
        var tag = datos[TamanoNonce..(TamanoNonce + TamanoTag)];
        var cifrado = datos[(TamanoNonce + TamanoTag)..];
        var plano = new byte[cifrado.Length];

        using var aes = new AesGcm(_clave, TamanoTag);
        aes.Decrypt(nonce, cifrado, tag, plano);
        return Encoding.UTF8.GetString(plano);
    }
}
