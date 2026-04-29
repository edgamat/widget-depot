using WidgetDepot.Web.Features.Orders.Create.Step1;
using WidgetDepot.Web.Features.Orders.Create.Step2;

namespace WidgetDepot.Web.Features.Orders.Create;

public class OrderWizardState
{
    public int OrderId { get; set; }
    public List<OrderItemModel> SelectedItems { get; set; } = [];
    public Step2FormModel AddressData { get; set; } = new();
}