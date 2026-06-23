namespace CSharpCodePortfolio.Shared;

/// <summary>
/// Source code ready to be rendered in a tutorial.
/// </summary>
public sealed record CodeSnippet(string FileName, string Code, string? Caption = null);

/// <summary>
/// A 1-based inclusive line range inside a source member.
/// </summary>
public readonly record struct CodeExcerpt
{
    public CodeExcerpt(int startLine, int endLine, string? caption = null)
    {
        if (startLine <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(startLine), startLine, "Start line must be greater than zero.");
        }

        if (endLine < startLine)
        {
            throw new ArgumentOutOfRangeException(nameof(endLine), endLine, "End line must be greater than or equal to start line.");
        }

        StartLine = startLine;
        EndLine = endLine;
        Caption = caption;
    }

    public int StartLine { get; }

    public int EndLine { get; }

    public string? Caption { get; }
}
