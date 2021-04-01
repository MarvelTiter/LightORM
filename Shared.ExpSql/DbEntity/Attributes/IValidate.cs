using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDbEntity.Attributes
{
    public interface IValidate<T>
    {
        List<string> ValidateMsg { get; }
        bool Validate(T entity);
    }
}
