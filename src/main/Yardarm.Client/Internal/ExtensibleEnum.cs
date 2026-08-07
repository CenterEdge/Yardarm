using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using RootNamespace.Models;

namespace RootNamespace.Internal;

/// <summary>
/// Tools for working with <see cref="IExtensibleEnum{TSelf}"/>.
/// </summary>
internal static class ExtensibleEnum
{
    /// <summary>
    /// Creates a new instance of the extensible enumeration with the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the extensible enumeration.</typeparam>
    /// <param name="value">The value of the extensible enumeration.</param>
    /// <returns>A new instance of the extensible enumeration.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"> may not be null.</exception>'
    /// <remarks>
    /// For .NET 7 and later, the extensible enumeration type must implement the static abstract method <see cref="IExtensibleEnum{TSelf}.Create(string)"/>.
    /// For runtimes before .NET 7, the extensible enumeration type must have a public constructor that takes a single string parameter.
    /// </remarks>
    public static T Create<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(string value)
        where T : struct, IExtensibleEnum<T>
    {
        ArgumentNullException.ThrowIfNull(value);

#if NET7_0_OR_GREATER
        return T.Create(value);
#else
        return FactoryContainer<T>.Factory(value);
#endif
    }

#if !NET7_0_OR_GREATER

    // Static abstract interface methods are not supported before .NET 7, so use reflection to get the constructor instead.

    private static class FactoryContainer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>
        where T : struct, IExtensibleEnum<T>
    {
        public static readonly Func<string, T> Factory = CreateFactory();

        private static Func<string, T> CreateFactory()
        {
            ConstructorInfo? constructor = typeof(T).GetConstructor([typeof(string)]);
            if (constructor is null)
            {
                ThrowHelper.ThrowInvalidOperationException($"Type {typeof(T)} does not have a public constructor that takes a single string parameter.");
            }

#if NETCOREAPP3_0_OR_GREATER
            if (RuntimeFeature.IsDynamicCodeCompiled)
            {
#endif
                // More performant option that is dynamically compiled to avoid using reflection on each invocation
                ParameterExpression parameterExpression = Expression.Parameter(typeof(string), "value");

                NewExpression newExpression = Expression.New(
                    constructor,
                    parameterExpression);

                return Expression
                    .Lambda<Func<string, T>>(newExpression, parameterExpression)
                    .Compile();
#if NETCOREAPP3_0_OR_GREATER
            }
            else
            {
                // Fallback for scenarios where JIT is not supported
                return (value) => (T)constructor.Invoke([value]);
            }
#endif
        }
    }

#endif
}
