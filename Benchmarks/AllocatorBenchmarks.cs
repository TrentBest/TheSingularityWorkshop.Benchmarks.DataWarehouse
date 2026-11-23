using BenchmarkDotNet.Attributes;
using TheSingularityWorkshop.DataWarehouse;
using Warehouse = TheSingularityWorkshop.DataWarehouse.DataWarehouse;

namespace TheSingularityWorkshop.Benchmarks.DataWarehouse.Benchmarks;

/// <summary>
/// Measures shared-lake page-run allocation and reclamation while retaining the backing region
/// for reuse between invocations.
/// </summary>
[MemoryDiagnoser]
public class SharedLakeShelfAllocationBenchmarks
{
    [Params(1, 16, 128)]
    public int ShelfCount { get; set; }

    private Warehouse _warehouse = null!;
    private DataShelf<Payload>[] _shelves = null!;

    [GlobalSetup]
    public void Setup()
    {
        _warehouse = new Warehouse(defaultPageSize: 4096, defaultRegionSize: 4 * 1024 * 1024);
        _shelves = new DataShelf<Payload>[ShelfCount];
    }

    [Benchmark(Description = "DWH-LAKE-SHARED-SHELF-ALLOCATE-FREE")]
    public long AllocateAndFreeShelfSet()
    {
        for (var i = 0; i < _shelves.Length; i++)
            _shelves[i] = _warehouse.CreateShelf<Payload>(capacity: 256);

        var activeBytes = _warehouse.Lake.AllocatedBytes;

        for (var i = _shelves.Length - 1; i >= 0; i--)
        {
            _shelves[i].Dispose();
            _shelves[i] = null!;
        }

        return activeBytes;
    }

    [GlobalCleanup]
    public void Cleanup() => _warehouse.Dispose();

    public struct Payload
    {
        public long A;
        public long B;
    }
}

/// <summary>
/// Measures an end-to-end shelf growth operation with live records. Each invocation creates a
/// fresh shelf, populates its initial capacity, grows it, and disposes it.
/// </summary>
[MemoryDiagnoser]
public class ShelfGrowthBenchmarks
{
    [Params(64, 1024)]
    public int InitialCapacity { get; set; }

    [Params(4096, 65536)]
    public int TargetCapacity { get; set; }

    private Warehouse _warehouse = null!;

    [GlobalSetup]
    public void Setup() =>
        _warehouse = new Warehouse(defaultPageSize: 4096, defaultRegionSize: 4 * 1024 * 1024);

    [Benchmark(Description = "DWH-GROWTH-POPULATE-AND-ENSURE-CAPACITY")]
    public int PopulateAndGrowShelf()
    {
        using var shelf = _warehouse.CreateShelf<Payload>(InitialCapacity);

        for (var i = 0; i < InitialCapacity; i++)
        {
            if (!shelf.TryAllocate(out var handle))
                throw new InvalidOperationException("Unexpected shelf exhaustion during growth setup.");
            shelf.GetRef(handle).Value = i;
            if ((i & 1) == 0)
                shelf.MarkClean(handle);
        }

        shelf.EnsureCapacity(TargetCapacity);
        return shelf.Capacity;
    }

    [GlobalCleanup]
    public void Cleanup() => _warehouse.Dispose();

    public struct Payload
    {
        public long Value;
    }
}

/// <summary>Measures creation and disposal of a new native backing region.</summary>
[MemoryDiagnoser]
public class NativeRegionLifecycleBenchmarks
{
    [Benchmark(Description = "DWH-LAKE-REGION-CREATE-RELEASE")]
    public long CreateAndReleaseRegion()
    {
        using var warehouse = new Warehouse(defaultPageSize: 4096, defaultRegionSize: 4096);
        using var shelf = warehouse.CreateShelf<Payload>(capacity: 256);
        return warehouse.Lake.ReservedBytes;
    }

    public struct Payload
    {
        public long A;
        public long B;
    }
}
