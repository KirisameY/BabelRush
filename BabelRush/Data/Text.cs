using System.Runtime.Serialization;

using Godot;

namespace BabelRush.Data;

[GlobalClass]
public partial class Text : Resource
{
    [Export] public string Content { get; set; } = "";
}