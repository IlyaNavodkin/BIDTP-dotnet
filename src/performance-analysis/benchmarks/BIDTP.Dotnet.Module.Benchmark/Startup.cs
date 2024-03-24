using BenchmarkDotNet.Running;

namespace BIDTP.Dotnet.Benchmark;

public class Startup
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<SendMessagesBenchmark>();
            // Recursion Benchmark :(
            // BenchmarkRunner.Run<ConnectToServerBenchmark>();
        }
    }
}