using System.Diagnostics.CodeAnalysis;

using BabelRush.Cards;

using Godot;

namespace BabelRush.Gui.Screens.Cards;

public partial class CardInfoText : Control
{
    //Sub Nodes
    [field: AllowNull, MaybeNull]
    private Node Label => field ??= GetNode("RichTextLabel");


    //StringNames
    private static class Names
    {
        public static StringName Reset { get; } = "reset";
        public static StringName AppendTitle { get; } = "append_title";
        public static StringName AppendAction { get; } = "append_action";
        public static StringName AppendFeature { get; } = "append_feature";
        public static StringName AppendFooter { get; } = "append_footer";
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
}