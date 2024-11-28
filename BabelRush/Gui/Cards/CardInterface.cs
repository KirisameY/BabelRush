using System.Diagnostics.CodeAnalysis;

using BabelRush.Cards;

using Godot;

using KirisameLib.Core.Events;
using KirisameLib.Core.Logging;

namespace BabelRush.Gui.Cards;

[EventHandlerContainer]
public partial class CardInterface : Node2D
{
    //Factory
    private CardInterface() { }

    public static CardInterface GetInstance(Card card)
    {
        CardInterface instance = CreateInstance();
        instance.Card = card;
        return instance;
    }

    private static CardInterface CreateInstance()
    {
        var instance = Scene.Instantiate<CardInterface>();
        return instance;
    }

    private const string ScenePath = "res://Gui/Cards/Card.tscn";
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
    [field: AllowNull, MaybeNull]
    public Card Card
    {
        get
        {
            if (field is not null) return field;
            Logger.Log(LogLevel.Error, "GettingCard", $"CardInterface {this} has no card instance reference");
            return Card.Default;
        }
        private set
        {
            field = value;
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
    private bool _preSelected, _prePressed;
    private bool _selectable;
    public bool Selectable
    {
        get => _selectable;
        set
        {
            if (_selectable == value) return;
            _selectable = value;
            if (_selectable)
            {
                Selected = _preSelected;
                Pressed = _prePressed;
            }
            else
            {
                Selected = false;
                Pressed = false;
            }
        }
    }

    private bool _selected;
    public bool Selected
    {
        get => _selected;
        private set
        {
            if (_selected == value) return;
            if (!Selectable && value) return;

            _selected = value;
            GameNode.EventBus.Publish(new CardInterfaceSelectedEvent(this, _selected));
        }
    }

    private bool _pressed;
    public bool Pressed
    {
        get => _pressed;
        private set
        {
            if (_pressed == value) return;
            if (!Selectable && value) return;

            _pressed = value;
            GameNode.EventBus.Publish(new CardInterfacePressedEvent(this, _pressed));
        }
    }


    //Signal
    private void OnMouseEntered() => Selected = _preSelected = true;
    private void OnMouseExited() => Selected = _preSelected = false;
    private void OnButtonDown() => Pressed = _prePressed = true;
    private void OnButtonUp() => Pressed = _prePressed = false;
    private void OnPressed() => GameNode.EventBus.Publish(new CardInterfaceClickedEvent(this));


    //Events
    public override void _EnterTree()
    {
        SubscribeInstanceHandler(GameNode.EventBus);
    }

    public override void _ExitTree()
    {
        UnsubscribeInstanceHandler(GameNode.EventBus);
    }

    [EventHandler]
    private void OnCardUsed(CardUsedEvent e)
    {
        if (Card != e.Card) return;
        XPosTween?.Kill();
        YPosTween?.Kill();
        Selectable = false;
        var tween = CreateTween();
        tween.TweenProperty(this, "global_position", Project.ViewportSize / 2, 0.1f);
        //tween.TweenProperty(this, "position", new Vector2(200, -108), 0.1f);
        tween.TweenCallback(Callable.From(QueueFree)).SetDelay(0.15f); //temp, will be replaced by remove from tree
    }


    //Logging
    private static Logger Logger { get; } = LogManager.GetLogger(nameof(CardInterface));
}