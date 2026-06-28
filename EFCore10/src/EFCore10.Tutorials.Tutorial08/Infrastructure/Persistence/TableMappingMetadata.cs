using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore10.Tutorials.Tutorial08.Infrastructure.Persistence;

internal static class TableMappingMetadata
{
    public const string AnnotationName = "Tutorial08:TableMapping";
    public const string ConcreteTable = "Tabela concreta: aggregate root ou entidade sem hierarquia EF";
    public const string InternalEntity = "Entidade interna: tem identidade, mas nao e aggregate root";
    public const string Tph = "TPH: Table per Hierarchy para Employee/Professor/AdministrativeEmployee";
    public const string OwnsOne = "OwnsOne: dependente do owner, table splitting por padrao";
    public const string OwnsMany = "OwnsMany: colecao dependente, tabela propria com chave do owner";
    public const string JoinEntity = "Join table/entity: tabela de associacao com ou sem CLR explicita";

    /// <summary>
    /// Adds tutorial metadata that describes the EF table mapping strategy.
    /// </summary>
    public static EntityTypeBuilder<TEntity> HasTableMapping<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        string mapping)
        where TEntity : class
    {
        builder.HasAnnotation(AnnotationName, mapping);

        return builder;
    }

    /// <summary>
    /// Adds tutorial metadata that describes the owned table mapping strategy.
    /// </summary>
    public static OwnedNavigationBuilder<TOwner, TDependent> HasTableMapping<TOwner, TDependent>(
        this OwnedNavigationBuilder<TOwner, TDependent> builder,
        string mapping)
        where TOwner : class
        where TDependent : class
    {
        builder.OwnedEntityType.SetAnnotation(AnnotationName, mapping);

        return builder;
    }
}
