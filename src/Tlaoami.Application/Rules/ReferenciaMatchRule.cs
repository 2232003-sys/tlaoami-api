using Tlaoami.Domain.Entities;

namespace Tlaoami.Application.Rules;

public class ReferenciaMatchRule : IMatchRule
{
    public string Nombre => "Coincidencia de Referencia";

    public Task<MatchRuleResult> EvaluarAsync(Pago pago)
    {
        // Si no hay referencia, no hay score
        if (string.IsNullOrWhiteSpace(pago.IdempotencyKey))
        {
            return Task.FromResult(new MatchRuleResult
            {
                Score = 0,
                Reason = "Sin referencia disponible"
            });
        }

        // Referencia contiene patrón típico de alumno (UUID inicio)
        var ref_lower = pago.IdempotencyKey.ToLowerInvariant();
        
        // UUID típico tiene formato: "xxxxxxxx-xxxx-xxxx..."
        if (ref_lower.Length >= 8 && ref_lower.Contains("-"))
        {
            return Task.FromResult(new MatchRuleResult
            {
                Score = 20,
                Reason = "Referencia contiene formato UUID"
            });
        }

        // Si tiene números/letras de forma consistente
        if (ref_lower.Length >= 5)
        {
            return Task.FromResult(new MatchRuleResult
            {
                Score = 10,
                Reason = "Referencia tiene estructura válida"
            });
        }

        return Task.FromResult(new MatchRuleResult
        {
            Score = 0,
            Reason = "Referencia no válida"
        });
    }
}
