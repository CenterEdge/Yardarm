using System;

namespace RootNamespace.Authentication;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class SecuritySchemeSetAttribute : Attribute
{
    public Type[] SecuritySchemes
    {
        get => field;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            field = value;
        }
    }

    public SecuritySchemeSetAttribute(params Type[] types)
    {
        ArgumentNullException.ThrowIfNull(types);

        SecuritySchemes = types;
    }
}
