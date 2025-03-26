using System.Collections.Generic;
using System.Linq;

using BabelRush.Data;

using JetBrains.Annotations;

using Tomlyn.Syntax;

namespace BabelRush.Scenery.Rooms;

[ModelSet("Room")]
public partial class RoomTemplateModel : IDataModel<RoomTemplate>
{
    [NecessaryProperty]
    public partial string Id { get; set; }
    [NecessaryProperty]
    public partial int Length { get; set; }
    [UsedImplicitly]
    public List<RoomObjInfo> Objects { get; set; } = [];


    public (RegKey, RoomTemplate) Convert(string nameSpace)
    {
        var id = RegKey.From(nameSpace, Id);
        var room = new RoomTemplate(id, Length, Objects.Select(o => (RoomObject.FromString(o.Obj), o.Position)));
        return (id, room);
    }

    public static IReadOnlyCollection<IModel<RoomTemplate>> FromSource(DocumentSyntax source, out ModelParseErrorInfo errorMessages) =>
        ModelUtils.ParseFromSource<ModelSet, RoomTemplate>(source, out errorMessages);

    partial void CustomCheck(List<string> errorList)
    {
        foreach (var objInfo in Objects)
        {
            if (objInfo.Check(out var errors)) continue;
            errorList.AddRange(errors);
            break;
        }
    }


    [UsedImplicitly] //todo: 老这样不是个事，有空针对嵌套模型类写个生成器
    public class RoomObjInfo
    {
        private string? _obj;
        public string Obj
        {
            get => _obj ?? throw new ModelDidNotInitializeException();
            set => _obj = value;
        }

        public double Position { get; set; }

        public bool Check(out string[] errors)
        {
            if (_obj is null)
            {
                errors = ["Obj type did not initialized"];
                return false;
            }
            errors = [];
            return true;
        }
    }
}