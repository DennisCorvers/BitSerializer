using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace Bench
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = DefaultConfig.Instance.With(ConfigOptions.DisableOptimizationsValidator);
            BenchmarkRunner.Run<SerializeBench>(config);
            Console.ReadLine();
        }
    }
}
