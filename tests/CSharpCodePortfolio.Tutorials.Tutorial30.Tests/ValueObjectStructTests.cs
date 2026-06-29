using CSharpCodePortfolio.Tutorials.Tutorial30.Domain;
using LanguageExt;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Tests;

/// <summary>
/// Architecture tests that lock in the value-object-as-struct design choice:
/// PersonName, Email, PhoneNumber, Timestamp must be readonly record structs
/// so that absence semantics, equality by value, and absence of boxing apply
/// uniformly across the domain layer.
/// </summary>
[TestClass]
public sealed class ValueObjectStructTests
{
    /// <summary>
    /// Proves that PersonName is a value type. Reference types are forbidden
    /// to avoid allocation pressure and to keep equality structural.
    /// </summary>
    [TestMethod]
    public void PersonName_IsStruct()
    {
        Assert.IsTrue(typeof(PersonName).IsValueType);
    }

    /// <summary>
    /// Proves that Email is a value type so that normalization + equality
    /// by value is a structural contract.
    /// </summary>
    [TestMethod]
    public void Email_IsStruct()
    {
        Assert.IsTrue(typeof(Email).IsValueType);
    }

    /// <summary>
    /// Proves that PhoneNumber is a value type so that Option&lt;PhoneNumber&gt;
    /// uses no extra reference overhead.
    /// </summary>
    [TestMethod]
    public void PhoneNumber_IsStruct()
    {
        Assert.IsTrue(typeof(PhoneNumber).IsValueType);
    }

    /// <summary>
    /// Proves that Timestamp is a value type so it can act as a key
    /// in comparisons and accumulators without boxing.
    /// </summary>
    [TestMethod]
    public void Timestamp_IsStruct()
    {
        Assert.IsTrue(typeof(Timestamp).IsValueType);
    }

    /// <summary>
    /// Proves structural equality: two PersonName values created from the
    /// same trimmed string are equal regardless of construction site.
    /// </summary>
    [TestMethod]
    public void PersonName_Equals_IsStructural()
    {
        var a = PersonName.Create("Ada").IfRight(x => x, _ => default);
        var b = PersonName.Create("Ada").IfRight(x => x, _ => default);

        Assert.IsTrue(a.Equals(b));
        Assert.IsTrue(a == b);
        Assert.IsFalse(a.Equals(default));
    }

    /// <summary>
    /// Proves that Email.Create normalises casing so two differently-cased
    /// inputs are equal after normalization.
    /// </summary>
    [TestMethod]
    public void Email_Equals_IsCaseInsensitiveAfterNormalize()
    {
        var lower = Email.Create("ada@example.com").IfRight(x => x, _ => default);
        var upper = Email.Create("ADA@EXAMPLE.COM").IfRight(x => x, _ => default);

        Assert.IsTrue(lower.Equals(upper));
    }

    /// <summary>
    /// Proves that empty/whitespace input yields Option.None, the absence
    /// semantic of optional phone numbers.
    /// </summary>
    [TestMethod]
    public void PhoneNumber_CreateOptional_NoneForEmpty()
    {
        var result = PhoneNumber.CreateOptional(null);
        var opt = result.Match(Right: o => o, Left: _ => Option<PhoneNumber>.None);

        Assert.IsTrue(opt.IsNone);
    }

    /// <summary>
    /// Proves that valid digit input yields Some(PhoneNumber).
    /// </summary>
    [TestMethod]
    public void PhoneNumber_CreateOptional_SomeForValid()
    {
        var result = PhoneNumber.CreateOptional("11999998888");
        var opt = result.Match(Right: o => o, Left: _ => Option<PhoneNumber>.None);

        Assert.IsTrue(opt.IsSome);
    }

    /// <summary>
    /// Proves that PersonName.Create trims surrounding whitespace.
    /// </summary>
    [TestMethod]
    public void PersonName_Create_Trims()
    {
        var name = PersonName.Create("  Ada  ").IfRight(x => x, _ => default);

        Assert.AreEqual("Ada", name.Value);
    }

    /// <summary>
    /// Proves that AbstractEntity helpers (ToOption / ToNullable) accept the
    /// new struct-shaped value objects by constraining T : struct (no
    /// leftover `where T : class`).
    /// </summary>
    [TestMethod]
    public void AbstractEntity_StructConstrainedHelpers()
    {
        var helpers = typeof(AbstractEntity<Guid>)
            .GetMethods(System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Static
                        | System.Reflection.BindingFlags.Instance)
            .Where(m => m.Name is "ToOption" or "ToNullable")
            .Where(m => m.IsGenericMethod)
            .ToArray();

        Assert.IsNotEmpty(helpers);

        foreach (var helper in helpers)
        {
            var generic = helper.GetGenericArguments().First();
            var constraints = generic.GetGenericParameterConstraints();

            Assert.IsTrue(constraints.Any(c => c.IsValueType),
                $"Helper {helper.Name} must constrain T : struct (was: {string.Join(", ", constraints.Select(c => c.Name))}).");
        }
    }
}