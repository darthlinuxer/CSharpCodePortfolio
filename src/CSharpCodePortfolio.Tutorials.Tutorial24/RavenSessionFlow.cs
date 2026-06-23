namespace CSharpCodePortfolio.Tutorials.Tutorial24;

internal static class RavenSessionFlow
{
    public static IReadOnlyList<string> DescribeUserFlow(RavenUser user, int page, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (page <= 0)
            throw new ArgumentOutOfRangeException(nameof(page), page, "A página deve ser positiva.");

        if (pageSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "O tamanho da página deve ser positivo.");

        var skip = (page - 1) * pageSize;

        return
        [
            "OpenSession()",
            $"Store({user.Id})",
            "SaveChanges()",
            $"Query<RavenUser>().Skip({skip}).Take({pageSize})",
            $"Load<RavenUser>({user.Id})",
            $"Delete({user.Id})",
            "SaveChanges()"
        ];
    }
}
