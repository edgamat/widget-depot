using Microsoft.Extensions.Configuration;

namespace WidgetDepot.ApiService.Features.Orders.Submit;

public class OrderFileWriter : IOrderFileWriter
{
    private readonly string _pickupDirectory;

    public OrderFileWriter(IConfiguration configuration)
    {
        _pickupDirectory = configuration["Orders:PickupDirectory"] ?? "";
    }

    public async Task WriteAsync(OrderFile orderFile, CancellationToken cancellationToken)
    {
        var filePath = Path.Combine(_pickupDirectory, orderFile.FileName);
        await File.WriteAllTextAsync(filePath, orderFile.GetContent(), cancellationToken);
    }
}