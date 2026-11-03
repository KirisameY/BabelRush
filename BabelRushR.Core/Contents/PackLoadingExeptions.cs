using KirisameY.Relinq.Extensions;

namespace BabelRushR.Core.Contents;

public class PackDependenciesNotSatisfiedException(IEnumerable<string> messages) : Exception(
    $"On or more package dependencies not satisfied: \n{messages.Join('\n')}");

public class PackCircularDependencyException(IEnumerable<string> messages) : Exception(
    $"Package circular dependencies found: \n{messages.Join('\n')}");

public class PackCircularOrderedException(IEnumerable<string> messages) : Exception(
    $"Package circular order rules found: \n{messages.Join('\n')}");