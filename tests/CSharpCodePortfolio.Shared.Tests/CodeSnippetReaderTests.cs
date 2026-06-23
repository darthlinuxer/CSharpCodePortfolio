using CSharpCodePortfolio.Shared;

namespace CSharpCodePortfolio.Shared.Tests;

[TestClass]
public sealed class CodeSnippetReaderTests
{
    [TestMethod]
    public void ReadFile_WithRelativePath_ReturnsWholeFile()
    {
        var snippet = CodeSnippetReader.ReadFile("src/CSharpCodePortfolio.Shared/TutorialConclusionKind.cs");

        Assert.AreEqual("src/CSharpCodePortfolio.Shared/TutorialConclusionKind.cs", snippet.FileName);
        Assert.Contains("public enum TutorialConclusionKind", snippet.Code);
    }

    [TestMethod]
    public void ReadType_WithType_ReturnsWholeType()
    {
        var snippet = CodeSnippetReader.ReadType(typeof(SnippetSubject));

        Assert.Contains("internal sealed class SnippetSubject", snippet.Code);
        Assert.Contains("public string Name", snippet.Code);
        Assert.Contains("public string Describe()", snippet.Code);
    }

    [TestMethod]
    public void ReadMembers_WithConstructorPropertyAndMethod_ReturnsWrappedMembers()
    {
        var snippet = CodeSnippetReader.ReadMembers(
            typeof(SnippetSubject),
            nameof(SnippetSubject),
            nameof(SnippetSubject.Name),
            nameof(SnippetSubject.Describe));

        Assert.Contains("CodeSnippetReaderTests.cs | SnippetSubject", snippet.FileName);
        Assert.Contains("class SnippetSubject", snippet.Code);
        Assert.Contains("SnippetSubject(string name)", snippet.Code);
        Assert.Contains("public string Name", snippet.Code);
        Assert.Contains("public string Describe()", snippet.Code);
        Assert.Contains("return $\"Subject: {Name}\";", snippet.Code);
    }

    [TestMethod]
    public void ReadFile_WithMissingFile_ThrowsFileNotFound()
    {
        var exception = Assert.ThrowsExactly<FileNotFoundException>(
            () => CodeSnippetReader.ReadFile("src/CSharpCodePortfolio.Shared/DoesNotExist.cs"));

        Assert.Contains("DoesNotExist.cs", exception.Message);
    }

    [TestMethod]
    public void ReadMembers_WithMissingMember_ThrowsInvalidOperation()
    {
        var exception = Assert.ThrowsExactly<InvalidOperationException>(
            () => CodeSnippetReader.ReadMembers(typeof(SnippetSubject), "MissingMember"));

        Assert.Contains("MissingMember", exception.Message);
    }
}

internal sealed class SnippetSubject
{
    public SnippetSubject(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public string Describe()
    {
        return $"Subject: {Name}";
    }
}
