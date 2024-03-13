using LightORM.ExpressionSql.Interface.Select;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.ExpressionSql.Interface;

internal interface ISelectColumns
{
    IExpSelect0 Columns(Expression body);
}
