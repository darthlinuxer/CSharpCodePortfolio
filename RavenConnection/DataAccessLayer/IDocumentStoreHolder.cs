using Raven.Client.Documents;

namespace RavenConnection.Database
{
    public interface IDocumentStoreHolder
    {
        IDocumentStore Store { get; }
    }
}
