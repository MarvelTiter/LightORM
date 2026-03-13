using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Interfaces;

/// <summary>
/// 处理JSON列的序列化和反序列化
/// </summary>
public interface ILightJsonHelper
{
    void Serialize<T>(T value);
    T? Deserialize<T>(string json);
}
