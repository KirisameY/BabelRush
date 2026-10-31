using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;

using BabelRushR.Core.GamePlay;
using BabelRushR.Core.Scenery;

namespace BabelRushR.Core.Test;

public class GamePlayEventHandlerTests
{
    private static readonly ConcurrentDictionary<string, int> Received = new();
    private static readonly ConcurrentDictionary<string, string> Unexpected = new();
    private static readonly ConcurrentDictionary<ulong, int> DisposedSeeds = new();
    private static long _nextSeed;


    private static ulong NewSeed() => (ulong)Interlocked.Increment(ref _nextSeed);

    private static string NewToken() => Guid.NewGuid().ToString("N");

    private static CommonGamePlay NewGame(ulong seed)
        => CommonGamePlay.Create(seed, new TestEntity(maxHP: 10));

    private static CommonGamePlay NewHandAssembledGame(ulong seed)
        => new(seed)
        {
            Scene       = new CommonScene(),
            PlayerState = new CommonPlayerState(new TestEntity(maxHP: 10)),
        };


    [Fact]
    public void Create_Auto_Registers_Attributed_Private_Nested_Handler()
    {
        using var game = NewGame(NewSeed());
        var token = NewToken();

        game.EventBus.Publish(new ProbeEvent(token));

        Assert.Equal(1, Received[token]);
        Assert.False(Unexpected.ContainsKey(token));
    }

    [Fact]
    public void Interface_Only_Type_Is_Ignored()
    {
        using var game = NewGame(NewSeed());
        var token = NewToken();

        game.EventBus.Publish(new ProbeEvent(token));

        Assert.Equal(1, Received[token]);
        Assert.False(Unexpected.ContainsKey(token));
    }

    [Fact]
    public void InitializeEventHandlers_Is_Idempotent()
    {
        using var game = NewGame(NewSeed());
        var token = NewToken();

        game.InitializeEventHandlers();
        game.EventBus.Publish(new ProbeEvent(token));

        Assert.Equal(1, Received[token]);
    }

    [Fact]
    public void Dispose_Disposes_Handlers_And_Is_Idempotent()
    {
        var seed = NewSeed();
        var game = NewGame(seed);
        var token = NewToken();

        game.EventBus.Publish(new ProbeEvent(token));
        Assert.Equal(1, Received[token]);

        game.Dispose();
        game.Dispose();
        game.EventBus.Publish(new ProbeEvent(token));

        // 处理器释放后事件方法应变为惰性；订阅本身不要求在这里撤销。
        Assert.Equal(1, Received[token]);
        Assert.Equal(1, DisposedSeeds[seed]);
    }

    [Fact]
    public void Manual_Assembly_Requires_Explicit_Initialization()
    {
        using var game = NewHandAssembledGame(NewSeed());
        var token = NewToken();

        game.EventBus.Publish(new ProbeEvent(token));
        Assert.False(Received.ContainsKey(token));

        game.InitializeEventHandlers();
        game.EventBus.Publish(new ProbeEvent(token));

        Assert.Equal(1, Received[token]);
    }

    [Fact]
    public void Initialize_After_Dispose_Does_Nothing()
    {
        var game = NewGame(NewSeed());
        game.Dispose();

        game.InitializeEventHandlers();
        var token = NewToken();
        game.EventBus.Publish(new ProbeEvent(token));

        Assert.False(Received.ContainsKey(token));
    }

    [Fact]
    public void Registration_Failure_Disposes_Earlier_Handlers()
    {
        GoodExplicitHandler.Reset();
        using var game = NewHandAssembledGame(NewSeed());

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            GamePlayEventHandlerRegistry.RegisterAll(
                game, [typeof(GoodExplicitHandler), typeof(ThrowingRegisterHandler)]);
        });

        Assert.Contains(nameof(ThrowingRegisterHandler), ex.Message);
        Assert.True(GoodExplicitHandler.Disposed);

        var token = NewToken();
        game.EventBus.Publish(new ProbeEvent(token));
        Assert.False(Received.ContainsKey(token));
    }

    [Fact]
    public void Dispose_Continues_After_A_Handler_Throws()
    {
        GoodExplicitHandler.Reset();
        using var game = NewHandAssembledGame(NewSeed());

        var registrations = GamePlayEventHandlerRegistry.RegisterAll(
            game, [typeof(GoodExplicitHandler), typeof(ThrowingDisposeHandler)]);

        var ex = Assert.Throws<AggregateException>(() => registrations.Dispose());

        Assert.Single(ex.InnerExceptions);
        Assert.True(GoodExplicitHandler.Disposed);
    }

    [Fact]
    public void Null_Returning_Handler_Is_Allowed()
    {
        using var game = NewHandAssembledGame(NewSeed());

        var registrations = GamePlayEventHandlerRegistry.RegisterAll(game, [typeof(NullReturnHandler)]);

        registrations.Dispose();
    }

    [Fact]
    public void Invalid_Explicit_Type_Is_Rejected()
    {
        using var game = NewHandAssembledGame(NewSeed());

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            GamePlayEventHandlerRegistry.RegisterAll(game, [typeof(string)]);
        });

        Assert.Contains(nameof(IGamePlayEventHandler), ex.Message);
    }

    [Fact]
    public void Attributed_Type_Without_Interface_Throws()
    {
        var assembly = EmitAttributedType("NotAHandler");

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            GamePlayEventHandlerRegistry.DiscoverHandlerTypes([assembly]);
        });

        Assert.Contains("NotAHandler", ex.Message);
        Assert.Contains(nameof(IGamePlayEventHandler), ex.Message);
    }

    [Fact]
    public void Attributed_Abstract_Type_Throws()
    {
        var assembly = EmitAttributedType("AbstractHandler", isAbstract: true);

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            GamePlayEventHandlerRegistry.DiscoverHandlerTypes([assembly]);
        });

        Assert.Contains("AbstractHandler", ex.Message);
        Assert.Contains("abstract", ex.Message);
    }

    [Fact]
    public void Attributed_Open_Generic_Type_Throws()
    {
        var assembly = EmitAttributedType("GenericHandler", isGeneric: true);

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            GamePlayEventHandlerRegistry.DiscoverHandlerTypes([assembly]);
        });

        Assert.Contains("GenericHandler", ex.Message);
        Assert.Contains("open generic", ex.Message);
    }


    private static Assembly EmitAttributedType(string typeName, bool isAbstract = false, bool isGeneric = false)
    {
        var assembly = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName($"BabelRushR.Core.Test.InvalidHandlers.{Guid.NewGuid():N}"),
            AssemblyBuilderAccess.Run);
        var module = assembly.DefineDynamicModule("Main");

        var attributes = TypeAttributes.Public | TypeAttributes.Class;
        if (isAbstract) attributes |= TypeAttributes.Abstract;

        var typeBuilder = module.DefineType(typeName, attributes);
        if (isGeneric) typeBuilder.DefineGenericParameters("T");

        var attributeConstructor = typeof(GamePlayEventHandlerAttribute).GetConstructor(Type.EmptyTypes)!;
        typeBuilder.SetCustomAttribute(new CustomAttributeBuilder(attributeConstructor, []));

        typeBuilder.CreateType();
        return assembly;
    }


    private sealed record ProbeEvent(string Token) : GamePlayEvent;

    [GamePlayEventHandler]
    private sealed class ProbeHandler : IGamePlayEventHandler, IDisposable
    {
        private readonly ulong _seed;
        private bool _disposed;

        private ProbeHandler(IGamePlay gamePlay)
        {
            _seed = gamePlay.RandomSeed;
            _ = gamePlay.EventBus.Subscribe<ProbeEvent>(OnProbe);
        }

        public static IDisposable? Register(IGamePlay gamePlay) => new ProbeHandler(gamePlay);

        private void OnProbe(ProbeEvent e)
        {
            if (_disposed) return;
            Received.AddOrUpdate(e.Token, 1, (_, count) => count + 1);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            DisposedSeeds.AddOrUpdate(_seed, 1, (_, count) => count + 1);
        }
    }

    private sealed class InterfaceOnlyHandler : IGamePlayEventHandler
    {
        public static IDisposable? Register(IGamePlay gamePlay)
        {
            _ = gamePlay.EventBus.Subscribe<ProbeEvent>(
                e => Unexpected.TryAdd(e.Token, nameof(InterfaceOnlyHandler)));
            return null;
        }
    }

    private sealed class GoodExplicitHandler : IGamePlayEventHandler, IDisposable
    {
        public static bool Disposed { get; private set; }

        public static void Reset() => Disposed = false;

        public static IDisposable? Register(IGamePlay gamePlay)
        {
            var handler = new GoodExplicitHandler();
            _ = gamePlay.EventBus.Subscribe<ProbeEvent>(handler.OnProbe);
            return handler;
        }

        private bool _disposed;

        private void OnProbe(ProbeEvent e)
        {
            if (_disposed) return;
            Received.AddOrUpdate(e.Token, 1, (_, count) => count + 1);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Disposed = true;
        }
    }

    private sealed class ThrowingRegisterHandler : IGamePlayEventHandler
    {
        public static IDisposable? Register(IGamePlay gamePlay)
            => throw new InvalidOperationException("register failed");
    }

    private sealed class ThrowingDisposeHandler : IGamePlayEventHandler, IDisposable
    {
        public static IDisposable? Register(IGamePlay gamePlay) => new ThrowingDisposeHandler();

        public void Dispose() => throw new InvalidOperationException("dispose failed");
    }

    private sealed class NullReturnHandler : IGamePlayEventHandler
    {
        public static IDisposable? Register(IGamePlay gamePlay) => null;
    }
}
