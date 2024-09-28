// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using PerformanceTest;
BenchmarkRunner.Run<SqlBuildTest>();

Console.ReadKey();
