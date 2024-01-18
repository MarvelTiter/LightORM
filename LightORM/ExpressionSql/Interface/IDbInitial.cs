using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MDbContext.DbStruct;

namespace MDbContext.ExpressionSql.Interface
{
    public interface IDbInitial
    {
        IDbInitial CreateTable<T>(string key = ConstString.Main, params T[]? datas);
        IDbInitial Configuration(Action<TableGenerateOption> option);
        string GenerateCreateSql<T>(string key = ConstString.Main);
    }
}
