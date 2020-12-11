using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DExpSql.ExpressionMethod {
    public static class Db {
        public static bool Like(this object self, string keyWord) {
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

        public static object Sum( Expression<Func<bool>> exp) {//this object self,
            return null;
        }
    }
}
