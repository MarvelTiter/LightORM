using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDbEntity.Attributes
{
    internal interface IValidateAttribute
    {
        bool Check(object value);
        string ErrorMsg();
    }
}
