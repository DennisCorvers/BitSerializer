using System;
using System.Numerics;
using BenchmarkDotNet;
using BenchmarkDotNet.Running;
using BlittableSerializer;

namespace Bench
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<ReadBench>();
            Console.ReadLine();
        }
    }
}
