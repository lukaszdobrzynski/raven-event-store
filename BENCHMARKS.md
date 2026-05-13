# Benchmarks

```
BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8457/25H2/2025Update/HudsonValley2)
AMD Ryzen 9 5900HX with Radeon Graphics 3.30GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.202
  [Host] : .NET 8.0.26 (8.0.26, 8.0.2626.16921), X64 RyuJIT x86-64-v3
```

## Append

Appending a batch of events to an existing stream. `EventsInStream` is the number of events already in the head slice (no prior slicing); `EventsToAppend` is the batch size.

The dominant cost is the size of the head slice document, not the batch being appended — existing events are never loaded on the client, but the server must process the full document to apply the patch. Latency grows roughly 4× when the head slice grows from 1 000 to 10 000 events. Slicing streams regularly keeps this cost flat regardless of total stream length.

`InvocationCount=1  UnrollFactor=1`

| Method              | EventsToAppend | EventsInStream | Mean      | Error     | StdDev    | Allocated |
|-------------------- |--------------- |--------------- |----------:|----------:|----------:|----------:|
| **AppendAndStore**      | **1**              | **1000**           |  **6.543 ms** | **0.4053 ms** | **1.1630 ms** |  **0.09 MB** |
| AppendAndStoreAsync | 1              | 1000           |  5.828 ms | 0.1652 ms | 0.4712 ms |  0.20 MB |
| **AppendAndStore**      | **1**              | **10000**          | **26.412 ms** | **1.0304 ms** | **2.9894 ms** |  **0.20 MB** |
| AppendAndStoreAsync | 1              | 10000          | 27.311 ms | 1.3402 ms | 3.7580 ms |  0.09 MB |
| **AppendAndStore**      | **10**             | **1000**           |  **6.415 ms** | **0.3467 ms** | **0.9666 ms** |  **0.44 MB** |
| AppendAndStoreAsync | 10             | 1000           |  6.114 ms | 0.2253 ms | 0.6242 ms |  0.30 MB |
| **AppendAndStore**      | **10**             | **10000**          | **26.447 ms** | **0.5287 ms** | **1.2564 ms** |  **0.43 MB** |
| AppendAndStoreAsync | 10             | 10000          | 29.142 ms | 1.5019 ms | 4.3573 ms |  0.42 MB |
| **AppendAndStore**      | **100**            | **1000**           | **12.089 ms** | **0.6317 ms** | **1.7817 ms** |  **2.79 MB** |
| AppendAndStoreAsync | 100            | 1000           | 12.615 ms | 0.5795 ms | 1.6719 ms |  2.69 MB |
| **AppendAndStore**      | **100**            | **10000**          | **33.831 ms** | **1.2490 ms** | **3.5229 ms** |  **2.68 MB** |
| AppendAndStoreAsync | 100            | 10000          | 30.882 ms | 0.5045 ms | 0.9839 ms |  2.69 MB |

## Slice

Creating a new slice from an existing stream. `EventsInSourceStream` is the number of events in the current head slice; `EventsInNewSlice` is the number of events written into the new slice; `SliceChainDepth` is the number of existing prior slices.

Latency scales with the source slice size for the same reason as append — the server patches the old head document. Allocation tracks only the new slice events and is unaffected by how large the source slice is, confirming that existing events are never loaded on the client.

`InvocationCount=1  UnrollFactor=1`

| Method             | EventsInNewSlice | EventsInSourceStream | SliceChainDepth | Mean      | Error     | StdDev    | Allocated |
|------------------- |----------------- |--------------------- |---------------- |----------:|----------:|----------:|----------:|
| **SliceAndStore**      | **10**               | **1000**                 | **10**              |  **6.121 ms** | **0.1220 ms** | **0.2923 ms** |  **0.49 MB** |
| SliceAndStoreAsync | 10               | 1000                 | 10              |  8.887 ms | 1.3249 ms | 3.7152 ms |  0.49 MB |
| **SliceAndStore**      | **10**               | **10000**                | **10**              | **24.880 ms** | **1.1740 ms** | **3.3683 ms** |  **0.49 MB** |
| SliceAndStoreAsync | 10               | 10000                | 10              | 23.823 ms | 0.9965 ms | 2.8430 ms |  0.49 MB |
| **SliceAndStore**      | **100**              | **1000**                 | **10**              |  **9.914 ms** | **0.1954 ms** | **0.3319 ms** |  **3.32 MB** |
| SliceAndStoreAsync | 100              | 1000                 | 10              | 10.196 ms | 0.2027 ms | 0.4001 ms |  3.32 MB |
| **SliceAndStore**      | **100**              | **10000**                | **10**              | **27.676 ms** | **0.7501 ms** | **2.1034 ms** |  **3.32 MB** |
| SliceAndStoreAsync | 100              | 10000                | 10              | 27.509 ms | 0.6935 ms | 1.8985 ms |  3.33 MB |

## Version traversal

Replaying aggregate state at a specific historical version. `EventsPerSlice` is the number of events in each slice; `SliceCount` is the total number of slices in the chain. The target version is fixed near the start of the chain (slice 2), exercising the binary-search slice lookup and seed-based replay.

The target slice is located via binary search, so doubling `SliceCount` from 500 to 1 000 adds only ~25% to latency. The number of events per slice is the dominant factor — it determines how many events must be deserialized and replayed through the aggregate. At 100 events per slice the cost stays under 2.5 ms even across a 1 000-slice chain.

| Method                     | EventsPerSlice | SliceCount | Mean     | Error     | StdDev    | Allocated |
|--------------------------- |--------------- |----------- |---------:|----------:|----------:|----------:|
| **GetAggregateAtVersion**      | **100**            | **500**        | **1.843 ms** | **0.0045 ms** | **0.0038 ms** |   **1.12 MB** |
| GetAggregateAtVersionAsync | 100            | 500        | 1.750 ms | 0.0312 ms | 0.0277 ms |   1.22 MB |
| **GetAggregateAtVersion**      | **100**            | **1000**       | **2.325 ms** | **0.0180 ms** | **0.0169 ms** |   **1.76 MB** |
| GetAggregateAtVersionAsync | 100            | 1000       | 2.336 ms | 0.0467 ms | 0.0624 ms |   1.84 MB |
| **GetAggregateAtVersion**      | **1000**           | **500**        | **6.453 ms** | **0.1260 ms** | **0.1294 ms** |   **5.67 MB** |
| GetAggregateAtVersionAsync | 1000           | 500        | 5.908 ms | 0.0656 ms | 0.0512 ms |   5.77 MB |
| **GetAggregateAtVersion**      | **1000**           | **1000**       | **6.819 ms** | **0.0447 ms** | **0.0418 ms** |   **6.31 MB** |
| GetAggregateAtVersionAsync | 1000           | 1000       | 6.775 ms | 0.0927 ms | 0.0868 ms |    6.4 MB |

## Time traversal

Replaying aggregate state at a specific historical timestamp. Same setup as version traversal — the target timestamp is fixed near the start of the chain (slice 2), exercising the binary-search slice lookup and seed-based replay.

Results are near-identical to version traversal, reflecting that both operations follow the same path — binary search on the slice index, single slice load, seed-based replay — differing only in the field used for lookup.

| Method              | EventsPerSlice | SliceCount | Mean     | Error     | StdDev    | Allocated |
|-------------------- |--------------- |----------- |---------:|----------:|----------:|----------:|
| **GetAggregateAt**      | **100**            | **500**        | **1.864 ms** | **0.0059 ms** | **0.0052 ms** |   **1.12 MB** |
| GetAggregateAtAsync | 100            | 500        | 1.751 ms | 0.0106 ms | 0.0094 ms |   1.21 MB |
| **GetAggregateAt**      | **100**            | **1000**       | **2.281 ms** | **0.0076 ms** | **0.0063 ms** |   **1.75 MB** |
| GetAggregateAtAsync | 100            | 1000       | 2.287 ms | 0.0226 ms | 0.0200 ms |   1.84 MB |
| **GetAggregateAt**      | **1000**           | **500**        | **6.264 ms** | **0.0567 ms** | **0.0530 ms** |   **5.67 MB** |
| GetAggregateAtAsync | 1000           | 500        | 6.082 ms | 0.0468 ms | 0.0391 ms |   5.77 MB |
| **GetAggregateAt**      | **1000**           | **1000**       | **6.795 ms** | **0.0750 ms** | **0.0702 ms** |   **6.31 MB** |
| GetAggregateAtAsync | 1000           | 1000       | 6.724 ms | 0.0343 ms | 0.0287 ms |   6.41 MB |
