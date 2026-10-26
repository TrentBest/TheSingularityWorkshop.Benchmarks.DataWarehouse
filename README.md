<p align="center">
  <a href="https://github.com/TrentBest/TheSingularityWorkshop.Visualize">
    <img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop" width="720" />
  </a>
</p>

# TheSingularityWorkshop.Benchmarks.DataWarehouse

**The performance laboratory for the Workshop's low-level DataWarehouse.**

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Status](https://img.shields.io/badge/status-baseline%20in%20progress-00A6A6)](#benchmark-catalog)

This repository measures the low-level [DataWarehouse](https://github.com/TrentBest/TheSingularityWorkshop.DataWarehouse) reconstruction. It is deliberately separate from the higher-level AEC-facing [SingularityWarehouse](https://github.com/TrentBest/SingularityWarehouse).

## Benchmark rules

- Keep benchmark names stable and specific; historical results must remain attributable to a workload.
- Separate reads, writes, allocation, reclamation, dirty tracking, and scaling instead of blending them into one headline number.
- Use BenchmarkDotNet in Release mode and preserve raw reports as CI artifacts.
- Treat results as measurements of the named workload and machine—not universal performance claims.
- Compare equivalent operations. A direct array access is a lower-bound control, not a claim that arrays and handle-validated storage have identical semantics.
- Do not claim a speedup until repeated runs on comparable environments support it.

## Benchmark catalog

| Benchmark ID | What it measures | Current comparison |
|---|---|---|
| `AccessReadBenchmarks.ArrayRead` | Sequential record read control | Array, dictionary, DataShelf handle |
| `AccessWriteBenchmarks.ArrayWrite` | Sequential record write control | Array, dictionary, DataShelf handle plus dirty bit |
| `LifecycleBenchmarks.DataShelfAllocateAndFree` | Slot allocation and reclamation | DataShelf free-stack path versus dictionary add/remove |

The access suite uses 65,536 pre-created records and cycles through them sequentially. Setup and teardown are outside the measured operations. Read and write comparisons have separate baselines so ratios are meaningful within each operation category.

## Run locally

Requires the .NET 8 SDK and both repositories checked out as sibling directories:

```text
workspace/
├── TheSingularityWorkshop.DataWarehouse/
└── TheSingularityWorkshop.Benchmarks.DataWarehouse/
```

Run all benchmarks:

```sh
dotnet run --project TheSingularityWorkshop.Benchmarks.DataWarehouse.csproj -c Release
```

Filter to a suite:

```sh
dotnet run --project TheSingularityWorkshop.Benchmarks.DataWarehouse.csproj -c Release -- --filter '*AccessReadBenchmarks*'
dotnet run --project TheSingularityWorkshop.Benchmarks.DataWarehouse.csproj -c Release -- --filter '*LifecycleBenchmarks*'
```

The project reference resolves to the sibling DataWarehouse checkout. This is intentional while the library is being reconstructed; no NuGet package is published or required.

## Automated reports

The GitHub Actions workflow runs on demand and when relevant files change on `development`. It checks out both repositories, runs the suite in Release mode, and uploads BenchmarkDotNet reports as an artifact. Reports are evidence for review; this initial harness does not yet enforce performance thresholds.

## Next benchmark areas

1. Capacity scaling: 1, 64, 1K, 64K, and million-slot shelves.
2. Random versus sequential access, and hot versus cold working sets.
3. Allocation/free churn and fragmentation behavior.
4. Dirty-bit marking, clean/reset costs, and dirty-slot enumeration density.
5. Page alignment and record-size effects.
6. Allocation count and managed bytes on the steady-state path.
7. Handle validation overhead, with a direct pointer/index control.
8. Multiple shelves and independent data types.

These are planned benchmark categories, not measured results.

<p align="center">
  <img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/Avatar.jpg" alt="The Singularity Workshop avatar" width="96" />
</p>
