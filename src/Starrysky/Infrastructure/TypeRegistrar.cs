using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace Starrysky.Infrastructure;

[ExcludeFromCodeCoverage]
internal sealed class TypeRegistrar : ITypeRegistrar
{
    private readonly IServiceCollection _builder;

    public TypeRegistrar(IServiceCollection builder) => _builder = builder;

    public ITypeResolver Build() => new TypeResolver(_builder.BuildServiceProvider());

    public void Register(Type service, Type implementation) => _builder.AddSingleton(service, implementation);

    public void RegisterInstance(Type service, object implementation) => _builder.AddSingleton(service, implementation);

    public void RegisterLazy(Type service, Func<object> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _builder.AddSingleton(service, _ => factory());
    }
}
