using Tlaoami.Domain.Entities;

namespace Tlaoami.Application.Rules;

public class FechaDiaEspecialRule : IConciliacionRule
{
    public string Nombre => "Fecha común de pago (día 1 o 15)";
    public int Puntos => 80;

    public Task<bool> EvaluarAsync(Pago pago)
    {
        var resultado = pago.FechaPago.Day == 1 || pago.FechaPago.Day == 15;
        return Task.FromResult(resultado);
    }
}
