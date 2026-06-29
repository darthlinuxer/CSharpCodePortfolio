using System.Reflection;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Tests;

/// <summary>
/// Architecture tests that lock in the removal of <c>CreatedBy</c> and
/// <c>LastModifiedBy</c> audit actor fields from the abstract entity base.
/// The user decided that actor attribution belongs at the authentication
/// seam, not on the entity base; until an authentication context exists,
/// the aggregate only tracks timestamps (UTC-validated, time-provider-
/// driven) — not who performed the change.
/// </summary>
[TestClass]
public sealed class AbstractEntityAuditTests
{
    /// <summary>
    /// Proves that the AbstractEntity base no longer exposes a
    /// <c>CreatedBy</c> audit property. The aggregate's responsibility stops
    /// at "when" the change happened; "by whom" lives in the application
    /// seam once authentication lands.
    /// </summary>
    [TestMethod]
    public void AbstractEntity_DoesNotExposeCreatedBy()
    {
        var createdBy = typeof(AbstractEntity<Guid>).GetProperty(
            "CreatedBy",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        Assert.IsNull(createdBy);
    }

    /// <summary>
    /// Proves that the AbstractEntity base no longer exposes a
    /// <c>LastModifiedBy</c> audit property.
    /// </summary>
    [TestMethod]
    public void AbstractEntity_DoesNotExposeLastModifiedBy()
    {
        var lastModifiedBy = typeof(AbstractEntity<Guid>).GetProperty(
            "LastModifiedBy",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        Assert.IsNull(lastModifiedBy);
    }

    /// <summary>
    /// Proves that the IEntity interface no longer declares CreatedBy and
    /// LastModifiedBy members, so the contract itself commits to timestamp-
    /// only audit semantics.
    /// </summary>
    [TestMethod]
    public void IEntity_DoesNotDeclareActorAuditProperties()
    {
        var properties = typeof(IEntity).GetProperties().Select(p => p.Name).ToArray();

        CollectionAssert.AllItemsAreNotNull(properties);
        foreach (var name in properties)
        {
            Assert.IsFalse(name?.EndsWith("By", StringComparison.Ordinal) == true,
                $"IEntity must not declare an audit-actor property '{name}'.");
        }
    }
}