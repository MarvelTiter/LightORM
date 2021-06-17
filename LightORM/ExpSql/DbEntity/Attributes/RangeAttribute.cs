using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDbEntity.Attributes
{
    public class RangeAttribute : ValidateAttribute
    {
        private readonly int max;
        private readonly int min;

        public RangeAttribute(int Min = 0, int Max = 10)
        {
            max = Max;
            min = Min;
        }

        public override bool Check(object value)
        {
            var v = Convert.ToInt32(value);
            var result = v >= min && v <= max;
            return result;
        }

        public override string ErrorMsg()
        {
            return $"不在范围({min}~{max})内";
        }
    }
}
