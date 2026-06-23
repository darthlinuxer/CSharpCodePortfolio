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
    public void ReadMemberExcerpts_WithValidRange_ReturnsSelectedLinesAndCaption()
    {
        var snippets = CodeSnippetReader.ReadMemberExcerpts(
            typeof(SnippetSubject),
            nameof(SnippetSubject.Describe),
            new CodeExcerpt(3, 3, "Retorno"));

        Assert.HasCount(1, snippets);
        var snippet = snippets[0];

        Assert.AreEqual("Retorno", snippet.Caption);
        Assert.Contains("CodeSnippetReaderTests.cs | SnippetSubject.Describe | linhas 3-3", snippet.FileName);
        Assert.Contains("return $\"Subject: {Name}\";", snippet.Code);
        Assert.DoesNotContain("public string Describe()", snippet.Code);
    }

    [TestMethod]
    public void ReadMemberExcerpts_WithRangePastMemberEnd_ThrowsArgumentOutOfRange()
    {
        var exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            () => CodeSnippetReader.ReadMemberExcerpts(
                typeof(SnippetSubject),
                nameof(SnippetSubject.Describe),
                new CodeExcerpt(20, 21)));

        Assert.Contains("Describe", exception.Message);
        Assert.Contains("20-21", exception.Message);
    }

    [TestMethod]
    public void CodeExcerpt_WithInvalidRange_ThrowsArgumentOutOfRange()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new CodeExcerpt(0, 1));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new CodeExcerpt(2, 1));
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
