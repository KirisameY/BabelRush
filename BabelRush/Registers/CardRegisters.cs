using BabelRush.Cards;
using BabelRush.Data;
using BabelRush.Data.ExtendModels;
using BabelRush.Registering;

using Godot;

using KirisameLib.Data.Registers;

namespace BabelRush.Registers;

[RegisterContainer]
public static class CardRegisters
{
    public static IRegister<NameDesc> CardNameDesc { get; } =
        CreateSimpleRegister.Lang<NameDesc, NameDescModel>("cards", "en", id => (id, ""));

    public static IRegister<Texture2D> CardIcon { get; } =
        CreateSimpleRegister.Res<Texture2D, Texture2DModel>("textures/cards", new PlaceholderTexture2D());

    public static IRegister<CardType> Cards { get; } =
        CreateSimpleRegister.Data<CardType, CardTypeModel>("cards", CardType.Default);
}