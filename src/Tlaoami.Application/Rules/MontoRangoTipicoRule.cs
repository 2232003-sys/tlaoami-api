using Tlaoami.Domain.Entities;

namespace Tlaoami.Application.Rules;

public class MontoRangoTipicoRule : IConciliacionRule
{
    public string Nombre => "Monto en rango típico (1000-5000)";
    public int Puntos => 60;

    public Task<bool> EvaluarAsync(Pago pago)
    {
        var resultado = pago.Monto >= 1000 && pago.Monto <= 5000;
        return Task.FromResult(resultado);
    }
}
