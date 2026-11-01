namespace BabelRushR.Game.Bootstrap;

public class ResourcesInfo
{
    // temp
    public const string RelativePackPath =
    #if TOOLS
        "../Contents";
    #else
        "./Contents";
    #endif
}