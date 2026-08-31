// See https://aka.ms/new-console-template for more information
using BenchmarkTest;

Console.WriteLine("Hello, World!");
BenchmarkDotNet.Running.BenchmarkRunner.Run<StringBuilderPoolTest>();

