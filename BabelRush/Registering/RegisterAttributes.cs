using System;

namespace BabelRush.Registering;

[AttributeUsage(AttributeTargets.Class)]
public class RegisterContainerAttribute : Attribute;

[AttributeUsage(AttributeTargets.Property)]
public class RegistrationMapAttribute : Attribute;