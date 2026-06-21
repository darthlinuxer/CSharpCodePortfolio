using EFCore10.Shared;
using Microsoft.EntityFrameworkCore;
using EFCore10.Tutorials.Tutorial02.Context;
using EFCore10.Tutorials.Tutorial02.Models;

namespace EFCore10.Tutorials.Tutorial02;

public sealed class CRUD(BloggingContext db)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var schemaCreated = await db.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);

        TutorialConsole.WritePreparation(
            "BloggingContext foi criado pelo container de DI.",
            "EnsureCreatedAsync garantiu o schema SQLite para a execução do tutorial.",
            "OnModelCreating aplicou as classes IEntityTypeConfiguration<T> do assembly.");
        TutorialConsole.WriteCodeSnippet(
            "O serviço recebe BloggingContext pelo construtor em vez de instanciar manualmente.",
            "CRUD.cs",
            """
            public sealed class CRUD(BloggingContext db)
            {
                public async Task ExecuteAsync(CancellationToken cancellationToken)
                {
                    await db.Database.EnsureCreatedAsync(cancellationToken);
                }
            }
            """);
        TutorialConsole.WriteEvidence(
            "Infraestrutura resolvida por DI",
            ("DbContext recebido", db.GetType().Name),
            ("Provider EF Core", db.Database.ProviderName ?? "(desconhecido)"),
            ("Banco SQLite", SqliteConnectionStrings.GetDisplayDataSource(
                db.Database.GetDbConnection().ConnectionString,
                AppContext.BaseDirectory)),
            ("Schema criado nesta execução?", schemaCreated ? "sim" : "não, já existia"));

        TutorialConsole.WriteExperiment(
            1,
            "Inserir um blog usando o contexto injetado",
            "Usar o serviço CRUD resolvido por DI para adicionar um Blog e salvar as alterações.");
        TutorialConsole.WriteCodeSnippet(
            "Usar o contexto injetado para adicionar e salvar um Blog.",
            "CRUD.cs",
            """
            var createdBlog = new Blog { Url = "http://blogs.msdn.com/adonet" };
            db.Add(createdBlog);
            await db.SaveChangesAsync(cancellationToken);

            var persistedBlogs = await db.Blogs
                .CountAsync(
                    blog => blog.BlogId == createdBlog.BlogId,
                    cancellationToken);
            """);
        var createdBlog = new Blog { Url = "http://blogs.msdn.com/adonet" };
        db.Add(createdBlog);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        var persistedBlogs = await db.Blogs
            .CountAsync(blog => blog.BlogId == createdBlog.BlogId, cancellationToken)
            .ConfigureAwait(false);

        TutorialConsole.WriteEvidence(
            "Blog salvo pelo contexto injetado",
            ("BlogId gerado", createdBlog.BlogId.ToString()),
            ("URL salva", createdBlog.Url),
            ("Registros encontrados por ID", persistedBlogs.ToString()));
        TutorialConsole.WriteConclusion(
            "O serviço CRUD executou a escrita usando o BloggingContext recebido por injeção de dependência.",
            TutorialConclusionKind.Success);

        TutorialConsole.WriteExperiment(
            2,
            "Consultar o blog e carregar a coleção de posts",
            "Consultar Blogs com Include para carregar a navegação Posts no mesmo fluxo.");
        TutorialConsole.WriteCodeSnippet(
            "Consultar usando Include para carregar a navegação Posts.",
            "CRUD.cs",
            """
            var blog = await db.Blogs
                .Include(blog => blog.Posts)
                .SingleAsync(
                    blog => blog.BlogId == createdBlog.BlogId,
                    cancellationToken);
            """);
        var blog = await db.Blogs
            .Include(blog => blog.Posts)
            .SingleAsync(blog => blog.BlogId == createdBlog.BlogId, cancellationToken)
            .ConfigureAwait(false);

        TutorialConsole.WriteEvidence(
            "Consulta com Include",
            ("BlogId consultado", blog.BlogId.ToString()),
            ("URL obtida", blog.Url),
            ("Posts carregados", blog.Posts.Count.ToString()),
            ("Modelo aplicado por", "IEntityTypeConfiguration<T>"));
        TutorialConsole.WriteConclusion(
            "A consulta usa o contexto configurado por DI e respeita o modelo configurado por Fluent API.",
            TutorialConclusionKind.Success);

        TutorialConsole.WriteExperiment(
            3,
            "Atualizar o blog e adicionar um post",
            "Alterar a URL, adicionar um Post à navegação e salvar para exercitar o relacionamento 1:N configurado no modelo.");
        TutorialConsole.WriteCodeSnippet(
            "Adicionar um Post pela navegação configurada como relacionamento 1:N.",
            "CRUD.cs",
            """
            blog.Url = "https://devblogs.microsoft.com/dotnet";
            blog.Posts.Add(new Post
            {
                Title = "Hello World",
                Content = "I wrote an app using EF Core!"
            });
            await db.SaveChangesAsync(cancellationToken);

            var postCount = await db.Posts
                .CountAsync(post => post.BlogId == blog.BlogId, cancellationToken);
            """);
        blog.Url = "https://devblogs.microsoft.com/dotnet";
        blog.Posts.Add(
            new Post { Title = "Hello World", Content = "I wrote an app using EF Core!" });
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var savedPost = blog.Posts.Single();
        var postCount = await db.Posts
            .CountAsync(post => post.BlogId == blog.BlogId, cancellationToken)
            .ConfigureAwait(false);

        TutorialConsole.WriteEvidence(
            "Relacionamento 1:N persistido",
            ("URL atualizada", blog.Url),
            ("Posts no agregado", blog.Posts.Count.ToString()),
            ("Posts no banco para o BlogId", postCount.ToString()),
            ("PostId gerado", savedPost.PostId.ToString()),
            ("BlogId do Post", savedPost.BlogId.ToString()));

        TutorialConsole.WriteConclusion(
            "O relacionamento Blog 1:N Posts foi usado sem instanciar BloggingContext manualmente no serviço CRUD.",
            TutorialConclusionKind.Success);

        TutorialConsole.WriteExperiment(
            4,
            "Remover o blog",
            "Remover a entidade rastreada e salvar as alterações usando o mesmo contexto injetado.");
        TutorialConsole.WriteCodeSnippet(
            "Remover a entidade rastreada usando o mesmo contexto injetado.",
            "CRUD.cs",
            """
            var blogId = blog.BlogId;
            db.Remove(blog);
            await db.SaveChangesAsync(cancellationToken);

            var remainingBlogs = await db.Blogs
                .CountAsync(candidate => candidate.BlogId == blogId, cancellationToken);
            """);
        var blogIdToDelete = blog.BlogId;
        db.Remove(blog);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        var remainingBlogs = await db.Blogs
            .CountAsync(candidate => candidate.BlogId == blogIdToDelete, cancellationToken)
            .ConfigureAwait(false);
        var remainingPosts = await db.Posts
            .CountAsync(post => post.BlogId == blogIdToDelete, cancellationToken)
            .ConfigureAwait(false);

        TutorialConsole.WriteEvidence(
            "Remoção confirmada por consulta",
            ("BlogId removido", blogIdToDelete.ToString()),
            ("Blogs restantes com esse ID", remainingBlogs.ToString()),
            ("Posts restantes com esse BlogId", remainingPosts.ToString()));
        TutorialConsole.WriteConclusion(
            "A operação de remoção também passou pelo serviço e pelo contexto gerenciados por DI.",
            TutorialConclusionKind.Success);
        TutorialConsole.WriteCleanup("A demonstração terminou removendo o Blog criado no início do tutorial.");
    }
}
