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
    private Sprite2D? _boardNode;
    private Sprite2D BoardNode => _boardNode ??= GetNode<Sprite2D>("Board");

    private TextureRect? _iconNode;
    private TextureRect IconNode => _iconNode ??= GetNode<TextureRect>("Icon");

    private Node2D? _costNode;
    private Node2D CostNode => _costNode ??= GetNode<Node2D>("Cost");

    private Node2D? _action0Node;
    private Node2D Action0Node => _action0Node ??= GetNode<Node2D>("Action0");

    private Node2D? _action1Node;
    private Node2D Action1Node => _action1Node ??= GetNode<Node2D>("Action1");

    private Node2D? _featuresNode;
    private Node2D FeaturesNode => _featuresNode ??= GetNode<Node2D>("Features");


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
    public Tween? XPosTween { get; set; }
    public Tween? YPosTween { get; set; }


    //Update
    private static readonly StringName StringNameSetValue = "SetValue";
    private static readonly StringName StringNameSetCount = "SetCount";
    private static readonly StringName StringNameSetEmpty = "SetEmpty";
    private static readonly StringName StringNameSetIcon = "SetIcon";

    private void Refresh()
    {
        //Icon
        IconNode.Texture = Card.Type.Icon;

        //Cost
        CostNode.CallDeferred(StringNameSetValue, Card.Cost);

        //Actions
        var actionCount = Card.Actions.Count;
        Action0Node.CallDeferred(StringNameSetEmpty, actionCount <= 0);
        Action1Node.CallDeferred(StringNameSetEmpty, actionCount <= 1);
        if (actionCount > 0)
        {
            Action0Node.CallDeferred(StringNameSetIcon,  Card.Actions[0].Type.Icon);
            Action0Node.CallDeferred(StringNameSetValue, Card.Actions[0].Value);
        }

        if (actionCount > 1)
        {
            Action1Node.CallDeferred(StringNameSetIcon,  Card.Actions[1].Type.Icon);
            Action1Node.CallDeferred(StringNameSetValue, Card.Actions[1].Value);
        }

        //Features
        var featuresCount = Card.Features.Count;
        FeaturesNode.CallDeferred(StringNameSetCount, featuresCount);
        for (int i = 0; i < featuresCount; i++)
        {
            FeaturesNode.CallDeferred(StringNameSetIcon, i, Card.Features[i].Type.Icon);
        }
    }


    //Extend
    public void SetPositionX(float x) => Position = new(x, Position.Y);
    public void SetPositionY(float y) => Position = new(Position.X, y);


    //Select
    public ICardContainer? Container { get; set; }

    private bool _selectable;
    public bool Selectable
    {
        get => _selectable;
        set
        {
            if (_selectable == value) return;
            _selectable = value;
            if (!_selectable && Selected) Selected = false;
        }
    }

    private bool _selected;
    public bool Selected
    {
        get => _selected;
        private set
        {
            if (!Selectable) return;
            if (_selected == value) return;
            _selected = value;
            if (_selected) Container?.CardSelected(this);
            else Container?.CardUnselected(this);
        }
    }

    private void InitSignal()
    {
        var boxNode = GetNode<Control>("Box");
        boxNode.MouseEntered += () => Selected = true;
        boxNode.MouseExited += () => Selected = false;
    }


    //Logging
    private static Logger Logger { get; } = LogManager.GetLogger(nameof(CardInterface));
}

public interface ICardContainer
{
    void CardSelected(CardInterface card);
    void CardUnselected(CardInterface card);
}