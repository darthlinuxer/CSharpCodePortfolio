namespace CSharpCodePortfolio.Tutorials.Tutorial17;

internal sealed record ZipEntrySnapshot(
    string Name,
    long RawSize,
    long CompressedSize,
    string Preview);
