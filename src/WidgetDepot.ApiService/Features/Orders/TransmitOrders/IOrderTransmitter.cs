namespace WidgetDepot.ApiService.Features.Orders.TransmitOrders;

public interface IOrderTransmitter
{
    Task<bool> TransmitAsync(string localFilePath, string fileName, CancellationToken cancellationToken);
}