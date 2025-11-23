using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using RootNamespace.Serialization.Literals.Converters;

namespace RootNamespace.Serialization.Literals;

/// <summary>
/// Registry for <see cref="LiteralConverter{T}"/> instances.
/// </summary>
/// <remarks>
/// This type is thread-safe for reads (Get/TryGet), but not for writes (AddValueType/AddReferenceType). It is recommended to set up the registry once and then only read from it.
/// </remarks>
public sealed class LiteralConverterRegistry
{
    /// <summary>
    /// Singleton instance of the registry.
    /// </summary>
    [field: MaybeNull]
    public static LiteralConverterRegistry Instance
    {
        get
        {
            var instance = field;
            if (instance is not null)
            {
                return instance;
            }

            // In case two threads are getting Instance for the first time at the same time,
            // use CompareExchange. One of the threads will not set the value, discarding the
            // CreateDefaultRegistry result, and both calls will get the same value.
            return Interlocked.CompareExchange(ref field, CreateDefaultRegistry(), null) ?? field;
        }
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            Volatile.Write(ref field!, value);
        }
    }

    private readonly Dictionary<Type, LiteralConverter> _converters = [];

    // A LiteralConverterRegistry is generally initialized once and then read many times, so using a FrozenDictionary
    // for reads becomes worthwhile for the slightly faster read performance.
    private FrozenDictionary<Type, LiteralConverter>? _frozenConverters;
    private FrozenDictionary<Type, LiteralConverter> FrozenConverters
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            FrozenDictionary<Type, LiteralConverter>? converters = _frozenConverters;
            if (converters is not null)
            {
                return converters;
            }

            return Interlocked.CompareExchange(ref _frozenConverters, _converters.ToFrozenDictionary(), null) ?? _frozenConverters;
        }
    }

    /// <summary>
    /// Gets a registered converter for the specified type.
    /// </summary>
    /// <typeparam name="T">Converter to get.</typeparam>
    /// <returns>The registered converter.</returns>
    /// <exception cref="KeyNotFoundException">No converter is registered for the type.</exception>
    public LiteralConverter<T> Get<T>()
    {
        if (TryGet(out LiteralConverter<T>? converter))
        {
            return converter;
        }

        ThrowHelper.ThrowKeyNotFoundException();
        return null!;
    }

    /// <summary>
    /// Gets a registered converter for the specified type.
    /// </summary>
    /// <typeparam name="T">Converter to get.</typeparam>
    /// <param name="converter">The registered converter, if found.</param>
    /// <returns>True if the converter was found, otherwise false.</returns>
    public bool TryGet<T>([NotNullWhen(true)] out LiteralConverter<T>? converter)
    {
        if (FrozenConverters.TryGetValue(typeof(T), out LiteralConverter? baseConverter))
        {
            converter = (LiteralConverter<T>)baseConverter;
            return true;
        }

        converter = null;
        return false;
    }

    /// <summary>
    /// Register a custom <see cref="LiteralConverter{T}"/> for a value type. Takes precedence over built-in converters and previously registered converters.
    /// </summary>
    /// <typeparam name="T">Type to register.</typeparam>
    /// <param name="converter">Converter for the type.</param>
    /// <param name="registerNullable">If true, automatically register a converter for <see cref="Nullable{T}"/> as well.</param>
    /// <returns>The <see cref="LiteralConverterRegistry"/> for chaining.</returns>
    public LiteralConverterRegistry Add<T>(LiteralConverter<T> converter, bool registerNullable = true)
        where T : struct
    {
        ArgumentNullException.ThrowIfNull(converter);
        Debug.Assert(typeof(T).IsValueType);

        _converters[typeof(T)] = converter;

        if (registerNullable && Nullable.GetUnderlyingType(typeof(T)) == null)
        {
            // registerNullable is true and T is not already a nullable type

            var nullableConverter = new NullableLiteralConverter<T>(converter);
            _converters[typeof(T?)] = nullableConverter;
        }

        _frozenConverters = null; // Clear the frozen dictionary to force a rebuild
        return this;
    }

    /// <summary>
    /// Register a custom <see cref="LiteralConverter{T}"/> for a reference type. Takes precedence over built-in converters and previously registered converters.
    /// </summary>
    /// <typeparam name="T">Type to register.</typeparam>
    /// <param name="converter">Converter for the type.</param>
    /// <returns>The <see cref="LiteralConverterRegistry"/> for chaining.</returns>
    public LiteralConverterRegistry Add<T>(LiteralConverter<T?> converter)
        where T : class?
    {
        ArgumentNullException.ThrowIfNull(converter);
        Debug.Assert(!typeof(T).IsValueType);

        _converters[typeof(T)] = converter;

        _frozenConverters = null; // Clear the frozen dictionary to force a rebuild
        return this;
    }

    /// <summary>
    /// Returns a new <see cref="LiteralConverterRegistry"/> with the built-in converters registered.
    /// </summary>
    /// <returns>A new <see cref="LiteralConverterRegistry"/>.</returns>
    public static LiteralConverterRegistry CreateBasicRegistry() =>
        new LiteralConverterRegistry()
            .Add(new BooleanLiteralConverter())
            .Add(new DateTimeLiteralConverter())
            .Add(new DateTimeOffsetLiteralConverter())
            .Add(new TimeSpanLiteralConverter())
            .Add(new GuidLiteralConverter())
            .Add(new ByteLiteralConverter())
            .Add(new SByteLiteralConverter())
            .Add(new Int16LiteralConverter())
            .Add(new UInt16LiteralConverter())
            .Add(new Int32LiteralConverter())
            .Add(new UInt32LiteralConverter())
            .Add(new Int64LiteralConverter())
            .Add(new UInt64LiteralConverter())
            .Add(new FloatLiteralConverter())
            .Add(new DoubleLiteralConverter())
            .Add(new DecimalLiteralConverter())
            .Add(new StringLiteralConverter())
            .Add(new UriLiteralConverter())
#if NET6_0_OR_GREATER
            .Add(new DateOnlyLiteralConverter())
            .Add(new TimeOnlyLiteralConverter())
#endif
            ;

    /// <summary>
    /// Returns a new <see cref="LiteralConverterRegistry"/> with the all default converters registered.
    /// </summary>
    /// <returns>A new <see cref="LiteralConverterRegistry"/>.</returns>
    public static LiteralConverterRegistry CreateDefaultRegistry()
    {
        LiteralConverterRegistry registry = CreateBasicRegistry();
        // Enrichment point
        return registry;
    }
}
