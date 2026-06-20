using EFCore10.Tutorials.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace EFCore10.Tutorials.Tutorial01;

[Tutorial("01", "simple-modeling", "Simple EF Core Modeling")]
public sealed class FirstTutorial : ITutorial
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Tutorial 01 - Simple EFCore Modeling");
        string multiline = @"
            Steps: 
            1. dotnet add src/EFCore10.Tutorials.Tutorial01 package Microsoft.EntityFrameworkCore.Sqlite
            2. Created the models and the dbcontext 
            3. Created the Database : 
                - dotnet tool install --global dotnet-ef
                - dotnet add src/EFCore10.Tutorials.Tutorial01 package Microsoft.EntityFrameworkCore.Design
                - dotnet ef migrations --project src/EFCore10.Tutorials.Tutorial01 add InitialCreate
                - dotnet ef database --project src/EFCore10.Tutorials.Tutorial01 update
        ";

        Console.WriteLine(multiline);
        await CRUD.ExecuteAsync();        
    }
}
