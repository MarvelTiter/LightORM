using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Performances;

internal struct SlimList<T>
{
    private T _item0, _item1, _item2, _item3;
    private T[]? _overflow;
    private int _count;


    public void Add(T item)
    {
        if (_count < 4)
        {
            switch (_count)
            {
                case 0:
                    _item0 = item;
                    break;
                case 1:
                    _item1 = item;
                    break;
                case 2:
                    _item2 = item;
                    break;
                case 3:
                    _item3 = item;
                    break;
                default:
                    break;
            }
        }
        else
        {
            // 溢出到数组
            _overflow ??= new T[8];
            if (_count - 4 >= _overflow.Length)
                Array.Resize(ref _overflow, _overflow.Length * 2);
            _overflow[_count - 4] = item;
        }
        _count++;
    }


    public int Count => _count;

    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= _count)
                throw new IndexOutOfRangeException();
            return index switch
            {
                < 4 => index switch
                {
                    0 => _item0,
                    1 => _item1,
                    2 => _item2,
                    3 => _item3,
                    _ => throw new InvalidOperationException()
                },
                _ => _overflow![index - 4]
            };
        }
    }
}
