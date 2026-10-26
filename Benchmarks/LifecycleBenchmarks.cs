using BenchmarkDotNet.Attributes;
using TheSingularityWorkshop.DataWarehouse;
using Warehouse = TheSingularityWorkshop.DataWarehouse.DataWarehouse;

namespace TheSingularityWorkshop.Benchmarks.DataWarehouse.Benchmarks;

[MemoryDiagnoser]
public class LifecycleBenchmarks
{
    private Warehouse _warehouse = null!;
    private DataShelf<Payload> _shelf = null!;
    private readonly Dictionary<int, Payload> _dictionary = new();
    private int _nextKey;

    [GlobalSetup]
    public void Setup()
    {
        _warehouse = new Warehouse();
        _shelf = _warehouse.CreateShelf<Payload>(1024);
    }

    [Benchmark(Description = "DataShelf.AllocateAndFree")]
    public int DataShelfAllocateAndFree()
    {
        if (!_shelf.TryAllocate(out var handle))
            throw new InvalidOperationException("Unexpected shelf exhaustion.");
        _shelf.GetRef(handle).Value = handle.Slot;
        _shelf.TryFree(handle);
        return handle.Slot;
    }

    [Benchmark(Description = "Dictionary.AddAndRemove")]
    public int DictionaryAddAndRemove()
    {
        var key = _nextKey++;
        _dictionary.Add(key, new Payload { Value = key });
        _dictionary.Remove(key);
        return key;
    }

    [GlobalCleanup]
    public void Cleanup() => _warehouse.Dispose();

    public struct Payload
    {
        public int Value;
    }
}
