namespace BabelRush.Registering.I18n;

// ReSharper disable once InconsistentNaming
public interface II18nRegistrant<TItem>
{
    public void AcceptRegister(II18nRegTarget<TItem> register);
}