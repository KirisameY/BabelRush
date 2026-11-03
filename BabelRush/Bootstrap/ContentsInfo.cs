namespace BabelRushR.Game.Bootstrap;

public class ContentsInfo
{
    // temp
    public const string RelativePackPath =
    #if TOOLS
        "../Contents";
    #else
        "./Contents";
    #endif
}