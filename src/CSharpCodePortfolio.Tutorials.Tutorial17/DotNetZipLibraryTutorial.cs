using CSharpCodePortfolio.Shared;
using CSharpCodePortfolio.Tutorials.Abstractions;

namespace CSharpCodePortfolio.Tutorials.Tutorial17;

[Tutorial("17", "dotnet-zip-library", "Arquivos ZIP com System.IO.Compression")]
public sealed class DotNetZipLibraryTutorial : ITutorial
{
    public Task RunAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        TutorialConsole.WriteHeader("17", "Arquivos ZIP com System.IO.Compression");
        TutorialConsole.WriteContext(
            ("Tema", "Compactação de arquivos"),
            ("Conceito", "Criar, inspecionar e extrair arquivos ZIP com APIs nativas do .NET"),
            ("Runtime", ".NET 10"),
            ("Slug", "dotnet-zip-library"));
        TutorialConsole.WriteQuestion("Como compactar arquivos, ler metadados e extrair o conteúdo de um ZIP?");
        TutorialConsole.WriteHypothesis(
            "`ZipFile.CreateFromDirectory` cria o arquivo ZIP a partir de uma pasta.",
            "`ZipArchive` permite inspecionar entradas, tamanhos e conteúdo sem extrair tudo.",
            "A extração valida o caminho final de cada entrada antes de gravar arquivos.");
        TutorialConsole.WritePreparation(
            "O cenário cria arquivos JSON temporários em uma pasta isolada.",
            "A compactação usa `CompressionLevel.Optimal`, sem dependência externa.",
            "A extração grava os arquivos em outra pasta temporária e valida o resultado.");

        TutorialConsole.WriteExperiment(
            1,
            "Criação do arquivo ZIP",
            "Compacta uma pasta de entrada com arquivos JSON.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: `ZipFile.CreateFromDirectory` recebe diretório de origem, destino e nível de compressão.",
            "ZipLibraryScenario.cs",
            """
            ZipFile.CreateFromDirectory(
                sourceDirectory,
                archivePath,
                CompressionLevel.Optimal,
                includeBaseDirectory: false);
            """);

        TutorialConsole.WriteExperiment(
            2,
            "Inspeção das entradas",
            "Lê nome, tamanho original, tamanho compactado e prévia do conteúdo.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: `ZipArchive` abre o ZIP em modo leitura e percorre `Entries`.",
            "ZipLibraryScenario.cs",
            """
            using var archive = ZipFile.OpenRead(archivePath);

            var entries = archive.Entries
                .Where(static entry => !string.IsNullOrEmpty(entry.Name))
                .Select(ReadSnapshot)
                .OrderBy(static entry => entry.Name, StringComparer.Ordinal)
                .ToArray();
            """);

        TutorialConsole.WriteExperiment(
            3,
            "Extração com validação de caminho",
            "Extrai cada entrada somente quando o destino permanece dentro da pasta permitida.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: cada caminho final é normalizado antes de `ExtractToFile`.",
            "ZipLibraryScenario.cs",
            """
            var destinationPath = Path.GetFullPath(
                Path.Combine(targetDirectory, entry.FullName));

            if (!destinationPath.StartsWith(targetRoot, StringComparison.Ordinal))
            {
                throw new IOException("Entrada ZIP fora do destino permitido.");
            }

            entry.ExtractToFile(destinationPath, overwrite: true);
            """);

        var report = ZipLibraryScenario.Run();
        VerifyReport(report);

        TutorialConsole.WriteEvidence(
            "Arquivo ZIP",
            ("Arquivo", report.ArchiveName),
            ("Tamanho do ZIP", $"{report.ArchiveSize} bytes"),
            ("Entradas", report.EntryCount.ToString()),
            ("Soma original", $"{report.RawTotal} bytes"),
            ("Soma compactada", $"{report.CompressedTotal} bytes"));

        foreach (var entry in report.Entries)
        {
            TutorialConsole.WriteEvidence(
                $"Entrada {entry.Name}",
                ("Tamanho original", $"{entry.RawSize} bytes"),
                ("Tamanho compactado", $"{entry.CompressedSize} bytes"),
                ("Prévia", entry.Preview));
        }

        TutorialConsole.WriteEvidence(
            "Extração",
            ("Arquivos extraídos", string.Join(", ", report.ExtractedFiles)));
        TutorialConsole.WriteObservation(
            "Para arquivos recebidos de fora do processo, valide o caminho de destino antes de extrair cada entrada.");
        TutorialConsole.WriteConclusion(
            "`System.IO.Compression` cobre o fluxo principal de ZIP no .NET: compactar diretórios, inspecionar entradas e extrair arquivos com controle de destino.",
            TutorialConclusionKind.Success);

        return Task.CompletedTask;
    }

    private static void VerifyReport(ZipArchiveReport report)
    {
        if (report.EntryCount != 2)
        {
            throw new InvalidOperationException("O ZIP deve conter dois arquivos JSON.");
        }

        if (report.ArchiveSize <= 0 || report.RawTotal <= 0)
        {
            throw new InvalidOperationException("O ZIP deve registrar tamanhos maiores que zero.");
        }

        if (report.ExtractedFiles.Count != 2)
        {
            throw new InvalidOperationException("A extração deve restaurar os dois arquivos.");
        }
    }
}
