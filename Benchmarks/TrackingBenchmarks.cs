using BenchmarkDotNet.Attributes;
using TheSingularityWorkshop.DataWarehouse;
using Warehouse = TheSingularityWorkshop.DataWarehouse.DataWarehouse;

namespace TheSingularityWorkshop.Benchmarks.DataWarehouse.Benchmarks;

/// <summary>
/// Isolates handle liveness and dirty-state queries from record payload access.
/// Setup is excluded from measurement; the queried handle remains valid throughout each run.
/// </summary>
[MemoryDiagnoser]
public class TrackingQueryBenchmarks
{
    private const int RecordCount = 65_536;
    private Warehouse _warehouse = null!;
    private DataShelf<int> _shelf = null!;
    private DataHandle[] _handles = null!;
    private int _cursor;

    [GlobalSetup]
    public void Setup()
    {
        _warehouse = new Warehouse();
        _shelf = _warehouse.CreateShelf<int>(RecordCount);
        _handles = new DataHandle[RecordCount];

        for (var i = 0; i < RecordCount; i++)
        {
            if (!_shelf.TryAllocate(out _handles[i]))
                throw new InvalidOperationException("Unexpected shelf exhaustion during setup.");
            _shelf.GetRef(_handles[i]) = i;
        }

        // Make every record clean so IsDirty measures a stable false result.
        for (var i = 0; i < RecordCount; i++)
            _shelf.MarkClean(_handles[i]);
    }

    [Benchmark(Description = "Tracking.IsAlive.ValidHandle")]
    public bool IsAliveValidHandle() => _shelf.IsAlive(NextHandle());

    [Benchmark(Description = "Tracking.IsDirty.CleanHandle")]
    public bool IsDirtyCleanHandle() => _shelf.IsDirty(NextHandle());

    [Benchmark(Description = "Tracking.MarkCleanAndRedirty")]
    public bool MarkCleanAndRedirty()
    {
        var handle = NextHandle();
        var value = _shelf.Read(handle);
        _shelf.GetRef(handle) = value;
        return _shelf.MarkClean(handle);
    }

    [GlobalCleanup]
    public void Cleanup() => _warehouse.Dispose();

    private DataHandle NextHandle()
    {
        var index = _cursor++;
        if (_cursor == RecordCount) _cursor = 0;
        return _handles[index];
    }
}

/// <summary>
/// Measures dirty-slot enumeration at controlled dirty densities.
/// The output buffer is allocated before measurement and is large enough for all dirty slots.
/// </summary>
[MemoryDiagnoser]
public class DirtyEnumerationBenchmarks
{
    private const int RecordCount = 65_536;
    private Warehouse _warehouse = null!;
    private DataShelf<int> _shelf = null!;
    private int[] _destination = null!;

    [Params(0, 1, 50, 100)]
    public int DirtyPercent { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _warehouse = new Warehouse();
        _shelf = _warehouse.CreateShelf<int>(RecordCount);
        _destination = new int[RecordCount];
        var handles = new DataHandle[RecordCount];

        for (var i = 0; i < RecordCount; i++)
        {
            if (!_shelf.TryAllocate(out handles[i]))
                throw new InvalidOperationException("Unexpected shelf exhaustion during setup.");
            _shelf.GetRef(handles[i]) = i;
        }

        var dirtyCount = RecordCount * DirtyPercent / 100;
        for (var i = dirtyCount; i < RecordCount; i++)
            _shelf.MarkClean(handles[i]);
    }

    [Benchmark(Description = "Tracking.CopyDirtySlots.Density")]
    public int CopyDirtySlots() => _shelf.CopyDirtySlots(_destination);

    [GlobalCleanup]
    public void Cleanup() => _warehouse.Dispose();
}
