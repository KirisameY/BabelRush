using KirisameY.Registration.Data;

using Tomlyn.Serialization;

namespace BabelRushR.Core.Card.Types;

[TomlPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[TomlDerivedType(typeof(DelayedRecordCardType))]
public interface ICardType
{
    RegKey ID { get; }

    ICard CreateCard();
}