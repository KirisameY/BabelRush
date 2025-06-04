using System.Collections;
using System.Collections.Generic;
using System.Linq;

using BabelRush.Data;

using JetBrains.Annotations;

namespace BabelRush.Scenery.Stages.Template;

[Model] [UsedImplicitly]
public partial class ConstantStageTemplateModel : StageTemplateModel
{
    [UsedImplicitly]
    public List<StageNodeModel> StageNodes { get; set; } = [];

    [UsedImplicitly]
    public uint FirstRoom { get; set; } = 0;

    [UsedImplicitly]
    public List<PathModel> Paths { get; set; } = [];

    public override (RegKey, StageTemplate) Convert(string nameSpace, string path)
    {
        RegKey id = (nameSpace, Id);

        var paths = Paths.GroupBy(p => p.From, p => p.To)
                         .ToDictionary(p => p.Key, p => p.ToArray());

        Stack<(uint index, StageNodeModel node)> pendingStack = [];
        Dictionary<uint, StageNode> nodes = [];
        pendingStack.Push((FirstRoom, StageNodes[(int)FirstRoom]));

        while (pendingStack.TryPeek(out var pending))
        {
            var ready = true;
            foreach (var nextIndex in paths[pending.index])
            {
                if (nodes.ContainsKey(nextIndex))
                {
                    ready = false;
                    pendingStack.Push((nextIndex, StageNodes[(int)nextIndex]));
                }
            }

            if (!ready) continue;
            nodes[pending.index] = pending.node.Convert(nameSpace, paths[pending.index].Select(i => nodes[i]));
            pendingStack.Pop();
        }

        return (id, new ConstantStageTemplate(id, nodes[FirstRoom]));
    }

    partial void CustomCheck(List<string> errorList)
    {
        foreach (var stageNode in StageNodes)
        {
            stageNode.Check(out var errors);
            errorList.AddRange(errors);
        }

        if (FirstRoom >= StageNodes.Count)
            errorList.Add("FirstRoom is out of range");

        foreach (var (index, path) in Paths.Index())
        {
            if (path.From >= StageNodes.Count || path.To >= StageNodes.Count)
                errorList.Add($"Path[{index}] is out of range");
        }
    }


    #region Sub-model class

    [UsedImplicitly]
    public class PathModel
    {
        public uint From { get; set; }
        public uint To { get; set; }
    }

    #endregion
}