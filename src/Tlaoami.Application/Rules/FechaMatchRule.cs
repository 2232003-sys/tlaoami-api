using Tlaoami.Domain.Entities;

namespace Tlaoami.Application.Rules;

public class FechaMatchRule : IMatchRule
{
    public string Nombre => "Coincidencia de Fecha";

    public Task<MatchRuleResult> EvaluarAsync(Pago pago)
    {
        var ahora = DateTime.UtcNow;
        var diferenciaDias = Math.Abs((ahora.Date - pago.FechaPago.Date).Days);

        // Fecha reciente (misma fecha o dentro de 1 día)
        if (diferenciaDias <= 1)
        {
            return Task.FromResult(new MatchRuleResult
            {
                Score = 30,
                Reason = $"Fecha reciente ({diferenciaDias} día(s) de diferencia)"
            });
        }

        // Fecha dentro de 7 días
        if (diferenciaDias <= 7)
        {
            return Task.FromResult(new MatchRuleResult
            {
                Score = 15,
                Reason = $"Fecha dentro de 7 días ({diferenciaDias} días)"
            });
        }

        // Fecha dentro de mes
        if (diferenciaDias <= 30)
        {
            return Task.FromResult(new MatchRuleResult
            {
                Score = 5,
                Reason = $"Fecha dentro del mes ({diferenciaDias} días)"
            });
        }

        return Task.FromResult(new MatchRuleResult
        {
            Score = 0,
            Reason = "Fecha muy antigua"
        });
    }
}
