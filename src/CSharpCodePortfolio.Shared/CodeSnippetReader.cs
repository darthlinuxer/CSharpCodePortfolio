using System.Text;
using System.Text.RegularExpressions;

namespace CSharpCodePortfolio.Shared;

/// <summary>
/// Reads source snippets from the repository for tutorial output.
/// </summary>
public static class CodeSnippetReader
{
    private const string RootSolutionFile = "CSharpCodePortfolio.slnx";

    /// <summary>
    /// Reads an entire file relative to the repository root.
    /// </summary>
    public static CodeSnippet ReadFile(string relativePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);

        var repositoryRoot = FindRepositoryRoot();
        var fullPath = Path.GetFullPath(Path.Combine(repositoryRoot, relativePath));
        var pathFromRoot = Path.GetRelativePath(repositoryRoot, fullPath);

        if (pathFromRoot.StartsWith("..", StringComparison.Ordinal) || Path.IsPathRooted(pathFromRoot))
        {
            throw new InvalidOperationException($"Snippet file '{relativePath}' must be inside the repository.");
        }

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Snippet file '{relativePath}' was not found.", fullPath);
        }

        return new CodeSnippet(ToRelativePath(repositoryRoot, fullPath), File.ReadAllText(fullPath));
    }

    /// <summary>
    /// Reads the whole source block for a type.
    /// </summary>
    public static CodeSnippet ReadType(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var source = FindTypeSource(type);
        var block = ExtractTypeBlock(source.Code, type.Name);
        return new CodeSnippet(source.FileName, block.Code);
    }

    /// <summary>
    /// Reads selected members from a type, wrapped in the containing type declaration.
    /// </summary>
    public static CodeSnippet ReadMembers(Type type, params string[] memberNames)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(memberNames);

        if (memberNames.Length == 0)
        {
            return ReadType(type);
        }

        var source = FindTypeSource(type);
        var block = ExtractTypeBlock(source.Code, type.Name);
        var members = memberNames
            .Where(static memberName => !string.IsNullOrWhiteSpace(memberName))
            .Select(memberName => ExtractMember(block.Code, type.Name, memberName))
            .ToArray();

        if (members.Length == 0)
        {
            throw new ArgumentException("At least one member name must be provided.", nameof(memberNames));
        }

        var code = new StringBuilder();
        code.AppendLine(block.Declaration.TrimEnd());
        code.AppendLine("{");

        foreach (var member in members)
        {
            code.AppendLine();
            code.AppendLine(Reindent(member, 4));
        }

        code.AppendLine("}");

        return new CodeSnippet($"{source.FileName} | {type.Name}", code.ToString());
    }

    /// <summary>
    /// Reads selected line ranges from a single member.
    /// </summary>
    public static IReadOnlyList<CodeSnippet> ReadMemberExcerpts(Type type, string memberName, params CodeExcerpt[] excerpts)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(memberName);
        ArgumentNullException.ThrowIfNull(excerpts);

        if (excerpts.Length == 0)
        {
            return [ReadMembers(type, memberName)];
        }

        var source = FindTypeSource(type);
        var block = ExtractTypeBlock(source.Code, type.Name);
        var member = ExtractMember(block.Code, type.Name, memberName);
        var lines = SplitLines(member.Trim('\r', '\n'));
        var snippets = new List<CodeSnippet>(excerpts.Length);

        foreach (var excerpt in excerpts)
        {
            if (excerpt.EndLine > lines.Length)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(excerpts),
                    excerpt.EndLine,
                    $"Range {excerpt.StartLine}-{excerpt.EndLine} is outside member '{memberName}', which has {lines.Length} line(s).");
            }

            var selectedLines = lines
                .Skip(excerpt.StartLine - 1)
                .Take(excerpt.EndLine - excerpt.StartLine + 1);
            var code = Reindent(selectedLines, 0);

            snippets.Add(new CodeSnippet(
                $"{source.FileName} | {type.Name}.{memberName} | linhas {excerpt.StartLine}-{excerpt.EndLine}",
                code,
                excerpt.Caption));
        }

        return snippets;
    }

    private static CodeSnippet FindTypeSource(Type type)
    {
        var repositoryRoot = FindRepositoryRoot();
        var assemblyName = type.Assembly.GetName().Name;

        if (!string.IsNullOrWhiteSpace(assemblyName))
        {
            var assemblySourceDirectory = Path.Combine(repositoryRoot, "src", assemblyName);
            if (Directory.Exists(assemblySourceDirectory))
            {
                var namedFile = Directory
                    .EnumerateFiles(assemblySourceDirectory, $"{type.Name}.cs", SearchOption.AllDirectories)
                    .FirstOrDefault();

                if (namedFile is not null)
                {
                    return new CodeSnippet(ToRelativePath(repositoryRoot, namedFile), File.ReadAllText(namedFile));
                }
            }
        }

        var sourceFile = Directory
            .EnumerateFiles(repositoryRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => IsSourceFile(repositoryRoot, path))
            .FirstOrDefault(path => TypeDeclarationRegex(type.Name).IsMatch(File.ReadAllText(path)));

        if (sourceFile is null)
        {
            throw new FileNotFoundException($"Source file for type '{type.FullName}' was not found under '{repositoryRoot}'.");
        }

        return new CodeSnippet(ToRelativePath(repositoryRoot, sourceFile), File.ReadAllText(sourceFile));
    }

    private static TypeBlock ExtractTypeBlock(string code, string typeName)
    {
        // ponytail: textual parser for repo tutorial sources; switch to Roslyn only when real C# edge cases break it.
        var match = TypeDeclarationRegex(typeName).Match(code);
        if (!match.Success)
        {
            throw new InvalidOperationException($"Type '{typeName}' was not found in the source file.");
        }

        var openBrace = code.IndexOf('{', match.Index);
        if (openBrace < 0)
        {
            throw new InvalidOperationException($"Type '{typeName}' does not have a body.");
        }

        var closeBrace = FindMatchingBrace(code, openBrace);
        var declaration = code[match.Index..openBrace].TrimEnd();
        var block = code[match.Index..(closeBrace + 1)];

        return new TypeBlock(declaration, block);
    }

    private static string ExtractMember(string typeCode, string typeName, string memberName)
    {
        var start = FindMemberStart(typeCode, typeName, memberName);
        var openBrace = typeCode.IndexOf('{', start);
        var semicolon = typeCode.IndexOf(';', start);

        if (semicolon >= 0 && (openBrace < 0 || semicolon < openBrace))
        {
            return typeCode[start..(semicolon + 1)].TrimEnd();
        }

        if (openBrace < 0)
        {
            throw new InvalidOperationException($"Member '{memberName}' does not have a readable body.");
        }

        var closeBrace = FindMatchingBrace(typeCode, openBrace);
        var end = closeBrace + 1;
        if (end < typeCode.Length && typeCode[end] == ';')
        {
            end++;
        }

        return typeCode[start..end].TrimEnd();
    }

    private static int FindMemberStart(string typeCode, string typeName, string memberName)
    {
        var depth = 0;
        var lineStart = 0;

        for (var index = 0; index <= typeCode.Length; index++)
        {
            if (index < typeCode.Length && typeCode[index] is not '\n')
            {
                continue;
            }

            var line = typeCode[lineStart..index];
            if (depth == 1 && IsMemberDeclaration(line, typeName, memberName))
            {
                return lineStart;
            }

            depth += Count(line, '{') - Count(line, '}');
            lineStart = index + 1;
        }

        throw new InvalidOperationException($"Member '{memberName}' was not found in type '{typeName}'.");
    }

    private static bool IsMemberDeclaration(string line, string typeName, string memberName)
    {
        var trimmed = line.TrimStart();
        if (trimmed.Length == 0 || trimmed.StartsWith("//", StringComparison.Ordinal) || trimmed.StartsWith("[", StringComparison.Ordinal))
        {
            return false;
        }

        return memberName == typeName
            ? Regex.IsMatch(trimmed, $@"\b{Regex.Escape(typeName)}\s*\(")
            : Regex.IsMatch(trimmed, $@"\b{Regex.Escape(memberName)}\b\s*(?:\(|\{{|=>|=|;)");
    }

    private static int FindMatchingBrace(string code, int openBrace)
    {
        var depth = 0;

        for (var index = openBrace; index < code.Length; index++)
        {
            depth += code[index] switch
            {
                '{' => 1,
                '}' => -1,
                _ => 0
            };

            if (depth == 0)
            {
                return index;
            }
        }

        throw new InvalidOperationException("Could not find the closing brace for the snippet.");
    }

    private static string FindRepositoryRoot()
    {
        return FindRepositoryRoot(Directory.GetCurrentDirectory())
            ?? FindRepositoryRoot(AppContext.BaseDirectory)
            ?? throw new DirectoryNotFoundException($"Could not find the repository root containing '{RootSolutionFile}'.");
    }

    private static string? FindRepositoryRoot(string startPath)
    {
        var current = new DirectoryInfo(startPath);

        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, RootSolutionFile)))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return null;
    }

    private static bool IsSourceFile(string repositoryRoot, string path)
    {
        var normalized = Path.GetRelativePath(repositoryRoot, path).Replace(Path.DirectorySeparatorChar, '/');
        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);

        return !segments.Contains("bin")
            && !segments.Contains("obj")
            && !segments.Contains(".worktrees");
    }

    private static string ToRelativePath(string repositoryRoot, string path)
    {
        return Path.GetRelativePath(repositoryRoot, path).Replace(Path.DirectorySeparatorChar, '/');
    }

    private static int Count(string value, char character)
    {
        return value.Count(candidate => candidate == character);
    }

    private static string Reindent(string value, int spaces)
    {
        var lines = SplitLines(value.Trim('\r', '\n'));
        return Reindent(lines, spaces);
    }

    private static string Reindent(IEnumerable<string> lines, int spaces)
    {
        var linesArray = lines.ToArray();
        var commonIndent = linesArray
            .Where(static line => !string.IsNullOrWhiteSpace(line))
            .Select(LeadingSpaces)
            .DefaultIfEmpty(0)
            .Min();
        var prefix = new string(' ', spaces);

        return string.Join(
            Environment.NewLine,
            linesArray.Select(line => string.IsNullOrWhiteSpace(line)
                ? string.Empty
                : $"{prefix}{line[commonIndent..]}"));
    }

    private static string[] SplitLines(string value)
    {
        return value
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n');
    }

    private static int LeadingSpaces(string value)
    {
        var count = 0;
        while (count < value.Length && value[count] == ' ')
        {
            count++;
        }

        return count;
    }

    private static Regex TypeDeclarationRegex(string typeName)
    {
        return new Regex(
            $@"^\s*(?:(?:public|internal|private|protected|file|sealed|abstract|static|partial|readonly|ref)\s+)*(?:class|struct|interface|record(?:\s+(?:class|struct))?)\s+{Regex.Escape(typeName)}\b",
            RegexOptions.CultureInvariant | RegexOptions.Multiline);
    }

    private sealed record TypeBlock(string Declaration, string Code);
}
