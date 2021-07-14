using MDbContext.NewExpSql.SqlFragment;
using System;
using System.Collections.Generic;

namespace MDbContext.NewExpSql {
    public enum Position {
        Left,
        Right
    }

    public enum LikeMode {
        None,
        Like,
        LeftLike,
        RightLike,
    }
    internal interface ISqlContext {
        Position Position { get; set; }
        LikeMode LikeMode { get; set; }
        int WhereIndex { get; set; }
        string BuildSql(out Dictionary<string, object> keyValues);
        bool SetTableAlias(Type tableName);
        string GetTableAlias(Type tableName);
        string GetTableName(bool withAlias, Type t = null);
        string AddDbParameter(object parameterValue);
        string GetPrefix();
        void AddFragment<F>(F fragment) where F:BaseFragment;
    }
}
