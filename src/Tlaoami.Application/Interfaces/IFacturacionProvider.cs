using System.Threading;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos.Facturacion;

namespace Tlaoami.Application.Facturacion;

public interface IFacturacionProvider
{
    Task<EmitirCfdiResult> EmitirAsync(EmitirCfdiRequest request, CancellationToken cancellationToken = default);
}
