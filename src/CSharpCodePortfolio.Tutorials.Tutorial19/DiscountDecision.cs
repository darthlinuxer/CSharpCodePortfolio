namespace CSharpCodePortfolio.Tutorials.Tutorial19;

internal sealed record DiscountDecision(
    decimal DiscountRate,
    decimal DiscountAmount,
    decimal PayableAmount,
    string Reason)
{
    public static DiscountDecision None(decimal orderTotal, string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        return new DiscountDecision(0m, 0m, orderTotal, reason);
    }
}
