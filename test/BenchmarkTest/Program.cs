// See https://aka.ms/new-console-template for more information
using BenchmarkTest;
using LightORM.Providers.Sqlite.Extensions;

Console.WriteLine("Hello, World!");
BenchmarkDotNet.Running.BenchmarkRunner.Run<SqlBuild>();

