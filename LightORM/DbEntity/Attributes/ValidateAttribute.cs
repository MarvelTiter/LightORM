using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDbEntity.Attributes
{
    public abstract class ValidateAttribute : Attribute, IValidateAttribute
    {
        public abstract bool Check(object value);

        public abstract string ErrorMsg();
    }
}
