using Godot;

using KirisameLib.Logging;

namespace BabelRush.Cards;

public partial class CardInterface : Node2D
{
    //Factory
    private CardInterface() { }

    public static CardInterface GetInstance(ICard card)
    {
        CardInterface instance = CreateInstance();
        instance.Card = card;
        return instance;
    }

    private static CardInterface CreateInstance()
    {
        var instance = Scene.Instantiate<CardInterface>();
        instance.CallDeferred(MethodName.InitSignal);
        return instance;
    }

    private const string ScenePath = "res://Cards/Card.tscn";
    private static PackedScene Scene { get; } = ResourceLoader.Load<PackedScene>(ScenePath);

    
    //Sub nodes
    private TextureRect? _iconNode;
    private TextureRect IconNode => _iconNode ??= GetNode<TextureRect>("Icon");

    private Node2D? _costNode;
    private Node2D CostNode => _costNode ??= GetNode<Node2D>("Cost");

    private Sprite2D? _boardNode;
    private Sprite2D BoardNode => _boardNode ??= GetNode<Sprite2D>("Board");

    
    //Property
    private ICard? _card;
    public ICard Card
    {
        get
        {
            if (_card is not null) return _card;
            Logger.Log(LogLevel.Error, "GettingCard", $"CardInterface {this} has no card instance reference");
            return CommonCard.Default;
        }
        private set
        {
            _card = value;
            CallDeferred(MethodName.Refresh);
        }
    }

    
    //Update
    private static readonly StringName StringNameSetValue = "SetValue";

    private void Refresh()
    {
        IconNode.Texture = Card.Type.Icon;
        CostNode.CallDeferred(StringNameSetValue, Card.Cost);
    }

    
    //Signal
    [Signal] public delegate void MouseEnteredEventHandler(CardInterface source);

    [Signal] public delegate void MouseExitedEventHandler(CardInterface source);

    private void InitSignal()
    {
        var boxNode = GetNode<Control>("Box");
        boxNode.MouseEntered += () => EmitSignal(SignalName.MouseEntered, this);
        boxNode.MouseExited += () => EmitSignal(SignalName.MouseExited,   this);
    }


    //Logging
    private static Logger Logger { get; } = LogManager.GetLogger(nameof(CardInterface));
}