using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DExpSql {
    public static class Fn {
        public static bool Like(this object self, string keyWord) {
            return true;
        }

        public static bool NotLike(this object self, string keyWord) {
            return true;
        }

        public static bool LeftLike(this object self, string keyWord) {
            return true;
        }

        public static bool RightLike(this object self, string keyWord) {
            return true;
        }

        public static bool In(this object self, params object[] array) {
            return true;
        }

        public static int Sum(Expression<Func<bool>> exp) {//this object self,
            return 0;
        }

        public static int Count(Expression<Func<bool>> exp) {
            return 0;
        }

        public static int GroupConcat(Expression<Func<object>> exp) {
            return 0;
        }
    }
}
