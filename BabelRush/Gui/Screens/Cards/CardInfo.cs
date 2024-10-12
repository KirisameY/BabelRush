using Godot;

using System;

using BabelRush.Cards;

public partial class CardInfo : Control
{
    //Sub Nodes
    private Node? _label;
    private Node Label => _label ??= GetNode("RichTextLabel");


    //StringNames
    private StringName StringNameReset { get; } = "reset";
    private StringName StringNameAppendTitle { get; } = "append_title";
    private StringName StringNameAppendAction { get; } = "append_action";
    private StringName StringNameAppendFeature { get; } = "append_feature";
    private StringName StringNameAppendFooter { get; } = "append_footer";


    //Public Methods
    public void SetCard(Card card)
    {
        Label.Call(StringNameReset);
        Label.Call(StringNameAppendTitle, card.Type.NameDesc.Name, card.Cost);
        foreach (var feature in card.Features)
        {
            var (name, desc) = feature.Type.NameDesc;
            Label.Call(StringNameAppendFeature, feature.Type.Icon, name, feature.Value, desc);
        }
        foreach (var action in card.Actions)
        {
            var (name, desc) = action.Type.NameDesc;
            Label.Call(StringNameAppendAction, action.Type.Icon, name, action.Value, desc);
        }
        Label.Call(StringNameAppendFooter);
    }
}