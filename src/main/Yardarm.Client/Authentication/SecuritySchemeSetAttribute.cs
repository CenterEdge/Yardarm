using System;
using Yardarm.Client.Internal;

namespace RootNamespace.Authentication
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SecuritySchemeSetAttribute : Attribute
    {
        private Type[] _securitySchemes;

        public Type[] SecuritySchemes
        {
            get => _securitySchemes;
            set
            {
                ThrowHelper.ThrowIfNull(value);

                _securitySchemes = value;
            }
        }

        public SecuritySchemeSetAttribute(params Type[] types)
        {
            ThrowHelper.ThrowIfNull(types);

            _securitySchemes = types;
        }
    }
}
