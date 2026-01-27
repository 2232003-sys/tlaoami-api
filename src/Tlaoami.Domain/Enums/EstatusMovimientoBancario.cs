namespace Tlaoami.Domain.Enums;

/// <summary>
/// Estado del movimiento bancario en el proceso de conciliación
/// </summary>
public enum EstatusMovimientoBancario
{
    Nuevo = 0,               // Importado, sin procesar
    MatchPropuesto = 1,      // Sistema sugirió un match
    Conciliado = 2,          // Confirmado y aplicado
    Ignorado = 3,            // Movimiento ignorado/sin relevancia para escuela
}
