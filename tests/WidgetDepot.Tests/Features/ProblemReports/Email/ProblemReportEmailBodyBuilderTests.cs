using Shouldly;

using WidgetDepot.ApiService.Features.ProblemReports.Email;

namespace WidgetDepot.Tests.Features.ProblemReports.Email;

public class ProblemReportEmailBodyBuilderTests
{
    private static ProblemReportEmailMessage BuildMessage(
        int orderId = 42,
        IReadOnlyList<ProblemReportEmailItem>? items = null,
        string? notes = null) =>
        new(orderId, items ?? [], notes);

    [Fact]
    public void Build_IncludesOrderId_InBody()
    {
        var message = BuildMessage(orderId: 99);

        var body = ProblemReportEmailBodyBuilder.Build(message);

        body.ShouldContain("99");
    }

    [Fact]
    public void Build_IncludesWidgetNameAndIssueType_ForEachItem()
    {
        var items = new List<ProblemReportEmailItem>
        {
            new("Sprocket", "Damaged"),
            new("Cog", "UnderRequested")
        };
        var message = BuildMessage(items: items);

        var body = ProblemReportEmailBodyBuilder.Build(message);

        body.ShouldContain("Sprocket");
        body.ShouldContain("Damaged");
        body.ShouldContain("Cog");
        body.ShouldContain("UnderRequested");
    }

    [Fact]
    public void Build_IncludesNotes_WhenNotesPresent()
    {
        var message = BuildMessage(notes: "Arrived in poor condition");

        var body = ProblemReportEmailBodyBuilder.Build(message);

        body.ShouldContain("Arrived in poor condition");
    }

    [Fact]
    public void Build_ExcludesNotesSection_WhenNotesIsNull()
    {
        var message = BuildMessage(notes: null);

        var body = ProblemReportEmailBodyBuilder.Build(message);

        body.ShouldNotContain("Notes:");
    }
}