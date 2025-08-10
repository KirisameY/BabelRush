using System.Collections.Generic;

using Godot;

namespace BabelRush.Level.Stages.Template.GenerationRuless;

public abstract class AfterRule
{
    public abstract void Process(Dictionary<Vector2I, StageNode> nodes);
}