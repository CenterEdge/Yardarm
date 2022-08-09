﻿using System.Net.Http;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization
{
    public interface ITypeSerializer
    {
        HttpContent Serialize<T>(T value, string mediaType);

        ValueTask<T> DeserializeAsync<T>(HttpContent content);
    }
}
