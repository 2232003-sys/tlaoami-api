using Tlaoami.Domain.Entities;

namespace Tlaoami.Application.Rules;

public interface IConciliacionRule
{
    string Nombre { get; }
    int Puntos { get; }
    
    Task<bool> EvaluarAsync(Pago pago);
}
