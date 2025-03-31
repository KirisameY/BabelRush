using Godot;

namespace BabelRush.Gui.DisplayInfos;

public static class SpriteInfoExtensions
{
    public static void ApplySpriteInfo(this Sprite2D sprite, SpriteInfo spriteInfo)
    {
        sprite.Texture = spriteInfo.Texture;
        sprite.Material = spriteInfo.Material;
        foreach (var (key, value) in spriteInfo.InstanceUniforms)
        {
            sprite.SetInstanceShaderParameter(key, value);
        }
    }

    public static void ApplySpriteInfo(this TextureRect sprite, SpriteInfo spriteInfo)
    {
        sprite.Texture = spriteInfo.Texture;
        sprite.Material = spriteInfo.Material;
        foreach (var (key, value) in spriteInfo.InstanceUniforms)
        {
            sprite.SetInstanceShaderParameter(key, value);
        }
    }

    public static void ApplySpriteInfoDeferred(this Sprite2D sprite, SpriteInfo spriteInfo)
    {
        sprite.Texture = spriteInfo.Texture;
        sprite.Material = spriteInfo.Material;
        foreach (var (key, value) in spriteInfo.InstanceUniforms)
        {
            sprite.SetInstanceShaderParameter(key, value);
        }
    }
}