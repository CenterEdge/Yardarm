using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using RootNamespace.Serialization.Literals.Converters;
using Yardarm.Client.Internal;

namespace RootNamespace.Serialization.Literals;

/// <summary>
/// Registry for <see cref="LiteralConverter{T}"/> instances.
/// </summary>
/// <remarks>
/// This type is thread-safe for reads (Get/TryGet), but not for writes (AddValueType/AddReferenceType). It is recommended to set up the registry once and then only read from it.
/// </remarks>
public sealed class LiteralConverterRegistry
{
    private static LiteralConverterRegistry? s_instance;

    /// <summary>
    /// Singleton instance of the registry.
    /// </summary>
    public static LiteralConverterRegistry Instance
    {
        get
        {
            var instance = s_instance;
            if (instance is not null)
            {
                return instance;
            }

            // In case two threads are getting Instance for the first time at the same time,
            // use CompareExchange. One of the threads will not set the value, discarding the
            // CreateDefaultRegistry result, and both calls will get the same value.
            return Interlocked.CompareExchange(ref s_instance, CreateDefaultRegistry(), null) ?? s_instance;
        }
        set
        {
            ThrowHelper.ThrowIfNull(value);

            Volatile.Write(ref s_instance, value);
        }
    }

    private readonly Dictionary<Type, LiteralConverter> _converters = new();

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
    public LiteralConverterRegistry AddValueType<T>(LiteralConverter<T> converter, bool registerNullable = true)
        where T : struct
    {
        ThrowHelper.ThrowIfNull(converter);

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
    public LiteralConverterRegistry AddReferenceType<T>(LiteralConverter<T> converter)
    {
        ThrowHelper.ThrowIfNull(converter);

        _converters[typeof(T)] = converter;

        _frozenConverters = null; // Clear the frozen dictionary to force a rebuild
        return this;
    }

    /// <summary>
    /// Returns a new <see cref="LiteralConverterRegistry"/> with the all default converters registered.
    /// </summary>
    /// <returns>A new <see cref="LiteralConverterRegistry"/>.</returns>
    public static LiteralConverterRegistry CreateDefaultRegistry() =>
        new LiteralConverterRegistry()
            .AddValueType(new BooleanLiteralConverter())
            .AddValueType(new DateTimeLiteralConverter())
            .AddValueType(new DateTimeOffsetLiteralConverter())
            .AddValueType(new TimeSpanLiteralConverter())
            .AddValueType(new GuidLiteralConverter())
            .AddValueType(new ByteLiteralConverter())
            .AddValueType(new SByteLiteralConverter())
            .AddValueType(new Int16LiteralConverter())
            .AddValueType(new UInt16LiteralConverter())
            .AddValueType(new Int32LiteralConverter())
            .AddValueType(new UInt32LiteralConverter())
            .AddValueType(new Int64LiteralConverter())
            .AddValueType(new UInt64LiteralConverter())
            .AddValueType(new FloatLiteralConverter())
            .AddValueType(new DoubleLiteralConverter())
            .AddValueType(new DecimalLiteralConverter())
            .AddReferenceType(new StringLiteralConverter())
            .AddReferenceType(new UriLiteralConverter());
}
