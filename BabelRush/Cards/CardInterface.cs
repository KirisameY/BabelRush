using Godot;

using KirisameLib.Logging;

namespace BabelRush.Cards;

public partial class CardInterface : Node2D
{
    //Factory
    private CardInterface() { }

    public static CardInterface CreateInstance(ICard card)
    {
        var instance = Scene.Instantiate<CardInterface>();
        instance.Card = card;
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

    //Logging
    private static Logger Logger { get; } = LogManager.GetLogger(nameof(CardInterface));
}