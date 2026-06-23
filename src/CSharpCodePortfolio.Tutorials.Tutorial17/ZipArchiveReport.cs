namespace CSharpCodePortfolio.Tutorials.Tutorial17;

internal sealed record ZipArchiveReport(
    string ArchiveName,
    long ArchiveSize,
    int EntryCount,
    long RawTotal,
    long CompressedTotal,
    IReadOnlyList<ZipEntrySnapshot> Entries,
    IReadOnlyList<string> ExtractedFiles);
