using System.Text.Json.Serialization;

namespace CatalogoGalactico.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Faccion { Rebelde, Imperio, Neutral }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EstadoPersonaje { Vivo, Muerto, Desconocido }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TipoHabilidad { Fuerza, Combate, Tecnica, Liderazgo, Pilotaje }
