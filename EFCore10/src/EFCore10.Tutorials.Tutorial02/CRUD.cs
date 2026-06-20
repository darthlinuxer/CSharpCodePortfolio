using EFCore10.Shared;
using Microsoft.EntityFrameworkCore;
using EFCore10.Tutorials.Tutorial02.Context;
using EFCore10.Tutorials.Tutorial02.Models;

namespace EFCore10.Tutorials.Tutorial02;

public sealed class CRUD(BloggingContext db)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await db.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);

        TutorialConsole.WritePreparation(
            "BloggingContext foi criado pelo container de DI.",
            "EnsureCreatedAsync garantiu o schema SQLite para a execução do tutorial.",
            "OnModelCreating aplicou as classes IEntityTypeConfiguration<T> do assembly.");

        TutorialConsole.WriteExperiment(
            1,
            "Inserir um blog usando o contexto injetado",
            "Usar o serviço CRUD resolvido por DI para adicionar um Blog e salvar as alterações.");
        db.Add(new Blog { Url = "http://blogs.msdn.com/adonet" });
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        TutorialConsole.WriteObservation("Um Blog foi adicionado e salvo pelo BloggingContext injetado.");
        TutorialConsole.WriteConclusion(
            "O serviço CRUD executou a escrita usando o BloggingContext recebido por injeção de dependência.",
            TutorialConclusionKind.Success);

        TutorialConsole.WriteExperiment(
            2,
            "Consultar o blog e carregar a coleção de posts",
            "Consultar Blogs com Include para carregar a navegação Posts no mesmo fluxo.");
        var blog = await db.Blogs
            .Include(blog => blog.Posts)
            .OrderBy(b => b.BlogId)
            .FirstAsync(cancellationToken).ConfigureAwait(false);

        TutorialConsole.WriteObservation($"Blog recuperado: {blog.Url}.");
        TutorialConsole.WriteObservation($"Posts carregados: {blog.Posts.Count}.");
        TutorialConsole.WriteConclusion(
            "A consulta usa o contexto configurado por DI e respeita o modelo configurado por Fluent API.",
            TutorialConclusionKind.Success);

        TutorialConsole.WriteExperiment(
            3,
            "Atualizar o blog e adicionar um post",
            "Alterar a URL, adicionar um Post à navegação e salvar para exercitar o relacionamento 1:N configurado no modelo.");
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
            "O relacionamento Blog 1:N Posts foi usado sem instanciar BloggingContext manualmente no serviço CRUD.",
            TutorialConclusionKind.Success);

        TutorialConsole.WriteExperiment(
            4,
            "Remover o blog",
            "Remover a entidade rastreada e salvar as alterações usando o mesmo contexto injetado.");
        db.Remove(blog);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        TutorialConsole.WriteConclusion(
            "A operação de remoção também passou pelo serviço e pelo contexto gerenciados por DI.",
            TutorialConclusionKind.Success);
        TutorialConsole.WriteCleanup("A demonstração terminou removendo o Blog criado no início do tutorial.");
    }
}
