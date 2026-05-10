namespace WidgetDepot.ApiService.Features.Orders.Submit;

public interface IOrderFileWriter
{
    Task WriteAsync(OrderFile orderFile, CancellationToken cancellationToken);
}