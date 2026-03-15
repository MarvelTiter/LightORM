using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Models;

public readonly struct Indexer
{
    private readonly int? intIndex;
    private readonly string? stringIndex;
    private readonly int valueIndex = -1;
    private readonly Stack<Indexer> indexers = [];
    public Indexer(int index)
    {
        intIndex = index;
        valueIndex = 0;
        indexers.Push(this);
    }

    public Indexer(string index)
    {
        stringIndex = index;
        valueIndex = 1;
        indexers.Push(this);
    }

    public readonly int Count => indexers?.Count ?? 0;

    public readonly object Value => valueIndex switch
    {
        0 => intIndex!.Value,
        1 => stringIndex!,
        _ => throw new Exception("Indexer value error")
    };

    public readonly bool IsIntValue => valueIndex == 0;

    public readonly bool IsStringValue => valueIndex == 1;

    public readonly int IntValue => intIndex ?? throw new Exception("Indexer value error");
    public readonly string StringValue => stringIndex ?? throw new Exception("Indexer value error");
    public readonly bool HasValue => intIndex.HasValue || stringIndex is not null;
    public void Combine(Indexer other)
    {
        this.indexers.Push(other);
    }

    public void Format(Action<Indexer> action)
    {
        if (indexers.Count == 1)
        {
            action(this);
            return;
        }

        while(indexers.Count > 0)
        {
            var indexer = indexers.Pop();
            action(indexer);
        }
    }

    public static implicit operator Indexer(int value) => new(value);
    public static implicit operator Indexer(string value) => new(value);
}

public readonly record struct MemberPathInfo(MemberInfo Member)
{
    public Indexer IndexValue { get; init; }
}
