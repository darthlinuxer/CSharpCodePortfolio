using System.Reflection;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Tests;

/// <summary>
/// Architecture tests that lock in the removal of the Document concept from
/// the UserAccount aggregate. Document-related errors were a TODO placeholder
/// for future PF (CPF) vs PJ (CNPJ) modelling and have no place in the
/// abstract registration aggregate today.
/// </summary>
[TestClass]
public sealed class UserAccountNoDocumentTests
{
    /// <summary>
    /// Proves that the UserAccount aggregate no longer exposes a Document
    /// property. The user has decided that PF vs PJ modelling is a separate
    /// tutorial and does not affect the abstract registration flow.
    /// </summary>
    [TestMethod]
    public void UserAccount_DoesNotExposeDocumentProperty()
    {
        var documentProperty = typeof(UserAccount)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(property => property.Name == "Document");

        Assert.IsNull(documentProperty,
            "UserAccount must not expose a Document property; PF vs PJ modelling is deferred to a dedicated tutorial.");
    }

    /// <summary>
    /// Proves that NormalizeDocument has been removed from the aggregate
    /// surface. The validation rule that used to live there is no longer
    /// applicable now that Document has been retired.
    /// </summary>
    [TestMethod]
    public void UserAccount_DoesNotExposeNormalizeDocumentMethod()
    {
        var normalizeMethod = typeof(UserAccount)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
            .FirstOrDefault(method => method.Name == "NormalizeDocument");

        Assert.IsNull(normalizeMethod,
            "UserAccount.NormalizeDocument must be retired alongside the Document property.");
    }

    /// <summary>
    /// Proves that the legacy document-related error types are gone, so the
    /// error catalog becomes smaller and tighter. New errors can only appear
    /// when the future PF/PJ bounded context lands.
    /// </summary>
    [TestMethod]
    public void DomainLayer_DoesNotExposeDocumentErrors()
    {
        var assembly = typeof(UserAccount).Assembly;

        var documentInvalid = assembly.GetType(
            "CSharpCodePortfolio.Tutorials.Tutorial30.Domain.UserAccountDocumentInvalidError");
        var documentDuplicate = assembly.GetType(
            "CSharpCodePortfolio.Tutorials.Tutorial30.Domain.UserAccountDocumentDuplicateError");

        Assert.IsNull(documentInvalid,
            "UserAccountDocumentInvalidError must be retired alongside Document.");
        Assert.IsNull(documentDuplicate,
            "UserAccountDocumentDuplicateError must be retired alongside Document.");
    }

    /// <summary>
    /// Proves that EnsureCanBeRegistered now takes only the email existence
    /// flag (a single boolean), reflecting that Document uniqueness is no
    /// longer the aggregate's concern.
    /// </summary>
    [TestMethod]
    public void EnsureCanBeRegistered_HasSingleBooleanParameter()
    {
        var method = typeof(UserAccount)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .First(m => m.Name == "EnsureCanBeRegistered");

        var parameters = method.GetParameters();
        Assert.HasCount(1, parameters);
        Assert.AreEqual(typeof(bool), parameters[0].ParameterType,
            "EnsureCanBeRegistered must now accept a single bool — email existence — after Document retirement.");
        Assert.AreEqual("emailExists", parameters[0].Name,
            "The single bool parameter must be named after the only concept that remains: email uniqueness.");
    }

    /// <summary>
    /// Proves that the static factory no longer accepts a raw document
    /// string: PersonName, Email, and an optional phone are the only
    /// inputs the aggregate understands.
    /// </summary>
    [TestMethod]
    public void UserAccountCreate_DoesNotAcceptDocumentParameter()
    {
        var create = typeof(UserAccount)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == "Create");

        var rawNames = create.GetParameters().Select(p => p.Name).ToArray();
        CollectionAssert.AllItemsAreNotNull(rawNames);
        foreach (var name in rawNames)
        {
            Assert.IsFalse(name?.Contains("document", StringComparison.OrdinalIgnoreCase) == true,
                $"UserAccount.Create must not expose a 'document' parameter (found '{name}').");
        }
    }
}