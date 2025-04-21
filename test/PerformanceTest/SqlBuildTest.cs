using BenchmarkDotNet.Attributes;
using System.Text;

namespace PerformanceTest;

public class SqlBuildTest
{
    [Benchmark]
    public string UseStringBuilder()
    {
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < 100; i++)
        {
            builder.Append("Hello World!\n");
        }
        return builder.ToString();
    }

    [Benchmark]
    public string UseList()
    {
        List<string> list = new List<string>();
        for (int i = 0; i < 100; i++)
        {
            list.Add("Hello World!\n");
        }
        return string.Join("", list);
    }
    [Benchmark]
    public string UseStream()
    {
        using StringWriter sw = new StringWriter();
        for (int i = 0; i < 100; i++)
        {
            sw.Write("Hello World!\n");
        }
        return sw.ToString();
    }




}
