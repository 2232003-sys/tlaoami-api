using Tlaoami.Domain.Entities;

namespace Tlaoami.Application.Rules;

public class MontoMultiploCientoRule : IConciliacionRule
{
    public string Nombre => "Monto exacto múltiplo de 100";
    public int Puntos => 100;

    public Task<bool> EvaluarAsync(Pago pago)
    {
        var resultado = pago.Monto % 100 == 0;
        return Task.FromResult(resultado);
    }
}
