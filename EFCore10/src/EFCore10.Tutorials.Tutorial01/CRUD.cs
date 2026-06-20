using EFCore10.Shared;
using Microsoft.EntityFrameworkCore;

namespace EFCore10.Tutorials.Tutorial01;

public static class CRUD
{
    public static async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var db = new BloggingContext();

        TutorialConsole.WritePreparation(
            "Pacote SQLite adicionado ao projeto do tutorial.",
            "Modelos Blog e Post criados.",
            "BloggingContext configurado com SQLite em OnConfiguring.",
            "Migration InitialCreate criada e aplicada com dotnet ef.",
            $"Banco SQLite: {db.DbPath}.");

        TutorialConsole.WriteExperiment(
            1,
            "Criar e consultar um blog",
            "Adicionar um Blog ao DbContext, salvar as alterações e consultar o primeiro registro ordenado pelo ID.");
        db.Add(new Blog { Url = "http://blogs.msdn.com/adonet" });
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var blog = await db.Blogs
            .OrderBy(b => b.BlogId)
            .FirstAsync(cancellationToken)
            .ConfigureAwait(false);

        TutorialConsole.WriteObservation($"Blog recuperado: {blog.Url}.");
        TutorialConsole.WriteObservation($"Posts carregados nesse momento: {blog.Posts.Count}.");
        TutorialConsole.WriteConclusion(
            "O EF Core persistiu o Blog e materializou a entidade consultada a partir do SQLite.",
            TutorialConclusionKind.Success);

        TutorialConsole.WriteExperiment(
            2,
            "Atualizar o blog e adicionar um post",
            "Alterar a URL do Blog rastreado e adicionar um Post à navegação antes de salvar.");
        blog.Url = "https://devblogs.microsoft.com/dotnet";
        blog.Posts.Add(
            new Post { Title = "Hello World", Content = "I wrote an app using EF Core!" });
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        TutorialConsole.WriteObservation($"URL atualizada: {blog.Url}.");
        TutorialConsole.WriteObservation($"Posts no agregado em memória: {blog.Posts.Count}.");
        foreach (var post in blog.Posts)
        {
            TutorialConsole.WriteObservation($"Post associado ao BlogId {post.BlogId}: {post.Content}.");
        }

        TutorialConsole.WriteConclusion(
            "Como o Blog estava rastreado, o EF Core detectou a mudança da URL e a inclusão do Post na navegação.",
            TutorialConclusionKind.Success);

        TutorialConsole.WriteExperiment(
            3,
            "Remover o blog",
            "Remover a entidade rastreada e salvar as alterações.");
        db.Remove(blog);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        TutorialConsole.WriteConclusion(
            "O Blog foi marcado para remoção e a operação foi persistida no banco.",
            TutorialConclusionKind.Success);
        TutorialConsole.WriteCleanup("A demonstração terminou removendo o Blog criado no início do tutorial.");
    }
}
