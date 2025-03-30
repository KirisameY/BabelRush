using System.Collections.Generic;
using System.Text;

using Godot;

namespace BabelRush.Data.ExtendModels;

public class ShaderIncludeModel(string id, string code) : IResModel<ShaderInclude>
{
    public (RegKey, ShaderInclude) Convert(string nameSpace, string path)
    {
        RegKey fid = (nameSpace, id);
        ShaderInclude result = new ShaderInclude();
        result.Code = code;
        return (fid, result);
    }

    public static IReadOnlyCollection<IModel<ShaderInclude>> FromSource(ResSourceInfo source, out ModelParseErrorInfo errorMessages)
    {
        if (!source.Files.TryGetValue(".gdshaderinc", out var gdshaderinc))
        {
            errorMessages = new ModelParseErrorInfo(1, [".gdshaderinc file not found"]);
            return [];
        }

        errorMessages = ModelParseErrorInfo.Empty;
        var code = Encoding.UTF8.GetString(gdshaderinc);
        var model = new ShaderIncludeModel(source.Path, code);
        return [model];
    }
}