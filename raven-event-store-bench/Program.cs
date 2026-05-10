using BenchmarkDotNet.Running;
using Raven.EventStore.Bench.Benchmarks;

namespace Raven.EventStore.Bench;

public class Program
{
    static void Main(string[] args)
    {
        BenchmarkRunner.Run<VersionTraversal>();
        BenchmarkRunner.Run<TimeTraversal>();
        BenchmarkRunner.Run<Append>();
        BenchmarkRunner.Run<Slice>();
    }
}