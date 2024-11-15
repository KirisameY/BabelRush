using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using BabelRush.Data;
using BabelRush.Registers;

using JetBrains.Annotations;

using KirisameLib.Core.Extensions;
using KirisameLib.Data.Model;

using Tomlyn;
using Tomlyn.Syntax;

namespace BabelRush.Actions;

[ModelSet("ActionStep")] [UsedImplicitly]
public partial class ActionStepModel : IDataModel<ActionStep>
{
    [NecessaryProperty]
    public partial string Id { get; set; }
    [NecessaryProperty]
    public partial string ActionDelegate { get; set; }
    
    public ActionStep Convert()
    {
        var actionDelegate = InCodeRegisters.ActionDelegates.GetItem(ActionDelegate);
        return new(actionDelegate);
    }

    public static IReadOnlyCollection<IModel<ActionStep>> FromSource(DocumentSyntax source, out ModelParseErrorInfo errorMessages) =>
        IDataModel<ActionStep>.ParseFromSource<ModelSet>(source, out errorMessages);
}