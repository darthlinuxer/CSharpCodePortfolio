using CSharpCodePortfolio.Tutorials.Tutorial30.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Tests;

/// <summary>
/// Architecture tests that lock in DomainError.Category as the sole mechanism
/// the presentation layer uses to map a domain failure to an HTTP response.
/// Presentation must never pattern-match concrete domain error types.
/// </summary>
[TestClass]
public sealed class DomainErrorCategoryTests
{
    /// <summary>
    /// Proves that DomainError exposes an abstract Category property every
    /// concrete error must override. Without this, the presentation has to
    /// fall back to type-pattern matching, which violates OCP.
    /// </summary>
    [TestMethod]
    public void DomainError_ExposesAbstractCategory()
    {
        var categoryProperty = typeof(DomainError).GetProperty(nameof(DomainError.Category));

        Assert.IsNotNull(categoryProperty, "DomainError must expose a Category property.");
        Assert.IsTrue(categoryProperty!.GetMethod!.IsAbstract,
            "DomainError.Category must be abstract so concrete errors must override it.");
    }

    /// <summary>
    /// Proves that DomainErrorCategory is a value-type wrapper around a
    /// stable string code, so HTTP mapping uses switch-by-category, not
    /// magic strings.
    /// </summary>
    [TestMethod]
    public void DomainErrorCategory_IsValueType()
    {
        Assert.IsTrue(typeof(DomainErrorCategory).IsValueType);
    }

    /// <summary>
    /// Proves that the canonical validation errors carry the Validation
    /// category, not a magic string comparison. This is the data that
    /// drives the presentation HTTP table.
    /// </summary>
    [TestMethod]
    public void ValidationErrors_CarryValidationCategory()
    {
        Assert.AreEqual(DomainErrorCategory.Validation, new PersonNameRequiredError().Category);
        Assert.AreEqual(DomainErrorCategory.Validation, new EmailInvalidError().Category);
        Assert.AreEqual(DomainErrorCategory.Validation, new PhoneNumberInvalidError().Category);
        Assert.AreEqual(DomainErrorCategory.Validation, new TimestampUtcRequiredError().Category);
    }

    /// <summary>
    /// Proves that the email uniqueness error carries the Conflict category
    /// so the presentation can map to HTTP 409 without knowing the concrete
    /// error type.
    /// </summary>
    [TestMethod]
    public void EmailDuplicateError_CarriesConflictCategory()
    {
        Assert.AreEqual(DomainErrorCategory.Conflict, new UserAccountEmailDuplicateError().Category);
    }

    /// <summary>
    /// Proves that the DomainError base is the only place where Category
    /// is declared — concrete errors must NOT introduce a new Category
    /// outside the canonical set, otherwise the presentation table would
    /// silently fall back to "unknown" status.
    /// </summary>
    [TestMethod]
    public void ConcreteErrors_UseCanonicalCategoryInstances()
    {
        var assembly = typeof(DomainError).Assembly;
        var concreteErrors = assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(DomainError)) && !t.IsAbstract)
            .ToArray();

        Assert.IsNotEmpty(concreteErrors);

        var canonicalNames = new HashSet<string>
        {
            nameof(DomainErrorCategory.Validation),
            nameof(DomainErrorCategory.Conflict),
        };

        foreach (var error in concreteErrors)
        {
            var instance = (DomainError)Activator.CreateInstance(error)!;
            Assert.IsTrue(canonicalNames.Contains(instance.Category.Name),
                $"Concrete error {error.FullName} must use a canonical DomainErrorCategory.");
        }
    }
}