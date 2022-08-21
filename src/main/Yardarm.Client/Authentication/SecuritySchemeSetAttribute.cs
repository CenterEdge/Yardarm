using System;

namespace RootNamespace.Authentication
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SecuritySchemeSetAttribute : Attribute
    {
        private Type[] _securitySchemes;

        public Type[] SecuritySchemes
        {
            get => _securitySchemes;
            set => _securitySchemes = value ?? throw new ArgumentNullException(nameof(value));
        }

        public SecuritySchemeSetAttribute(params Type[] types)
        {
            _securitySchemes = types ?? throw new ArgumentNullException(nameof(types));
        }
    }
}
