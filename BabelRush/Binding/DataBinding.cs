using KirisameY.BindingBridge.Binder;
using KirisameY.BindingBridge.CollectionBinding.Resolver;

namespace BabelRushR.Game.Binding;

public static class DataBinding
{
    public static IDataBinder Binder { get; } =
        new DataBinderBuilder()
           .WithCollectionSourceFallbackResolver(
                new NotifiableCollectionSourceEndpointResolver(DefaultCollectionEndpointSourceResolver.Instance)
            )
           .Build();
}