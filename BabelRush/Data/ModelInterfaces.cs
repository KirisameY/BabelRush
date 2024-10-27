using System.Collections.Generic;

using KirisameLib.Data.Model;

namespace BabelRush.Data;

public interface IDataModel<out TTarget> : IModel<byte[], TTarget>;

public interface IResModel<out TTarget> : IModel<ResSourceInfo, TTarget>;

public interface ILangModel<out TTarget> : IModel<IDictionary<string, object>, TTarget>;