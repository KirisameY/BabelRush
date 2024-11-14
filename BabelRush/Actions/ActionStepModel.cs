using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using BabelRush.Data;
using BabelRush.Registers;

using KirisameLib.Core.Extensions;
using KirisameLib.Data.Model;

using Tomlyn;
using Tomlyn.Syntax;

namespace BabelRush.Actions;

[ModelSet("ActionStep")]
public partial class ActionStepModel : IDataModel<ActionStep>
{
    [NecessaryProperty]
    public partial string Id { get; set; } = "";
    [NecessaryProperty]
    public partial string ActionDelegate { get; set; } = "";

    public ActionStep Convert()
    {
        var actionDelegate = InCodeRegisters.ActionDelegates.GetItem(ActionDelegate);
        return new(actionDelegate);
    }

    // public static bool FromEntry(IDictionary<string, object> entry,
    //                              [MaybeNullWhen(false)] out ActionStepModel model,
    //                              [NotNullWhen(false)] out string? errorMessage)
    // {
    //     model = null;
    //     errorMessage = null;
    //
    //     try
    //     {
    //         var id = (string)entry["id"];
    //         var actionDelegate = (string)entry["action_delegate"];
    //         model = new(id, actionDelegate);
    //     }
    //     catch (Exception e)
    //     {
    //         errorMessage = e.ToString();
    //     }
    //
    //     return errorMessage is null;
    // }

    // public static IModel<ActionStep>[] FromSource(IDictionary<string, object> source, out ModelParseErrorInfo errorMessages)
    // {
    //     //todo:迟早要重写成直接反序列化，现阶段别管这部分代码了
    //     if (!source.TryGetValue("action_delegates", out var item))
    //     {
    //         errorMessages = new(-1, ["table 'action_delegates' not found in file"]);
    //         return [];
    //     }
    //     if (item is not IList<IDictionary<string, object>> entries)
    //     {
    //         errorMessages = new(-1, ["table 'action_delegates' is not an table array"]);
    //         return [];
    //     }
    //
    //     var models = new List<IModel<ActionStep>>();
    //     var errorMessagesList = new List<string>();
    //     foreach (var entry in entries)
    //     {
    //         if (FromEntry(entry, out var model, out var errorMessage))
    //             models.Add(model);
    //         else
    //             errorMessagesList.Add(errorMessage);
    //     }
    //
    //     errorMessages = new(errorMessagesList.Count, errorMessagesList.ToArray());
    //     return models.ToArray();
    // }

    public static IReadOnlyCollection<IModel<ActionStep>> FromSource(DocumentSyntax source, out ModelParseErrorInfo errorMessages)
    {
        source.TryToModel<ModelSet>(out var model, out var diagnostics);
        string[] checkErrors = [];
        IReadOnlyCollection<ActionStepModel> result = model?.CheckAll(out checkErrors) ?? [];
        var errors = diagnostics.Select(msg => msg.ToString()).Concat(checkErrors).ToArray();
        errorMessages = new(errors.Length, errors);
        return result;
    }

    public void Deconstruct(out string id, out string actionDelegate)
    {
        id = Id;
        actionDelegate = ActionDelegate;
    }
}