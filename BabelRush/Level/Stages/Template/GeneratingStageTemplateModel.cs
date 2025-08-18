using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using BabelRush.Data;
using BabelRush.Level.Stages.Template.GenerationRules;

using JetBrains.Annotations;

namespace BabelRush.Level.Stages.Template;

[Model] [UsedImplicitly]
public partial class GeneratingStageTemplateModel : StageTemplateModel
{
    [NecessaryProperty]
    public partial string StartRoom { get; set; }

    [NecessaryProperty]
    public partial int Length { get; set; }

    [NecessaryProperty]
    public partial int Parallels { get; set; }

    [NecessaryProperty]
    public partial int Paths { get; set; }

    [UsedImplicitly]
    public List<RoomWithWeightModel> Rooms { get; set; } = [];

    [UsedImplicitly]
    public List<SeparationRuleModel> SeparationRules { get; set; } = [];

    [UsedImplicitly]
    public List<AfterRuleModel> AfterRules { get; set; } = [];


    public override (RegKey, StageTemplate) Convert(string nameSpace, string path)
    {
        RegKey id = (nameSpace, Id);
        var startRoomId = StartRoom.WithDefaultNameSpace(nameSpace);
        var rooms = Rooms.Select(m => (m.Room.WithDefaultNameSpace(nameSpace), m.Weight)).ToImmutableArray();
        var separationRules = SeparationRules.Select(m => m.Convert()).ToImmutableArray();
        var afterRules = AfterRules.Select(m => m.Convert(nameSpace)).ToImmutableArray();

        return (id, new GeneratingStageTemplate(id, startRoomId, Length, Parallels, Paths, rooms, separationRules, afterRules));
    }

    partial void CustomCheck(List<string> errorList)
    {
        if (Length <= 0) errorList.Add("Length is not greater than 0");
        if (Parallels <= 0) errorList.Add("Parallels is not greater than 0");
        if (Paths <= 0) errorList.Add("Paths is not greater than 0");
        if (Rooms.Count == 0) errorList.Add("Rooms is empty");
        foreach (var room in Rooms)
        {
            room.Check(errorList);
        }
        foreach (var rule in SeparationRules)
        {
            if (!rule.Check(out var errs)) errorList.AddRange(errs);
        }
        foreach (var rule in AfterRules)
        {
            if (!rule.Check(out var errs)) errorList.AddRange(errs);
        }
    }

    [UsedImplicitly]
    public class RoomWithWeightModel
    {
        private bool _roomSet = false;
        private bool _weightSet = false;

        [UsedImplicitly]
        public string Room
        {
            get;
            set
            {
                _roomSet = true;
                field    = value;
            }
        } = "";

        [UsedImplicitly]
        public double Weight
        {
            get;
            set
            {
                _weightSet = true;
                field      = value;
            }
        }

        public void Check(List<string> errors)
        {
            if (!_roomSet) errors.Add("Property Room of RoomWithWeightModel did not initialized");
            if (!_weightSet) errors.Add("Property Weight of RoomWithWeightModel did not initialized");
        }
    }
}