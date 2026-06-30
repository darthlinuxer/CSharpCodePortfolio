using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders.ValueObjects;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Ordering.Domain.Aggregates.Orders;

/// <summary>
/// Entity owned by the Order aggregate.
/// </summary>
public sealed class OrderLine
{
    private OrderLine()
    {
    }

    private OrderLine(OrderLineDraft draft)
    {
        Id = Guid.CreateVersion7();
        Sku = draft.Sku;
        Quantity = draft.Quantity;
        UnitPrice = draft.UnitPrice;
    }

    public Guid Id { get; private set; }

    public Sku Sku { get; private set; }

    public Quantity Quantity { get; private set; }

    public Money UnitPrice { get; private set; }

    public Money LineTotal => new(UnitPrice.Value * Quantity.Value);

    internal static OrderLine Create(OrderLineDraft draft) => new(draft);
}
