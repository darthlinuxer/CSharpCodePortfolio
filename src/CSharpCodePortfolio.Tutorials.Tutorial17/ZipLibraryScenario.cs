using System.IO.Compression;

namespace CSharpCodePortfolio.Tutorials.Tutorial17;

internal static class ZipLibraryScenario
{
    private static readonly ZipDemoEntry[] DemoEntries =
    [
        new("clientes.json", """{"clientes":[{"id":1,"nome":"Ana"},{"id":2,"nome":"Bruno"}]}"""),
        new("pedidos/2026-06.json", """{"pedidos":[{"id":100,"clienteId":1,"total":250.75}]}""")
    ];

    public static ZipArchiveReport Run()
    {
        var workspace = Path.Combine(Path.GetTempPath(), $"csharp-code-portfolio-zip-{Guid.NewGuid():N}");

        try
        {
            var sourceDirectory = Path.Combine(workspace, "origem");
            var extractDirectory = Path.Combine(workspace, "extraido");
            var archivePath = Path.Combine(workspace, "portfolio.zip");

            WriteSourceFiles(sourceDirectory);
            CreateArchive(sourceDirectory, archivePath);
            var entries = InspectArchive(archivePath);
            ExtractArchive(archivePath, extractDirectory);

            var extractedFiles = Directory
                .EnumerateFiles(extractDirectory, "*.json", SearchOption.AllDirectories)
                .Select(path => Path.GetRelativePath(extractDirectory, path).Replace(Path.DirectorySeparatorChar, '/'))
                .Order(StringComparer.Ordinal)
                .ToArray();
            var archiveInfo = new FileInfo(archivePath);

            return new ZipArchiveReport(
                archiveInfo.Name,
                archiveInfo.Length,
                entries.Count,
                entries.Sum(static entry => entry.RawSize),
                entries.Sum(static entry => entry.CompressedSize),
                entries,
                extractedFiles);
        }
        finally
        {
            if (Directory.Exists(workspace))
            {
                Directory.Delete(workspace, recursive: true);
            }
        }
    }

    public static void CreateArchive(string sourceDirectory, string archivePath)
    {
        ZipFile.CreateFromDirectory(
            sourceDirectory,
            archivePath,
            CompressionLevel.Optimal,
            includeBaseDirectory: false);
    }

    public static IReadOnlyList<ZipEntrySnapshot> InspectArchive(string archivePath)
    {
        using var archive = ZipFile.OpenRead(archivePath);

        var entries = archive.Entries
            .Where(static entry => !string.IsNullOrEmpty(entry.Name))
            .Select(ReadSnapshot)
            .OrderBy(static entry => entry.Name, StringComparer.Ordinal)
            .ToArray();

        return entries;
    }

    public static void ExtractArchive(string archivePath, string targetDirectory)
    {
        Directory.CreateDirectory(targetDirectory);
        var targetRoot = Path.GetFullPath(targetDirectory) + Path.DirectorySeparatorChar;

        using var archive = ZipFile.OpenRead(archivePath);
        var fileEntries = archive.Entries.Where(static entry => !string.IsNullOrEmpty(entry.Name));

        foreach (var entry in fileEntries)
        {
            var destinationPath = Path.GetFullPath(
                Path.Combine(targetDirectory, entry.FullName));

            if (!destinationPath.StartsWith(targetRoot, StringComparison.Ordinal))
            {
                throw new IOException("Entrada ZIP fora do destino permitido.");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            entry.ExtractToFile(destinationPath, overwrite: true);
        }
    }

    private static void WriteSourceFiles(string sourceDirectory)
    {
        foreach (var entry in DemoEntries)
        {
            var path = Path.Combine(sourceDirectory, entry.Name);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, entry.Content);
        }
    }

    private static ZipEntrySnapshot ReadSnapshot(ZipArchiveEntry entry)
    {
        using var stream = entry.Open();
        using var reader = new StreamReader(stream);

        return new ZipEntrySnapshot(
            entry.FullName,
            entry.Length,
            entry.CompressedLength,
            NormalizePreview(reader.ReadToEnd()));
    }

    private static string NormalizePreview(string content)
    {
        return content
            .Replace("{", "{ ")
            .Replace("}", " }")
            .Replace("[", "[ ")
            .Replace("]", " ]")
            .Replace(",", ", ");
    }
}
