using CSharpCodePortfolio.Tutorials.Tutorial19;

namespace CSharpCodePortfolio.Tutorials.Tutorial19.Tests;

[TestClass]
public sealed class CustomerDiscountPolicyTests
{
    [TestMethod]
    public void Evaluate_WithLoyalCustomer_ReturnsFifteenPercentDiscount()
    {
        var policy = new CustomerDiscountPolicy();
        var request = new CustomerDiscountRequest(200m, IsLoyalCustomer: true);

        var decision = policy.Evaluate(request);

        Assert.AreEqual(0.15m, decision.DiscountRate);
        Assert.AreEqual(30m, decision.DiscountAmount);
        Assert.AreEqual(170m, decision.PayableAmount);
    }

    [TestMethod]
    public void Evaluate_WithRegularCustomer_DoesNotApplyDiscount()
    {
        var policy = new CustomerDiscountPolicy();
        var request = new CustomerDiscountRequest(200m, IsLoyalCustomer: false);

        var decision = policy.Evaluate(request);

        Assert.AreEqual(0m, decision.DiscountRate);
        Assert.AreEqual(0m, decision.DiscountAmount);
        Assert.AreEqual(200m, decision.PayableAmount);
    }
}
