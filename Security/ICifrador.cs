namespace CatalogoGalactico.Security;

public interface ICifrador
{
    string Cifrar(string textoPlano);
    string Descifrar(string textoCifrado);
}
