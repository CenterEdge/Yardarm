using RootNamespace.Serialization;

namespace Yardarm.Benchmarks;

[MemoryDiagnoser]
public class PathSerializerInterpolation
{
    long BusinessLocationId { get; set; } = 123456;
    Guid CustomerId { get; set; } = Guid.NewGuid();

    [Benchmark(Baseline = true)]
    public string Serialize()
    {
        // We don't use stackalloc for an initial buffer here because this PathSegmentSerializer.Serialize overload returns intermediate strings,
        // so this is implemented as a call to string.Concat which is faster than using DefaultInterpolatedStringHandler.
        return $"org/{PathSegmentSerializer.Serialize("id", BusinessLocationId, PathSegmentStyle.Simple, "int64")}/customers/{PathSegmentSerializer.Serialize("customerId", CustomerId, PathSegmentStyle.Simple, "uuid")}";
    }

    [Benchmark]
    public string Build()
    {
        Span<char> initialBuffer = stackalloc char[256];
        return PathSegmentSerializer.Build(initialBuffer, $"org/{BusinessLocationId:int64}/customers/{CustomerId:uuid}");
    }
}
