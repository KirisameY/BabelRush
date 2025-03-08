// using System;
// using System.Linq;
//
// using BabelRush.Data;
// using BabelRush.Mobs.Animation;
// using BabelRush.Registering.SourceTakers;
//
// namespace BabelRush.Registering.Misc;
//
// internal class MobAnimationDefaultRegistrant(MobAnimationSetRegister register) : ISourceTaker<ResSourceInfo>
// {
//     public (string id, Func<bool> register)[] Take(ResSourceInfo source, out ModelParseErrorInfo errorMessages)
//     {
//         var models = MobAnimationModel.FromSource(source, out errorMessages);
//         return models.Select(model => (model.Id, GetRegister(model.Convert()))).ToArray();
//     }
//
//     private Func<bool> GetRegister(MobAnimationModel model)
//     {
//         return () => register.RegisterDefault(model.Id, model);
//     }
// }
//
// internal class MobAnimationLocalizedRegistrant(MobAnimationSetRegister register) : ISourceTaker<ResSourceInfo>
// {
//     public (string id, Func<bool> register)[] Take(ResSourceInfo source, out ModelParseErrorInfo errorMessages)
//     {
//         var models = MobAnimationModel.FromSource(source, out errorMessages);
//         return models.Select(model => (model.Id, GetRegister(model.Convert()))).ToArray();
//     }
//
//     private Func<bool> GetRegister(MobAnimationModel model)
//     {
//         return () => register.RegisterLocal(FileLoader.CurrentLocal, model.Id, model);
//     }
// }

//todo: 这俩也需要重写