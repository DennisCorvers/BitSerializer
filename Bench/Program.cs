using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using BitSerializer;

namespace Bench
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<SerializeBench>();
            Console.ReadLine();
        }
    }
}
