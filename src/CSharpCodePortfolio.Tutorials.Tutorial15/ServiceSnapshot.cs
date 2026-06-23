using Microsoft.Extensions.DependencyInjection;

namespace CSharpCodePortfolio.Tutorials.Tutorial15;

internal sealed record ServiceSnapshot(
    SingletonOperation Singleton,
    ScopedOperation Scoped,
    TransientOperation Transient)
{
    public Guid SingletonId => Singleton.InstanceId;

    public Guid ScopedId => Scoped.InstanceId;

    public Guid TransientId => Transient.InstanceId;

    public bool SingletonDisposed => Singleton.IsDisposed;

    public bool ScopedDisposed => Scoped.IsDisposed;

    public bool TransientDisposed => Transient.IsDisposed;

    public static ServiceSnapshot Capture(IServiceProvider services)
    {
        var singleton = services.GetRequiredService<SingletonOperation>();
        var scoped = services.GetRequiredService<ScopedOperation>();
        var transient = services.GetRequiredService<TransientOperation>();

        return new ServiceSnapshot(singleton, scoped, transient);
    }
}
