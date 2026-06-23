namespace CSharpCodePortfolio.Tutorials.Tutorial28;

internal sealed record PlinqPartition(
    IReadOnlyList<int> EvenNumbers,
    IReadOnlyList<int> OddNumbers);
