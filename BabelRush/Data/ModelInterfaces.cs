using System.Collections.Generic;

using KirisameLib.Data.Model;

namespace BabelRush.Data;


public interface IDataModel<out TTarget> : IModel<IDictionary<string, object>, TTarget>;

public interface IResModel<out TTarget> : IModel<ResSource, TTarget>;

public interface ILangModel<out TTarget> : IModel<KeyValuePair<string, object>, TTarget>;