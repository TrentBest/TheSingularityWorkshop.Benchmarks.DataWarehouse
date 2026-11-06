using BenchmarkDotNet.Attributes;
using TheSingularityWorkshop.DataWarehouse;
using Warehouse = TheSingularityWorkshop.DataWarehouse.DataWarehouse;

namespace TheSingularityWorkshop.Benchmarks.DataWarehouse.Benchmarks;

[MemoryDiagnoser]
public class AccessReadBenchmarks
{
    private const int RecordCount = 65_536;
    private Payload[] _array = null!;
    private Dictionary<int, Payload> _dictionary = null!;
    private Warehouse _warehouse = null!;
    private DataShelf<Payload> _shelf = null!;
    private DataHandle[] _handles = null!;
    private int _cursor;

    [GlobalSetup]
    public void Setup()
    {
        _array = new Payload[RecordCount];
        _dictionary = new Dictionary<int, Payload>(RecordCount);
        _warehouse = new Warehouse();
        _shelf = _warehouse.CreateShelf<Payload>(RecordCount);
        _handles = new DataHandle[RecordCount];

        for (var i = 0; i < RecordCount; i++)
        {
            var value = new Payload { A = i * 17L, B = i * 31L, State = i };
            _array[i] = value;
            _dictionary.Add(i, value);
            if (!_shelf.TryAllocate(out _handles[i]))
                throw new InvalidOperationException("Unexpected shelf exhaustion during setup.");
            _shelf.GetRef(_handles[i]) = value;
            _shelf.MarkClean(_handles[i]);
        }
    }

    [Benchmark(Baseline = true, Description = "DWH-ACCESS-READ-ARRAY")]
    public long ArrayRead()
    {
        var index = NextIndex();
        return _array[index].A;
    }

    [Benchmark(Description = "DWH-ACCESS-READ-DICTIONARY")]
    public long DictionaryRead()
    {
        var index = NextIndex();
        return _dictionary[index].A;
    }

    [Benchmark(Description = "DWH-ACCESS-READ-DATASHELF")]
    public long DataShelfRead()
    {
        var index = NextIndex();
        return _shelf.Read(_handles[index]).A;
    }

    [GlobalCleanup]
    public void Cleanup() => _warehouse.Dispose();

    private int NextIndex()
    {
        var index = _cursor++;
        if (_cursor == RecordCount) _cursor = 0;
        return index;
    }

    public struct Payload
    {
        public long A;
        public long B;
        public int State;
    }
}

[MemoryDiagnoser]
public class AccessWriteBenchmarks
{
    private const int RecordCount = 65_536;
    private Payload[] _array = null!;
    private Dictionary<int, Payload> _dictionary = null!;
    private Warehouse _warehouse = null!;
    private DataShelf<Payload> _shelf = null!;
    private DataHandle[] _handles = null!;
    private int _cursor;

    [GlobalSetup]
    public void Setup()
    {
        _array = new Payload[RecordCount];
        _dictionary = new Dictionary<int, Payload>(RecordCount);
        _warehouse = new Warehouse();
        _shelf = _warehouse.CreateShelf<Payload>(RecordCount);
        _handles = new DataHandle[RecordCount];

        for (var i = 0; i < RecordCount; i++)
        {
            var value = new Payload { A = i * 17L, B = i * 31L, State = i };
            _array[i] = value;
            _dictionary.Add(i, value);
            if (!_shelf.TryAllocate(out _handles[i]))
                throw new InvalidOperationException("Unexpected shelf exhaustion during setup.");
            _shelf.GetRef(_handles[i]) = value;
            _shelf.MarkClean(_handles[i]);
        }
    }

    [Benchmark(Baseline = true, Description = "DWH-ACCESS-WRITE-ARRAY")]
    public void ArrayWrite()
    {
        var index = NextIndex();
        _array[index].State++;
    }

    [Benchmark(Description = "DWH-ACCESS-WRITE-DICTIONARY")]
    public void DictionaryWrite()
    {
        var index = NextIndex();
        var value = _dictionary[index];
        value.State++;
        _dictionary[index] = value;
    }

    [Benchmark(Description = "DWH-ACCESS-WRITE-DATASHELF-DIRTY-ROUNDTRIP")]
    public void DataShelfWrite()
    {
        var index = NextIndex();
        var handle = _handles[index];
        _shelf.GetRef(handle).State++;
        // Reset in the measured method so every invocation starts the next one clean.
        // This intentionally measures the full dirty-bit round trip plus the write.
        _shelf.MarkClean(handle);
    }

    [GlobalCleanup]
    public void Cleanup() => _warehouse.Dispose();

    private int NextIndex()
    {
        var index = _cursor++;
        if (_cursor == RecordCount) _cursor = 0;
        return index;
    }

    public struct Payload
    {
        public long A;
        public long B;
        public int State;
    }
}
