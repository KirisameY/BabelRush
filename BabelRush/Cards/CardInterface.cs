using Godot;

namespace BabelRush.Cards;

public partial class CardInterface : Node2D
{
    //Factory
    private CardInterface() { }

    public static CardInterface CreateInstance()
    {
        var instance = Scene.Instantiate<CardInterface>();

        return instance;
    }

    private const string ScenePath = "res://Cards/Card.tscn";
    private static PackedScene Scene { get; } = ResourceLoader.Load<PackedScene>(ScenePath);

    //Sub nodes
    private TextureRect? _iconNode;
    private TextureRect IconNode => _iconNode ??= GetNode<TextureRect>("Icon");

    private Label? _titleNode;
    private Label TitleNode => _titleNode ??= GetNode<Label>("Title");

    private Label? _costNode;
    private Label CostNode => _costNode ??= GetNode<Label>("Cost");

    private Sprite2D? _frameNode;
    private Sprite2D FrameNode => _frameNode ??= GetNode<Sprite2D>("Frame");

    //Property
    private ICard? _card;
    public ICard? Card
    {
        get => _card;
        set
        {
            _card = value;
            Refresh();
        }
    }

    //Update
    private void Refresh()
    {
        if (Card is null)
        {
            Visible = false;
            return;
        }

        Visible = true;
        IconNode.Texture = Card.Type.Icon;
        TitleNode.Text = Card.Type.Name;
        CostNode.Text = Card.Type.Cost.ToString();
    }
}