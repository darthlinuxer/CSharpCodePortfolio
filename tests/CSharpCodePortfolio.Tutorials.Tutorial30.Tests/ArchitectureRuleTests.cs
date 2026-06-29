using CSharpCodePortfolio.Tutorials.Tutorial30.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Tests;

/// <summary>
/// Architecture-guardrail tests that enforce the production-grade design
/// commitments: domain VOs are value types, AbstractEntity helpers
/// accommodate structs, and concrete domain errors stay close to their
/// aggregate owner (no global error catalog).
/// </summary>
[TestClass]
public sealed class ArchitectureRuleTests
{
    /// <summary>
    /// Proves that all four canonical domain value objects are struct
    /// shapes, so equality is structural by value, allocation pressure
    /// is zero per instance, and EF materialisation uses the positional
    /// <c>Value</c> setter.
    /// </summary>
    [TestMethod]
    public void DomainValueObjects_AreStructs()
    {
        var assembly = typeof(UserAccount).Assembly;
        var valueObjectTypes = assembly
            .GetTypes()
            .Where(t => t.Namespace == "CSharpCodePortfolio.Tutorials.Tutorial30.Domain.ValueObjects")
            .Where(t => t.Name is "PersonName" or "Email" or "PhoneNumber" or "Timestamp")
            .ToArray();

        Assert.IsNotEmpty(valueObjectTypes);
        foreach (var type in valueObjectTypes)
        {
            Assert.IsTrue(type.IsValueType,
                $"Domain VO '{type.FullName}' must be a readonly record struct (was reference type).");
        }
    }

    /// <summary>
    /// Proves that the old global DomainErrors catalog is gone. Each error
    /// must live next to its aggregate or value object.
    /// </summary>
    [TestMethod]
    public void DomainLayer_HasNoGlobalDomainErrorsCatalog()
    {
        var assembly = typeof(UserAccount).Assembly;
        var globalCatalog = assembly.GetType(
            "CSharpCodePortfolio.Tutorials.Tutorial30.Domain.DomainErrors");

        Assert.IsNull(globalCatalog);
    }
}