using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MDbEntity.Attributes
{
    public class MaxLengthAttribute :ValidateAttribute
    {
        private readonly int length;

        public MaxLengthAttribute(int length)
        {
            this.length = length;
        }
        public override bool Check(object value)
        {
            return value.ToString().Length <= length;
        }

        public override string ErrorMsg()
        {
            return "超出最大长度限制";
        }
    }
}
