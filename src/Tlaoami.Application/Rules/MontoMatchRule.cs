using Tlaoami.Domain.Entities;

namespace Tlaoami.Application.Rules;

public class MontoMatchRule : IMatchRule
{
    public string Nombre => "Coincidencia de Monto";

    public Task<MatchRuleResult> EvaluarAsync(Pago pago)
    {
        // Monto exacto múltiplo de 100: alta confianza
        if (pago.Monto % 100 == 0)
        {
            return Task.FromResult(new MatchRuleResult
            {
                Score = 50,
                Reason = "Monto múltiplo exacto de 100"
            });
        }

        // Monto redondo (terminado en 00 o 50)
        var decimales = pago.Monto % 1;
        if (decimales == 0)
        {
            return Task.FromResult(new MatchRuleResult
            {
                Score = 30,
                Reason = "Monto es número entero"
            });
        }

        return Task.FromResult(new MatchRuleResult
        {
            Score = 0,
            Reason = "Monto no coincide con criterios"
        });
    }
}
