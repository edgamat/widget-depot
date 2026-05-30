using Microsoft.Extensions.Options;

namespace WidgetDepot.ApiService.Features.Orders.Submit;

public class OrderFileWriter : IOrderFileWriter
{
    private readonly string _pickupDirectory;

    public OrderFileWriter(IOptions<OrdersOptions> options)
    {
        _pickupDirectory = options.Value.PickupDirectory;
    }

    public async Task WriteAsync(OrderFile orderFile, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(_pickupDirectory))
        {
            Directory.CreateDirectory(_pickupDirectory);
        }

        var filePath = Path.Combine(_pickupDirectory, orderFile.FileName);
        await File.WriteAllTextAsync(filePath, orderFile.GetContent(), cancellationToken);
    }
}