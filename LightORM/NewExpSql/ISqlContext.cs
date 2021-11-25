using MDbContext.Extension;
using MDbContext.NewExpSql.SqlFragment;
using MDbEntity.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDbContext.NewExpSql
{
    public enum Position
    {
        None,
        Left,
        Right
    }

    public enum LikeMode
    {
        None,
        Like,
        LeftLike,
        RightLike,
    }
    internal interface ISqlContext
    {
        string BuildSql(out Dictionary<string, object> keyValues);
        void AddFragment<F>(F fragment) where F : BaseFragment;
    }
}
