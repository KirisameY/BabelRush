using System.Diagnostics.CodeAnalysis;

using BabelRush.Cards;
using BabelRush.I18n;
using BabelRush.Registers;

using Godot;

using KirisameLib.Event;

namespace BabelRush.Gui.Screens.Cards;

[EventHandlerContainer]
public partial class CardInfoText : Control
{
    //Sub Nodes
    [field: AllowNull, MaybeNull]
    private Node Label => field ??= GetNode("RichTextLabel");


    //StringNames
    private static class Names
    {
        //Methods
        public static readonly StringName Reset = "reset";
        public static readonly StringName AppendTitle = "append_title";
        public static readonly StringName AppendAction = "append_action";
        public static readonly StringName AppendFeature = "append_feature";
        public static readonly StringName AppendFooter = "append_footer";

        //Properties
        public static readonly StringName TitleFont = "title_font";
        public static readonly StringName TitleSize = "title_size";
        public static readonly StringName SubtitleFont = "subtitle_font";
        public static readonly StringName SubtitleSize = "subtitle_size";
        public static readonly StringName DetailFont = "detail_font";
        public static readonly StringName DetailSize = "detail_size";
    }


    //Update
    private bool _ready = false;

    private void UpdateFont()
    {
        var titleInfo = LocalInfoRegisters.FontInfo.GetItem("title");
        Label.Set(Names.TitleFont, titleInfo.Font);
        Label.Set(Names.TitleSize, titleInfo.Size);
        var subtitleInfo = LocalInfoRegisters.FontInfo.GetItem("subtitle");
        Label.Set(Names.SubtitleFont, subtitleInfo.Font);
        Label.Set(Names.SubtitleSize, subtitleInfo.Size);
        var detailInfo = LocalInfoRegisters.FontInfo.GetItem("detail");
        Label.Set(Names.DetailFont, detailInfo.Font);
        Label.Set(Names.DetailSize, detailInfo.Size);
    }


    //Scene Tree
    public override void _Ready()
    {
        UpdateFont();
        _ready = true;
    }

    public override void _EnterTree()
    {
        SubscribeInstanceHandler(Game.EventBus);
        if (_ready) UpdateFont();
    }

    public override void _ExitTree()
    {
        UnsubscribeInstanceHandler(Game.EventBus);
    }


    //Public Methods
    public void SetCard(Card card)
    {
        Label.Call(Names.Reset);
        Label.Call(Names.AppendTitle, card.Type.NameDesc.Name, card.Cost);
        foreach (var feature in card.Features)
        {
            var (name, desc) = feature.Type.NameDesc;
            Label.Call(Names.AppendFeature, feature.Type.Icon, name, feature.Value, desc);
        }
        foreach (var action in card.Actions)
        {
            var (name, desc) = action.Type.NameDesc;
            Label.Call(Names.AppendAction, action.Type.Icon, name, action.Value, desc);
        }
        Label.Call(Names.AppendFooter);
    }

    public void Clear() => Label.Call(Names.Reset);


    //EventHandlers
    [EventHandler]
    private void OnLocalChanged(LocalChangedEvent e)
    {
        UpdateFont();
    }
}