using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using RootNamespace.Models;

namespace RootNamespace.Internal;

internal static class ExternallyDiscriminatedUnion<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>
    where T : IUnion
{
    public sealed class CaseInfo(string caseName, Type caseType, Func<object, T> factory)
    {
        public readonly string CaseName = caseName;
        public readonly Type CaseType = caseType;
        public readonly Func<object, T> Factory = factory;
    }

    public static FrozenDictionary<string, CaseInfo> CasesByName { get; }
    public static FrozenDictionary<Type, CaseInfo> CasesByType { get; }
    public static Func<object, T>? UnknownCaseFactory { get; }

    static ExternallyDiscriminatedUnion()
    {
        var casesByName = new Dictionary<string, CaseInfo>();
        var casesByType = new Dictionary<Type, CaseInfo>();

        foreach (ConstructorInfo constructor in typeof(T).GetConstructors(BindingFlags.Public | BindingFlags.Instance))
        {
            ParameterInfo[] parameters = constructor.GetParameters();
            if (parameters.Length == 1 && parameters[0] is { ParameterType: Type parameterType })
            {
                if (parameterType == typeof(UnknownCase))
                {
                    UnknownCaseFactory = CreateCaseFactory(constructor, parameterType);
                }
                else
                {
                    var attribute = constructor.GetCustomAttribute<UnionCaseNameAttribute>();
                    if (attribute is null)
                    {
                        continue;
                    }

                    var caseInfo = new CaseInfo(attribute.CaseName, parameterType,
                        CreateCaseFactory(constructor, parameterType));

                    casesByName[attribute.CaseName] = caseInfo;
                    casesByType[parameterType] = caseInfo;
                }
            }
        }

        CasesByName = casesByName.ToFrozenDictionary();
        CasesByType = casesByType.ToFrozenDictionary();
    }

    private static Func<object, T> CreateCaseFactory(ConstructorInfo constructor, Type parameterType)
    {
        ArgumentNullException.ThrowIfNull(constructor);
        ArgumentNullException.ThrowIfNull(parameterType);

#if NETCOREAPP3_0_OR_GREATER
        if (RuntimeFeature.IsDynamicCodeCompiled)
        {
#endif
            // More performant option that is dynamically compiled to avoid using reflection on each invocation
            ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "value");

            NewExpression newExpression = Expression.New(
                constructor,
                Expression.Convert(parameterExpression, parameterType));

            return Expression
                .Lambda<Func<object, T>>(newExpression, parameterExpression)
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
