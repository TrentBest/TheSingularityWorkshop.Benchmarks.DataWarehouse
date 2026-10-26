using BenchmarkDotNet.Attributes;
using TheSingularityWorkshop.DataWarehouse;

namespace TheSingularityWorkshop.Benchmarks.DataWarehouse.Benchmarks;

[MemoryDiagnoser]
public class AccessBenchmarks
{
    private const int RecordCount = 65_536;
    private Payload[] _array = null!;
    private Dictionary<int, Payload> _dictionary = null!;
    private DataWarehouse _warehouse = null!;
    private DataShelf<Payload> _shelf = null!;
    private DataHandle[] _handles = null!;
    private int _cursor;

    [GlobalSetup]
    public void Setup()
    {
        _array = new Payload[RecordCount];
        _dictionary = new Dictionary<int, Payload>(RecordCount);
        _warehouse = new DataWarehouse();
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

    [Benchmark(Baseline = true, Description = "Array.Read.Sequential")]
    public long ArrayRead()
    {
        var index = NextIndex();
        return _array[index].A;
    }

    [Benchmark(Description = "Dictionary.Read.Sequential")]
    public long DictionaryRead()
    {
        var index = NextIndex();
        return _dictionary[index].A;
    }

    [Benchmark(Description = "DataShelf.Read.Handle")]
    public long DataShelfRead()
    {
        var index = NextIndex();
        return _shelf.Read(_handles[index]).A;
    }

    [Benchmark(Baseline = true, Description = "Array.Write.Sequential")]
    public void ArrayWrite()
    {
        var index = NextIndex();
        _array[index].State++;
    }

    [Benchmark(Description = "Dictionary.Write.Sequential")]
    public void DictionaryWrite()
    {
        var index = NextIndex();
        var value = _dictionary[index];
        value.State++;
        _dictionary[index] = value;
    }

    [Benchmark(Description = "DataShelf.Write.HandleAndDirtyBit")]
    public void DataShelfWrite()
    {
        var index = NextIndex();
        _shelf.GetRef(_handles[index]).State++;
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
