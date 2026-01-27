namespace Tlaoami.Domain.Enums;

/// <summary>
/// Estado del match de conciliación
/// </summary>
public enum EstatusConciliacionMatch
{
    Propuesto = 0,   // Sistema lo sugirió, pendiente confirmación
    Confirmado = 1,  // Usuario confirmó, ya está aplicado
    Rechazado = 2,   // Usuario lo rechazó
}
