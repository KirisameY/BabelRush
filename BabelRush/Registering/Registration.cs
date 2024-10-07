using System.Collections.Generic;

using KirisameLib.Core.Extensions;
using KirisameLib.Core.Logging;

namespace BabelRush.Registering;

public static class Registration
{
    #region RegisterTools Map

    private static Dictionary<string, AssetRegTool> RegToolMap { get; } = new();

    public static bool RegisterMap(string path, AssetRegTool tool) =>
        RegToolMap.TryAdd(path, tool);

    #endregion


    //Public Methods
    public static void RegisterAssets(string[] path, object assets)
    {
        switch (path)
        {
            case ["data", ..] or ["res", ..]:
                var pathStr = path.Join('/');
                if (RegToolMap.TryGetValue(pathStr, out var tool))
                    tool.RegisterSet(assets);
                break;
            case ["local", var local, .. var rest]
                when rest is ["lang", ..] or ["res", ..]:
                if (RegToolMap.TryGetValue(rest.Join('/'), out tool))
                    tool.RegisterLocalizedSet(local, assets);
                break;
            default:
                Logger.Log(LogLevel.Warning, nameof(RegisterAssets), $"Invalid asset path: {path.Join('/')}");
                break;
        }
    }


    //Logging
    private static Logger Logger { get; } = LogManager.GetLogger(nameof(Registration));
}