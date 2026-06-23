namespace CSharpCodePortfolio.Tutorials.Tutorial19;

internal sealed class CustomerDiscountPolicy
{
    private const decimal LoyalCustomerDiscountRate = 0.15m;

    public DiscountDecision Evaluate(CustomerDiscountRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.OrderTotal <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                request.OrderTotal,
                "O total do pedido deve ser maior que zero.");
        }

        if (!request.IsLoyalCustomer)
        {
            return DiscountDecision.None(request.OrderTotal, "Cliente sem desconto de fidelidade.");
        }

        var discountAmount = decimal.Round(
            request.OrderTotal * LoyalCustomerDiscountRate,
            2,
            MidpointRounding.AwayFromZero);

        return new DiscountDecision(
            LoyalCustomerDiscountRate,
            discountAmount,
            request.OrderTotal - discountAmount,
            "Cliente recorrente recebe desconto de fidelidade.");
    }
}
