using System.Text.RegularExpressions;

using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;

namespace WidgetDepot.ApiService.Features.Orders.SaveAddresses;

public record AddressRequest(
    string RecipientName,
    string StreetLine1,
    string? StreetLine2,
    string City,
    string State,
    string ZipCode);

public record SaveAddressesRequest(AddressRequest ShippingAddress, AddressRequest BillingAddress);

public abstract record SaveAddressesError
{
    public record OrderNotFound : SaveAddressesError;
    public record Forbidden : SaveAddressesError;
    public record InvalidRequest(Dictionary<string, string[]> Errors) : SaveAddressesError;
}

public class SaveAddressesHandler(AppDbContext db)
{
    private static readonly Regex ZipCodeRegex = new(@"^\d{5}(-\d{4})?$", RegexOptions.Compiled);

    public async Task<object?> HandleAsync(int orderId, int customerId, SaveAddressesRequest request, CancellationToken cancellationToken)
    {
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order is null)
            return new SaveAddressesError.OrderNotFound();

        if (order.CustomerId != customerId)
            return new SaveAddressesError.Forbidden();

        var errors = Validate(request);
        if (errors.Count > 0)
            return new SaveAddressesError.InvalidRequest(errors);

        order.ShippingAddress = MapAddress(request.ShippingAddress);
        order.BillingAddress = MapAddress(request.BillingAddress);

        await db.SaveChangesAsync(cancellationToken);

        return null;
    }

    private static Dictionary<string, string[]> Validate(SaveAddressesRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        ValidateAddress(request.ShippingAddress, "shippingAddress", errors);
        ValidateAddress(request.BillingAddress, "billingAddress", errors);

        return errors;
    }

    private static void ValidateAddress(AddressRequest address, string prefix, Dictionary<string, string[]> errors)
    {
        if (address.RecipientName.Length > 100)
            errors[$"{prefix}.recipientName"] = ["Recipient name must not exceed 100 characters."];

        if (address.StreetLine1.Length > 100)
            errors[$"{prefix}.streetLine1"] = ["Street line 1 must not exceed 100 characters."];

        if (address.StreetLine2 is not null && address.StreetLine2.Length > 100)
            errors[$"{prefix}.streetLine2"] = ["Street line 2 must not exceed 100 characters."];

        if (address.City.Length > 100)
            errors[$"{prefix}.city"] = ["City must not exceed 100 characters."];

        if (!ZipCodeRegex.IsMatch(address.ZipCode))
            errors[$"{prefix}.zipCode"] = ["ZIP code must be in 5-digit (12345) or ZIP+4 (12345-6789) format."];
    }

    private static Address MapAddress(AddressRequest request)
    {
        return new Address
        {
            RecipientName = request.RecipientName,
            StreetLine1 = request.StreetLine1,
            StreetLine2 = request.StreetLine2,
            City = request.City,
            State = request.State,
            ZipCode = request.ZipCode
        };
    }
}