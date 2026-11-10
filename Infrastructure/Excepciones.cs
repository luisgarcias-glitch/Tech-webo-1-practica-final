namespace CatalogoGalactico.Infrastructure;

public class NotFoundException(string mensaje) : Exception(mensaje);

public class ReglaNegocioException(string mensaje) : Exception(mensaje);

public class ConflictException(string mensaje) : Exception(mensaje);
