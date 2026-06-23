namespace CSharpCodePortfolio.Tutorials.Tutorial15;

internal sealed class TransientOperation : IDisposable
{
    public Guid InstanceId { get; } = Guid.NewGuid();

    public bool IsDisposed { get; private set; }

    public void Dispose() =>
        IsDisposed = true;
}
