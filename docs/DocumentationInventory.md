# Documentation Inventory — DataWarehouse Benchmarks

This inventory follows the Workshop's FSM_COS documentation principles while adapting the content to a measurement laboratory. The goal is reproducible evidence, not performance theater.

## Scope and evidence labels

- **Implemented** — present in the current repository and linked to source or workflow.
- **Source-checked** — conceptual view checked against the current harness.
- **Planned** — useful future coverage, not a claim that a benchmark exists.
- **Not applicable** — deliberately excluded for the current repository boundary.

## Inventory

| View / artifact | Status | Purpose / evidence |
|---|---|---|
| README / run instructions | Implemented | Identity, rules, current benchmark catalog, commands, limits |
| Benchmark identity catalog | Partial | Current benchmark methods are named; stable machine-readable metadata and historical index are planned |
| Per-benchmark methodology pages | Planned | Operation, setup, inputs, baseline, included costs, interpretation |
| Benchmark workflow | Implemented | Runs suite in Release mode and uploads BenchmarkDotNet reports |
| Result history / deltas | Planned | Comparable results indexed by benchmark ID, commit, runtime, and environment |
| Capacity-scaling charts | Planned | Visualize growth across record counts |
| Allocation / dirty-tracking studies | Planned | Isolate metadata and lifecycle costs |
| Sequence / activity diagrams | Partial | README conceptual measurement pipeline; detailed setup/measurement boundaries planned |
| Class/type diagrams | Not applicable to the benchmark product goal | Source types are covered by the library's own documentation |
| Test traceability | Planned | Map benchmark identities to the behavior or hypothesis each probes |
| Complexity and measurement caveats | Partial | README separates algorithmic expectations from measured wall-clock evidence |
| Regression thresholds / CI enforcement | Planned | Current harness reports but does not gate on performance deltas |
| Automated chart generation / stale-output gate | Planned | No automated documentation generator is claimed today |
| Deployment diagram | Not applicable | Benchmark project is a local/CI executable |
| MicroBundle views | Not applicable | No MicroBundle dependency or contract |

## Evidence contract

Every published result should preserve or identify:

1. Stable benchmark ID and exact operation measured.
2. Input size, access pattern, setup/teardown boundaries, and baseline.
3. Runtime, hardware/runner details, build configuration, and source revision.
4. Raw BenchmarkDotNet report and the comparison method.
5. Known limitations, noise, and any non-equivalent semantics between implementations.

A green workflow is evidence that the configured job completed. It is not, by itself, evidence of speedup, scalability, or absence of regressions.

## Branding

Use the shared Workshop moniker at the top of primary Markdown documents and the canonical Workshop footer image:

`https://raw.githubusercontent.com/TrentBest/FSM_COS/master/Documentation/Branding/TheSingularityWorkshop.png`

Keep the benchmark visual distinct from the library's memory diagram: it should show a traceable path from question to named workload to measurement to artifact and review.
