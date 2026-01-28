using Tlaoami.Domain.Entities;

namespace Tlaoami.Application.Rules;

public class MatchRuleResult
{
    public int Score { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool Matches => Score > 0;
}

public interface IMatchRule
{
    string Nombre { get; }
    
    Task<MatchRuleResult> EvaluarAsync(Pago pago);
}
