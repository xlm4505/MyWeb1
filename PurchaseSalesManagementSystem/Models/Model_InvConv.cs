namespace PurchaseSalesManagementSystem.Models;

public sealed class Model_InvConv
{
    public List<string> Logs { get; } = [];
    public List<string> Warnings { get; } = [];
    public List<string> Errors { get; } = [];
    public List<GeneratedSummaryFile> Files { get; } = [];

    public int TotalInvoices { get; set; }
    public int ConCount { get; set; }
    public int RinkuCount { get; set; }
    public int ConFlowCount { get; set; }
    public int RinkuFlowCount { get; set; }

    public bool HasError => Errors.Count > 0;
}

public sealed class GeneratedSummaryFile
{
    public required string FileName { get; init; }
    public required byte[] Content { get; init; }
}