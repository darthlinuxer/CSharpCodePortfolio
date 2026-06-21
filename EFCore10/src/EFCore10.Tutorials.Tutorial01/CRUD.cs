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
            "Adicionar um Blog ao DbContext, salvar as alterações e consultar o registro pelo ID gerado.");
        TutorialConsole.WriteCodeSnippet(
            "Criar uma entidade, salvar e consultar o mesmo registro pelo ID gerado.",
            "CRUD.cs",
            """
            var createdBlog = new Blog { Url = "http://blogs.msdn.com/adonet" };
            db.Add(createdBlog);
            await db.SaveChangesAsync(cancellationToken);

            var blog = await db.Blogs
                .SingleAsync(b => b.BlogId == createdBlog.BlogId, cancellationToken);
            """);
        var createdBlog = new Blog { Url = "http://blogs.msdn.com/adonet" };
        db.Add(createdBlog);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var blog = await db.Blogs
            .SingleAsync(b => b.BlogId == createdBlog.BlogId, cancellationToken)
            .ConfigureAwait(false);

        TutorialConsole.WriteEvidence(
            "Blog persistido e consultado",
            ("BlogId gerado", blog.BlogId.ToString()),
            ("URL esperada", "http://blogs.msdn.com/adonet"),
            ("URL obtida", blog.Url),
            ("Posts no agregado", blog.Posts.Count.ToString()));
        TutorialConsole.WriteConclusion(
            "O EF Core persistiu o Blog e materializou a entidade consultada a partir do SQLite.",
            TutorialConclusionKind.Success);

        TutorialConsole.WriteExperiment(
            2,
            "Atualizar o blog e adicionar um post",
            "Alterar a URL do Blog rastreado e adicionar um Post à navegação antes de salvar.");
        TutorialConsole.WriteCodeSnippet(
            "Alterar a entidade rastreada e adicionar um item à navegação.",
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
            "Blog atualizado e Post persistido",
            ("URL obtida", blog.Url),
            ("Posts no agregado", blog.Posts.Count.ToString()),
            ("Posts no banco para o BlogId", postCount.ToString()),
            ("PostId gerado", savedPost.PostId.ToString()),
            ("BlogId do Post", savedPost.BlogId.ToString()),
            ("Conteúdo do Post", savedPost.Content));

        TutorialConsole.WriteConclusion(
            "Como o Blog estava rastreado, o EF Core detectou a mudança da URL e a inclusão do Post na navegação.",
            TutorialConclusionKind.Success);

        TutorialConsole.WriteExperiment(
            3,
            "Remover o blog",
            "Remover a entidade rastreada e salvar as alterações.");
        TutorialConsole.WriteCodeSnippet(
            "Marcar a entidade para remoção e persistir.",
            "CRUD.cs",
            """
            var blogId = blog.BlogId;
            db.Remove(blog);
            await db.SaveChangesAsync(cancellationToken);

            var remainingBlogs = await db.Blogs
                .CountAsync(b => b.BlogId == blogId, cancellationToken);
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
            "O Blog foi marcado para remoção e a operação foi persistida no banco.",
            TutorialConclusionKind.Success);
        TutorialConsole.WriteCleanup("A demonstração terminou removendo o Blog criado no início do tutorial.");
    }
}
