using System.Collections.Generic;

using BabelRush.Registering.SourceTakers;

namespace BabelRush.Registering.I18n;

// ReSharper disable once InconsistentNaming
public interface II18nSourceTakerFactory<T>
{
    public IEnumerable<(string Local, ISourceTaker<T> SourceTaker)> CreateSourceTakers(string local);
}