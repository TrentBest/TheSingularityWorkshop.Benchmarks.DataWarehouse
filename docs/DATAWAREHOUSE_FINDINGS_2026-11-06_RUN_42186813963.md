# DataWarehouse Benchmark Findings — 2026-11-06, Run 42186813963

> **Evidence status:** one complete hosted BenchmarkDotNet run. These measurements are exploratory observations from one virtualized runner, not a stable cross-run baseline or a performance verdict.

- Workflow: [DataWarehouse Benchmarks — run 42186813963](https://github.com/TrentBest/TheSingularityWorkshop.Benchmarks.DataWarehouse/actions/runs/42186813963)
- Raw artifacts and environment manifest: [datawarehouse-benchmark-results](https://github.com/TrentBest/TheSingularityWorkshop.Benchmarks.DataWarehouse/actions/runs/42186813963/artifacts/13488036995)
- Artifact SHA-256: `96cb6a96fe4f299918250a5e4c5e25617f3a2fda8660eb90760106b81c8329df`
- Benchmark harness commit: `19a0ac7cee2da031b6560eade10f9d0a5032698b`
- DataWarehouse source commit: `a019d006dbb2eb4cb525cd3977c9eafaf6eee616`
- Tracking: [DataWarehouse benchmark repository](https://github.com/TrentBest/TheSingularityWorkshop.Benchmarks.DataWarehouse)

## 1. Environment and method

- BenchmarkDotNet 0.15.2
- .NET SDK 10.0.403; benchmark runtime .NET 8.0.33
- Ubuntu 24.04.5 LTS
- AMD EPYC 7763 virtualized runner; 1 CPU allocation, 4 logical and 2 physical cores reported
- Default BenchmarkDotNet job; 13–15 retained observations per case after outlier handling
- 15 benchmark cases executed in approximately 4 minutes 36 seconds
- MemoryDiagnoser enabled; no managed allocation was reported per operation in these steady-state cases
- Fixtures and setup are excluded from timed methods unless explicitly described

The CPU model and single-CPU virtualized allocation limit comparisons to this hosted environment. The sub-nanosecond array control means are amortized BenchmarkDotNet measurements, not claims that a single array access completes in less than one physical CPU cycle.

## 2. Sequential access

| Stable ID | Mean | Relative to array control |
|---|---:|---:|
| DWH-ACCESS-READ-ARRAY | 0.702 ns | 1.00× |
| DWH-ACCESS-READ-DICTIONARY | 4.910 ns | 6.99× |
| DWH-ACCESS-READ-DATASHELF | 12.677 ns | 18.06× |

The DataShelf path is not a raw pointer read: it validates shelf identity, slot bounds, generation, and liveness before resolving the record. The dictionary control performs a dictionary lookup but does not provide the same stale-handle protection. This result identifies the cost of the current public safety contract; it does not establish that DataShelf is a drop-in replacement for an array or dictionary in every workload.

## 3. Sequential writes

| Stable ID | Mean | Relative to array control |
|---|---:|---:|
| DWH-ACCESS-WRITE-ARRAY | 0.918 ns | 1.00× |
| DWH-ACCESS-WRITE-DICTIONARY | 11.218 ns | 12.22× |
| DWH-ACCESS-WRITE-DATASHELF-DIRTY-ROUNDTRIP | 18.235 ns | 19.86× |

The DataShelf write case performs a validated mutable-handle access, which marks the slot dirty, followed by MarkClean to reset the state for the next invocation. It measures a record mutation plus a complete dirty-bit transition round trip. It is intentionally not presented as equivalent to the array write control.

## 4. Dirty-slot enumeration

The shelf capacity is 65,536 slots; dirty density is configured before measurement.

| Dirty density | Mean |
|---:|---:|
| 0% | 677.8 ns |
| 1% | 1.651 μs |
| 50% | 34.518 μs |
| 100% | 68.394 μs |

The result rises approximately with the number of dirty slots. The 100% case is about 101× the 0% case, while moving from 50% to 100% is approximately 1.98×. This is consistent with the bitmap enumeration loop visiting set bits rather than scanning every slot individually, but a larger capacity sweep and independent runs are needed before drawing a scaling conclusion.

## 5. Lifecycle and tracking

| Stable ID | Mean |
|---|---:|
| DWH-LIFECYCLE-ALLOCATE-FREE | 24.230 ns |
| DWH-LIFECYCLE-DICTIONARY-ADD-REMOVE | 13.805 ns |
| DWH-TRACKING-IS-ALIVE | 4.426 ns |
| DWH-TRACKING-IS-DIRTY | 12.803 ns |
| DWH-TRACKING-DIRTY-ROUNDTRIP | 18.384 ns |

The DataShelf lifecycle includes free-stack bookkeeping, alive/dirty state, and generation handling; dictionary add/remove is a comparison control with different semantics. The two are not equivalent lifecycle implementations.

IsAlive is materially cheaper than IsDirty in this API because IsDirty uses the full throwing handle validation path before reading the dirty bitmap. This is a useful optimization question only if a faster API can preserve the intended safety contract; do not remove validation merely to improve a microbenchmark.

## 6. What this run establishes—and what it does not

### Supported by this artifact

- The full configured suite built and executed all 15 benchmark cases.
- The current public DataShelf read path costs more than the array and dictionary controls in this workload, while providing generation-checked handle validation.
- The dirty-bit write case includes the dirty transition and reset on every measured invocation.
- Dirty-slot enumeration time increases with configured dirty density.
- These steady-state operations reported no managed allocations per operation.

### Not established

- Cross-run repeatability, a regression threshold, or a universal DataShelf performance ranking.
- Total memory cost: MemoryDiagnoser does not account for unmanaged DataLake allocations.
- Cold allocation, page alignment, reclamation churn under fragmentation, or multi-shelf contention.
- Random/hot/cold access patterns, larger capacity scaling, or behavior under concurrent readers/writers.
- Whether a faster unchecked access path is desirable; that is a contract and safety decision, not merely a performance choice.

## 7. Next experiments

1. Add explicit native-memory accounting measurements using DataLake.AllocatedBytes alongside managed allocation diagnostics.
2. Add capacity sweeps for 1, 64, 1K, 64K, and million-slot shelves.
3. Compare sequential, randomized, hot-set, and cold-set access while retaining the same handle-validity guarantees.
4. Separate validated handle resolution from payload read/write cost without inventing an unsafe public API.
5. Repeat this exact workload on independent hosted runs and a known local machine before setting any regression gates.

---

**Interpretation:** this first measured run gives us a useful cost map, not a verdict. It shows exactly where the DataShelf contract costs time and where the dirty bitmap behaves as intended. Preserve the safety semantics, add the missing unmanaged-memory and capacity measurements, then repeat under controlled hardware.
